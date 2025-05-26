using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Services
{
    public partial class InventoryService
    {
        /// <summary>
        /// 按條件獲取預留
        /// </summary>
        /// <param name="filters">過濾條件</param>
        /// <returns>預留列表</returns>
        public async Task<List<Reservation>> GetReservationsAsync(Dictionary<string, object> filters)
        {
            try
            {
                _logger?.LogInformation($"按條件獲取預留: 條件數量={filters.Count}");
                
                var filterBuilder = Builders<Reservation>.Filter;
                var filter = filterBuilder.Empty;
                
                foreach (var kvp in filters)
                {
                    switch (kvp.Key)
                    {
                        case "Id":
                            filter &= filterBuilder.Eq(r => r.Id, kvp.Value.ToString());
                            break;
                        case "OwnerId":
                            filter &= filterBuilder.Eq(r => r.OwnerId, kvp.Value.ToString());
                            break;
                        case "OwnerType":
                            filter &= filterBuilder.Eq(r => r.OwnerType, kvp.Value.ToString());
                            break;
                        case "SessionId":
                            filter &= filterBuilder.Eq(r => r.SessionId, kvp.Value.ToString());
                            break;
                        case "UserId":
                            filter &= filterBuilder.Eq(r => r.UserId, kvp.Value.ToString());
                            break;
                        case "Status":
                            filter &= filterBuilder.Eq(r => r.Status, kvp.Value.ToString());
                            break;
                        case "ReferenceId":
                            filter &= filterBuilder.Eq(r => r.ReferenceId, kvp.Value.ToString());
                            break;
                        case "ProductId":
                            filter &= filterBuilder.ElemMatch(r => r.Items, 
                                Builders<ReservationItem>.Filter.Eq(i => i.ProductId, kvp.Value.ToString()));
                            break;
                        case "VariantId":
                            filter &= filterBuilder.ElemMatch(r => r.Items, 
                                Builders<ReservationItem>.Filter.Eq(i => i.VariantId, kvp.Value.ToString()));
                            break;
                        default:
                            _logger?.LogWarning($"未知的過濾條件: {kvp.Key}");
                            break;
                    }
                }
                
                var reservations = await _dbContext.Reservations.Find(filter).ToListAsync();
                
                _logger?.LogInformation($"按條件獲取預留完成: 找到={reservations.Count}");
                return reservations;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "按條件獲取預留時發生錯誤");
                return new List<Reservation>();
            }
        }

        public async Task<PagedResponse<InventoryChange>> GetInventoryChangesAsync(
            string productId, 
            string? variantId = null, 
            int page = 1, 
            int pageSize = 10)
        {
            try
            {
                _logger?.LogInformation($"獲取庫存變動記錄: ProductId={productId}, VariantId={variantId}, Page={page}, PageSize={pageSize}");
                
                // 构建过滤条件
                var filter = Builders<InventoryChange>.Filter.Eq(ic => ic.ProductId, productId);
                if (!string.IsNullOrEmpty(variantId))
                {
                    filter = filter & Builders<InventoryChange>.Filter.Eq(ic => ic.VariantId, variantId);
                }
                
                // 计算总数
                var totalItems = await _dbContext.InventoryChanges.CountDocumentsAsync(filter);
                
                // 获取分页数据
                var skip = (page - 1) * pageSize;
                var inventoryChanges = await _dbContext.InventoryChanges
                    .Find(filter)
                    .Sort(Builders<InventoryChange>.Sort.Descending(ic => ic.Timestamp))
                    .Skip(skip)
                    .Limit(pageSize)
                    .ToListAsync();
                
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                
                var result = new PagedResponse<InventoryChange>
                {
                    Items = inventoryChanges,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
                
                _logger?.LogInformation($"獲取庫存變動記錄完成: ProductId={productId}, Count={inventoryChanges.Count}");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"獲取庫存變動記錄時發生錯誤: ProductId={productId}");
                throw;
            }
        }
    }
}