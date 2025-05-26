using MongoDB.Driver;
using ProductService.DTOs;
using ProductService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Services
{
    public partial class InventoryService
    {
        /// <summary>
        /// 回滾庫存
        /// </summary>
        /// <param name="orderId">訂單ID</param>
        /// <param name="items">回滾項目</param>
        /// <returns>是否成功</returns>
        public async Task<bool> RollbackInventoryAsync(string orderId, List<RollbackInventoryItem> items)
        {
            try
            {
                _logger?.LogInformation($"嘗試回滾訂單 {orderId} 的庫存，項目數量: {items.Count}");
                
                if (items == null || items.Count == 0)
                {
                    _logger?.LogWarning("回滾項目列表為空");
                    return false;
                }
                
                // 處理每個項目的庫存回滾
                foreach (var item in items)
                {
                    // 檢查產品是否存在
                    var filter = Builders<Product>.Filter.Eq(p => p.Id, item.ProductId);
                    var product = await _dbContext.Products.Find(filter).FirstOrDefaultAsync();
                    
                    if (product == null)
                    {
                        _logger?.LogWarning($"回滾庫存時找不到產品: {item.ProductId}");
                        continue;
                    }
                    
                    if (item.VariantId != null)
                    {
                        // 更新變體庫存
                        var variantFilter = Builders<Product>.Filter.And(
                            Builders<Product>.Filter.Eq(p => p.Id, item.ProductId),
                            Builders<Product>.Filter.ElemMatch(p => p.Variants, v => v.VariantId == item.VariantId)
                        );
                        
                        var update = Builders<Product>.Update
                            .Inc("Variants.$.Stock", item.Quantity);
                        
                        await _dbContext.Products.UpdateOneAsync(variantFilter, update);
                    }
                    else
                    {
                        // 更新主產品庫存
                        var update = Builders<Product>.Update
                            .Inc(p => p.Stock.Available, item.Quantity);
                        
                        await _dbContext.Products.UpdateOneAsync(filter, update);
                    }
                    
                    // 記錄庫存變動
                    await CreateInventoryChangeAsync(
                        item.ProductId,
                        item.VariantId,
                        "rollback",
                        item.Quantity,
                        $"訂單取消回滾庫存: {orderId}",
                        orderId,
                        null
                    );
                }
                
                _logger?.LogInformation($"成功回滾訂單 {orderId} 的庫存");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"回滾訂單 {orderId} 的庫存時發生錯誤");
                return false;
            }
        }
        
        /// <summary>
        /// 根據ID獲取預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <returns>預留</returns>
        public async Task<Reservation?> GetReservationByIdAsync(string id)
        {
            try
            {
                var filter = Builders<Reservation>.Filter.Eq(r => r.Id, id);
                return await _dbContext.Reservations.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"獲取預留時發生錯誤: {id}");
                return null;
            }
        }
    }
}