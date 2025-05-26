using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Messaging.Publishers;

namespace ProductService.Services
{
    public partial class InventoryService
    {
        /// <summary>
        /// 創建商品預留
        /// </summary>
        /// <param name="request">預留請求</param>
        /// <returns>預留</returns>
        public async Task<Reservation> CreateReservationAsync(CreateReservationRequest request)
        {
            try
            {
                _logger?.LogInformation($"嘗試創建預留: OwnerId={request.OwnerId}, OwnerType={request.OwnerType}, 項目數量={request.Items.Count}");
                
                // 驗證預留項目
                if (request.Items == null || request.Items.Count == 0)
                {
                    _logger?.LogWarning("預留項目列表為空");
                    throw new ArgumentException("預留項目列表不能為空", nameof(request.Items));
                }
                
                // 檢查所有產品是否存在且庫存足夠
                foreach (var item in request.Items)
                {
                    var filter = Builders<Product>.Filter.Eq(p => p.Id, item.ProductId);
                    var product = await _dbContext.Products.Find(filter).FirstOrDefaultAsync();
                    
                    if (product == null)
                    {
                        _logger?.LogWarning($"找不到產品: {item.ProductId}");
                        throw new ArgumentException($"找不到產品: {item.ProductId}", nameof(item.ProductId));
                    }
                    // 檢查變體庫存或主產品庫存
                    if (item.VariantId != null)
                    {
                        var variant = product.Variants?.FirstOrDefault(v => v.VariantId == item.VariantId);
                        if (variant == null)
                        {
                            _logger?.LogWarning($"找不到產品變體: 產品={item.ProductId}, 變體={item.VariantId}");
                            throw new ArgumentException($"找不到產品變體: {item.VariantId}", nameof(item.VariantId));
                        }

                        if (variant.Stock < item.Quantity)
                        {
                            _logger?.LogWarning($"變體庫存不足: 產品={item.ProductId}, 變體={item.VariantId}, 可用={variant.Stock}, 請求={item.Quantity}");
                            throw new InvalidOperationException($"變體庫存不足: 可用={variant.Stock}, 請求={item.Quantity}");
                        }
                    }
                    else
            {
                        if (product.Stock.Available < item.Quantity)
                        {
                            _logger?.LogWarning($"庫存不足: 產品={item.ProductId}, 可用={product.Stock.Available}, 請求={item.Quantity}");
                            throw new InvalidOperationException($"庫存不足: 可用={product.Stock.Available}, 請求={item.Quantity}");
            }
        }
                }
        
                // 創建預留記錄
                var reservation = new Reservation
        {
                    OwnerId = request.OwnerId,
                    OwnerType = request.OwnerType,
                    Items = request.Items.Select(i => new ReservationItem
                    {
                        ProductId = i.ProductId,
                        VariantId = i.VariantId,
                        Quantity = i.Quantity
                    }).ToList(),
                    SessionId = request.SessionId,
                    UserId = request.UserId,
                    Status = "active",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(request.ExpirationMinutes ?? 15),
                    CreatedAt = DateTime.UtcNow
                };
                
                await _dbContext.Reservations.InsertOneAsync(reservation);
                
                // 更新各產品庫存
                foreach (var item in reservation.Items)
                {
                    // 更新產品庫存
                    if (item.VariantId != null)
            {
                        // 更新變體庫存
                        var filter = Builders<Product>.Filter.And(
                            Builders<Product>.Filter.Eq(p => p.Id, item.ProductId),
                            Builders<Product>.Filter.ElemMatch(p => p.Variants, v => v.VariantId == item.VariantId)
                        );
                        
                        var update = Builders<Product>.Update
                            .Inc("Variants.$.Stock", -item.Quantity);
                        
                        await _dbContext.Products.UpdateOneAsync(filter, update);
            }
                    else
        {
                        // 更新主產品庫存
                        var filter = Builders<Product>.Filter.Eq(p => p.Id, item.ProductId);
                        var update = Builders<Product>.Update
                            .Inc(p => p.Stock.Reserved, item.Quantity)
                            .Inc(p => p.Stock.Available, -item.Quantity);
                        
                        await _dbContext.Products.UpdateOneAsync(filter, update);
                    }
                    
                    // 記錄庫存變動
                    await CreateInventoryChangeAsync(
                        item.ProductId,
                        item.VariantId,
                        "reserve",
                        item.Quantity,
                        "庫存預留",
                        reservation.Id,
                        request.UserId
                    );
                }
                
                // 發布庫存預留事件
                if (_eventPublisher != null)
                {
                    await _eventPublisher.PublishInventoryReservedEventAsync(
                        reservation.Id,
                        request.OwnerId,
                        request.OwnerType,
                        reservation.Items.Select(i => new ReservationItemMessage
            {
                            ProductId = i.ProductId,
                            VariantId = i.VariantId,
                            Quantity = i.Quantity
                        }).ToList()
                    );
            }
                
                _logger?.LogInformation($"庫存預留成功: Id={reservation.Id}, OwnerId={request.OwnerId}, 項目數量={request.Items.Count}");
                return reservation;
        }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"創建預留時發生錯誤: OwnerId={request.OwnerId}, OwnerType={request.OwnerType}");
                throw;
    }
}
        
        /// <summary>
        /// 確認預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <param name="referenceId">相關單據ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> ConfirmReservationAsync(string id, string referenceId)
        {
            try
            {
                _logger?.LogInformation($"嘗試確認預留: Id={id}, ReferenceId={referenceId}");
                
                var filter = Builders<Reservation>.Filter.Eq(r => r.Id, id);
                var reservation = await _dbContext.Reservations.Find(filter).FirstOrDefaultAsync();
                
                if (reservation == null)
                {
                    _logger?.LogWarning($"找不到預留: {id}");
                    return false;
                }
                
                if (reservation.Status != "active")
                {
                    _logger?.LogWarning($"預留狀態不是活躍狀態: Id={id}, Status={reservation.Status}");
                    return false;
                }
                
                // 更新預留狀態
                var update = Builders<Reservation>.Update
                    .Set(r => r.Status, "used")
                    .Set(r => r.ReferenceId, referenceId);
                
                var result = await _dbContext.Reservations.UpdateOneAsync(filter, update);
                
                if (result.ModifiedCount == 0)
                {
                    _logger?.LogWarning($"確認預留失敗: {id}");
                    return false;
                }
                
                // 更新各產品庫存
                foreach (var item in reservation.Items)
                {
                    if (item.VariantId != null)
                    {
                        // 變體庫存已在預留時扣除，無需再次更新
                    }
                    else
                    {
                        // 更新主產品庫存 - 將預留數量從預留中減去
                        var productFilter = Builders<Product>.Filter.Eq(p => p.Id, item.ProductId);
                        var productUpdate = Builders<Product>.Update
                            .Inc(p => p.Stock.Reserved, -item.Quantity)
                            .Inc(p => p.Stock.Quantity, -item.Quantity);
                        
                        await _dbContext.Products.UpdateOneAsync(productFilter, productUpdate);
                    }
                    
                    // 記錄庫存變動
                    await CreateInventoryChangeAsync(
                        item.ProductId,
                        item.VariantId,
                        "decrement",
                        item.Quantity,
                        "確認預留",
                        referenceId,
                        reservation.UserId
                    );
                }
                
                _logger?.LogInformation($"確認預留成功: Id={id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"確認預留時發生錯誤: Id={id}");
                return false;
            }
        }
        
        /// <summary>
        /// 取消預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> CancelReservationAsync(string id)
        {
            try
            {
                _logger?.LogInformation($"嘗試取消預留: Id={id}");
                
                var filter = Builders<Reservation>.Filter.Eq(r => r.Id, id);
                var reservation = await _dbContext.Reservations.Find(filter).FirstOrDefaultAsync();
                
                if (reservation == null)
                {
                    _logger?.LogWarning($"找不到預留: {id}");
                    return false;
                }
                
                if (reservation.Status != "active")
                {
                    _logger?.LogWarning($"預留狀態不是活躍狀態: Id={id}, Status={reservation.Status}");
                    return false;
                }
                
                // 更新預留狀態
                var update = Builders<Reservation>.Update
                    .Set(r => r.Status, "cancelled");
                
                var result = await _dbContext.Reservations.UpdateOneAsync(filter, update);
                
                if (result.ModifiedCount == 0)
                {
                    _logger?.LogWarning($"取消預留失敗: {id}");
                    return false;
                }
                
                // 更新各產品庫存
                foreach (var item in reservation.Items)
                {
                    if (item.VariantId != null)
                    {
                        // 更新變體庫存 - 將預留數量釋放回可用庫存
                        var filter = Builders<Product>.Filter.And(
                            Builders<Product>.Filter.Eq(p => p.Id, item.ProductId),
                            Builders<Product>.Filter.ElemMatch(p => p.Variants, v => v.VariantId == item.VariantId)
                        );
                        
                        var update = Builders<Product>.Update
                            .Inc("Variants.$.Stock", item.Quantity);
                        
                        await _dbContext.Products.UpdateOneAsync(filter, update);
                    }
                    else
                    {
                        // 更新主產品庫存 - 將預留數量釋放回可用庫存
                        var productFilter = Builders<Product>.Filter.Eq(p => p.Id, item.ProductId);
                        var productUpdate = Builders<Product>.Update
                            .Inc(p => p.Stock.Reserved, -item.Quantity)
                            .Inc(p => p.Stock.Available, item.Quantity);
                        
                        await _dbContext.Products.UpdateOneAsync(productFilter, productUpdate);
                    }
                    
                    // 記錄庫存變動
                    await CreateInventoryChangeAsync(
                        item.ProductId,
                        item.VariantId,
                        "release",
                        item.Quantity,
                        "取消預留",
                        reservation.Id,
                        reservation.UserId
                    );
                }
                
                _logger?.LogInformation($"取消預留成功: Id={id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"取消預留時發生錯誤: Id={id}");
                return false;
            }
        }
        
        /// <summary>
        /// 清理過期預留
        /// </summary>
        /// <returns>清理的預留數量</returns>
        public async Task<int> CleanupExpiredReservationsAsync()
        {
            try
            {
                _logger?.LogInformation("嘗試清理過期預留");
                
                var now = DateTime.UtcNow;
                var filter = Builders<Reservation>.Filter.And(
                    Builders<Reservation>.Filter.Eq(r => r.Status, "active"),
                    Builders<Reservation>.Filter.Lt(r => r.ExpiresAt, now)
                );
                
                var expiredReservations = await _dbContext.Reservations.Find(filter).ToListAsync();
                
                if (expiredReservations.Count == 0)
                {
                    _logger?.LogInformation("沒有過期的預留需要清理");
                    return 0;
                }
                
                int cleanedCount = 0;
                
                foreach (var reservation in expiredReservations)
                {
                    // 更新預留狀態
                    var updateFilter = Builders<Reservation>.Filter.Eq(r => r.Id, reservation.Id);
                    var update = Builders<Reservation>.Update
                        .Set(r => r.Status, "expired");
                    
                    var result = await _dbContext.Reservations.UpdateOneAsync(updateFilter, update);
                    
                    if (result.ModifiedCount > 0)
                    {
                        // 更新各產品庫存
                        foreach (var item in reservation.Items)
                        {
                            if (item.VariantId != null)
                            {
                                // 更新變體庫存 - 將預留數量釋放回可用庫存
                                var filter = Builders<Product>.Filter.And(
                                    Builders<Product>.Filter.Eq(p => p.Id, item.ProductId),
                                    Builders<Product>.Filter.ElemMatch(p => p.Variants, v => v.VariantId == item.VariantId)
                                );
                                
                                var update = Builders<Product>.Update
                                    .Inc("Variants.$.Stock", item.Quantity);
                                
                                await _dbContext.Products.UpdateOneAsync(filter, update);
                            }
                            else
                            {
                                // 更新主產品庫