using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ProductService.Models;
using ProductService.Models.Requests;
namespace ProductService.Services
{
    public partial class InventoryService
    {
        public async Task<InventoryChange> CreateInventoryChangeAsync(
            string productId,
            string? variantId,
            string type,
            int quantity,
            string reason,
            string? referenceId = null,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation($"創建庫存變動記錄: ProductId={productId}, VariantId={variantId}, Type={type}, Quantity={quantity}");
                
                // 确保 type 是有效的
                if (type != "increment" && type != "decrement" && type != "adjustment" && 
                    type != "reserve" && type != "release")
                {
                    _logger?.LogWarning($"無效的庫存變動類型: {type}");
                    throw new ArgumentException($"無效的庫存變動類型: {type}", nameof(type));
                }

                // 创建库存变动记录
            var inventoryChange = new InventoryChange
            {
                    Id = Guid.NewGuid().ToString(),
                ProductId = productId,
                VariantId = variantId,
                Type = type,
                Quantity = quantity,
                    Reason = reason ?? "系统自动更新", // 提供默认值，避免空引用
                    ReferenceId = referenceId,
                    UserId = userId,
                Timestamp = DateTime.UtcNow
            };

                // 插入记录
            await _dbContext.InventoryChanges.InsertOneAsync(inventoryChange);

                _logger?.LogInformation($"庫存變動記錄已創建: Id={inventoryChange.Id}");
                return inventoryChange;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"創建庫存變動記錄失敗: ProductId={productId}, Type={type}, Quantity={quantity}");
                throw; // 重新抛出异常，让调用者处理
            }
        }
    }
}
