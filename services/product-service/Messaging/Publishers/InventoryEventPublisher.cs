using Microsoft.Extensions.Logging;
using ProductService.Models;
using Shared.Messaging;

namespace ProductService.Messaging.Publishers
{
    /// <summary>
    /// 庫存事件發布器，負責發送庫存相關事件
    /// </summary>
    public class InventoryEventPublisher
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<InventoryEventPublisher> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="messageBus">消息總線</param>
        /// <param name="logger">日誌記錄器</param>
        public InventoryEventPublisher(IMessageBus messageBus, ILogger<InventoryEventPublisher> logger)
        {
            _messageBus = messageBus;
            _logger = logger;
        }

        /// <summary>
        /// 發布庫存更新事件
        /// </summary>
        /// <param name="inventoryChange">庫存變動記錄</param>
        /// <param name="productName">商品名稱</param>
        /// <returns>異步任務</returns>
        public async Task PublishInventoryUpdatedEventAsync(InventoryChange inventoryChange, string productName)
        {
            try
            {
                _logger.LogInformation("準備發布庫存更新事件: ProductId={ProductId}, VariantId={VariantId}, Quantity={Quantity}", 
                    inventoryChange.ProductId, inventoryChange.VariantId, inventoryChange.Quantity);

                var message = new InventoryUpdatedMessage
                {
                    ProductId = inventoryChange.ProductId,
                    VariantId = inventoryChange.VariantId,
                    ProductName = productName,
                    NewQuantity = inventoryChange.NewQuantity,
                    QuantityChange = inventoryChange.Quantity,
                    Reason = inventoryChange.Reason,
                    ReferenceId = inventoryChange.ReferenceId,
                    UserId = inventoryChange.UserId,
                    Sender = "product-service"
                };

                await _messageBus.PublishAsync(message, "ecommerce", "inventory.updated");

                _logger.LogInformation("庫存更新事件已發布: ProductId={ProductId}, MessageId={MessageId}", 
                    inventoryChange.ProductId, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發布庫存更新事件失敗: ProductId={ProductId}", inventoryChange.ProductId);
                throw;
            }
        }

        /// <summary>
        /// 發布庫存不足事件
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="variantId">變體ID</param>
        /// <param name="productName">商品名稱</param>
        /// <param name="currentQuantity">當前數量</param>
        /// <param name="threshold">閾值</param>
        /// <returns>異步任務</returns>
        public async Task PublishInventoryLowEventAsync(string productId, string? variantId, string productName, int currentQuantity, int threshold)
        {
            try
            {
                _logger.LogInformation("準備發布庫存不足事件: ProductId={ProductId}, VariantId={VariantId}, CurrentQuantity={CurrentQuantity}, Threshold={Threshold}", 
                    productId, variantId, currentQuantity, threshold);

                var message = new InventoryLowMessage
                {
                    ProductId = productId,
                    VariantId = variantId,
                    ProductName = productName,
                    CurrentQuantity = currentQuantity,
                    Threshold = threshold,
                    Sender = "product-service"
                };

                await _messageBus.PublishAsync(message, "ecommerce", "inventory.low");

                _logger.LogInformation("庫存不足事件已發布: ProductId={ProductId}, MessageId={MessageId}", 
                    productId, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發布庫存不足事件失敗: ProductId={ProductId}", productId);
                throw;
            }
        }

        /// <summary>
        /// 發布庫存預留事件
        /// </summary>
        /// <param name="reservation">預留記錄</param>
        /// <returns>異步任務</returns>
        public async Task PublishInventoryReservedEventAsync(Reservation reservation)
        {
            try
            {
                _logger.LogInformation("準備發布庫存預留事件: ReservationId={ReservationId}, OwnerId={OwnerId}", 
                    reservation.Id, reservation.OwnerId);

                var message = new InventoryReservedMessage
                {
                    ReservationId = reservation.Id,
                    OwnerId = reservation.OwnerId,
                    OwnerType = reservation.OwnerType,
                    ExpiresAt = reservation.ExpiresAt,
                    Sender = "product-service",
                    Items = new List<ReservationItemMessage>()
                };

                // 添加預留項目
                foreach (var item in reservation.Items)
                {
                    message.Items.Add(new ReservationItemMessage
                    {
                        ProductId = item.ProductId,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity
                    });
                }

                await _messageBus.PublishAsync(message, "ecommerce", "inventory.reserved");

                _logger.LogInformation("庫存預留事件已發布: ReservationId={ReservationId}, MessageId={MessageId}", 
                    reservation.Id, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發布庫存預留事件失敗: ReservationId={ReservationId}", reservation.Id);
                throw;
            }
        }
    }

    /// <summary>
    /// 庫存更新消息
    /// </summary>
    public class InventoryUpdatedMessage : BaseMessage
    {
        public string ProductId { get; set; } = null!;
        public string? VariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public int NewQuantity { get; set; }
        public int QuantityChange { get; set; }
        public string Reason { get; set; } = null!;
        public string? ReferenceId { get; set; }
        public string? UserId { get; set; }
        public string Sender { get; set; } = null!;
    }

    /// <summary>
    /// 庫存不足消息
    /// </summary>
    public class InventoryLowMessage : BaseMessage
    {
        public string ProductId { get; set; } = null!;
        public string? VariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public int CurrentQuantity { get; set; }
        public int Threshold { get; set; }
        public string Sender { get; set; } = null!;
    }

    /// <summary>
    /// 庫存預留消息
    /// </summary>
    public class InventoryReservedMessage : BaseMessage
    {
        public string ReservationId { get; set; } = null!;
        public string OwnerId { get; set; } = null!;
        public string OwnerType { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public string Sender { get; set; } = null!;
        public List<ReservationItemMessage> Items { get; set; } = new List<ReservationItemMessage>();
    }

    /// <summary>
    /// 預留項目消息
    /// </summary>
    public class ReservationItemMessage
    {
        public string ProductId { get; set; } = null!;
        public string? VariantId { get; set; }
        public int Quantity { get; set; }
    }
}