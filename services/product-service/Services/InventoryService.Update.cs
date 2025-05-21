using Microsoft.Extensions.Logging;
using ProductService.Models;
using ProductService.Messaging.Publishers;

namespace ProductService.Services
{
    /// <summary>
    /// 庫存服務的庫存更新相關功能
    /// </summary>
    public partial class InventoryService : IInventoryService
    {
        private readonly InventoryEventPublisher _inventoryEventPublisher;

        /// <summary>
        /// 創建庫存變動記錄
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="variantId">變體ID</param>
        /// <param name="type">變動類型</param>
        /// <param name="quantity">變動數量</param>
        /// <param name="reason">變動原因</param>
        /// <param name="referenceId">相關單據ID</param>
        /// <param name="userId">操作用戶ID</param>
        /// <returns>庫存變動記錄</returns>
        public async Task<InventoryChange> CreateInventoryChangeAsync(
            string productId, 
            string? variantId, 
            string type, 
            int quantity, 
            string reason, 
            string? referenceId = null, 
            string? userId = null)
        {
            _logger.LogInformation("創建庫存變動記錄: ProductId={ProductId}, VariantId={VariantId}, Type={Type}, Quantity={Quantity}",
                productId, variantId, type, quantity);

            // 獲取商品
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException($"找不到商品: {productId}");
            }

            // 更新商品庫存
            int newQuantity;
            if (variantId != null)
            {
                // 更新變體庫存
                var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
                if (variant == null)
                {
                    throw new NotFoundException($"找不到商品變體: {variantId}");
                }

                variant.Stock.Quantity += quantity;
                newQuantity = variant.Stock.Quantity;

                // 檢查庫存是否低於閾值
                if (variant.Stock.Quantity <= variant.Stock.LowStockThreshold)
                {
                    _logger.LogWarning("商品變體庫存不足: ProductId={ProductId}, VariantId={VariantId}, Quantity={Quantity}, Threshold={Threshold}",
                        productId, variantId, variant.Stock.Quantity, variant.Stock.LowStockThreshold);

                    // 發布庫存不足事件
                    await _inventoryEventPublisher.PublishInventoryLowEventAsync(
                        productId, 
                        variantId, 
                        $"{product.Name} - {variant.Name}", 
                        variant.Stock.Quantity, 
                        variant.Stock.LowStockThreshold);
                }
            }
            else
            {
                // 更新主商品庫存
                product.Stock.Quantity += quantity;
                newQuantity = product.Stock.Quantity;

                // 檢查庫存是否低於閾值
                if (product.Stock.Quantity <= product.Stock.LowStockThreshold)
                {
                    _logger.LogWarning("商品庫存不足: ProductId={ProductId}, Quantity={Quantity}, Threshold={Threshold}",
                        productId, product.Stock.Quantity, product.Stock.LowStockThreshold);

                    // 發布庫存不足事件
                    await _inventoryEventPublisher.PublishInventoryLowEventAsync(
                        productId, 
                        null, 
                        product.Name, 
                        product.Stock.Quantity, 
                        product.Stock.LowStockThreshold);
                }
            }

            // 更新商品
            await _productService.UpdateProductAsync(productId, new DTOs.UpdateProductRequest
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price.Regular,
                Status = product.Status
            });

            // 創建庫存變動記錄
            var inventoryChange = new InventoryChange
            {
                Id = Guid.NewGuid().ToString(),
                ProductId = productId,
                VariantId = variantId,
                Type = type,
                Quantity = quantity,
                NewQuantity = newQuantity,
                Reason = reason,
                ReferenceId = referenceId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // 保存庫存變動記錄
            await _dbContext.InventoryChanges.InsertOneAsync(inventoryChange);

            _logger.LogInformation("庫存變動記錄已創建: Id={Id}, ProductId={ProductId}, Quantity={Quantity}, NewQuantity={NewQuantity}",
                inventoryChange.Id, productId, quantity, newQuantity);

            // 發布庫存更新事件
            await _inventoryEventPublisher.PublishInventoryUpdatedEventAsync(inventoryChange, product.Name);

            return inventoryChange;
        }
    }
}