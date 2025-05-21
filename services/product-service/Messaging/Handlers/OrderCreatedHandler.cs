using Microsoft.Extensions.Logging;
using ProductService.Services;
using Shared.Messaging.Handlers;
using Shared.Messaging.Messages;

namespace ProductService.Messaging.Handlers
{
    /// <summary>
    /// 訂單創建事件處理器，用於處理訂單創建後的庫存更新
    /// </summary>
    public class OrderCreatedHandler : IMessageHandler<OrderCreatedMessage>
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;
        private readonly ILogger<OrderCreatedHandler> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="inventoryService">庫存服務</param>
        /// <param name="productService">商品服務</param>
        /// <param name="logger">日誌記錄器</param>
        public OrderCreatedHandler(
            IInventoryService inventoryService,
            IProductService productService,
            ILogger<OrderCreatedHandler> logger)
        {
            _inventoryService = inventoryService;
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// 處理訂單創建事件
        /// </summary>
        /// <param name="message">訂單創建消息</param>
        /// <returns>異步任務</returns>
        public async Task HandleAsync(OrderCreatedMessage message)
        {
            _logger.LogInformation("收到訂單創建事件: OrderId={OrderId}, OrderNumber={OrderNumber}, Items={ItemCount}",
                message.OrderId, message.OrderNumber, message.Items.Count);

            try
            {
                // 針對訂單中的每個商品更新庫存
                foreach (var item in message.Items)
                {
                    _logger.LogInformation("處理訂單項目: ProductId={ProductId}, VariantId={VariantId}, Quantity={Quantity}",
                        item.ProductId, item.VariantId, item.Quantity);

                    // 檢查商品是否存在
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("找不到商品: ProductId={ProductId}", item.ProductId);
                        continue;
                    }

                    // 創建庫存變動記錄（減少庫存）
                    await _inventoryService.CreateInventoryChangeAsync(
                        productId: item.ProductId,
                        variantId: item.VariantId,
                        type: "OrderCreated",
                        quantity: -item.Quantity, // 負數表示減少庫存
                        reason: $"訂單創建: {message.OrderNumber}",
                        referenceId: message.OrderId,
                        userId: message.UserId);

                    _logger.LogInformation("已更新商品庫存: ProductId={ProductId}, VariantId={VariantId}, Quantity={Quantity}",
                        item.ProductId, item.VariantId, -item.Quantity);
                }

                _logger.LogInformation("訂單創建事件處理完成: OrderId={OrderId}", message.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理訂單創建事件時發生錯誤: OrderId={OrderId}", message.OrderId);
                throw;
            }
        }
    }
}