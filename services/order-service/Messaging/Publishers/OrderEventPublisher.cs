using Microsoft.Extensions.Logging;
using Shared.Messaging.MessageBus;
using Shared.Messaging.Messages;
using OrderService.Models;

namespace OrderService.Messaging.Publishers
{
    /// <summary>
    /// 訂單事件發布器，負責發送訂單相關事件
    /// </summary>
    public class OrderEventPublisher
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<OrderEventPublisher> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="messageBus">消息總線</param>
        /// <param name="logger">日誌記錄器</param>
        public OrderEventPublisher(IMessageBus messageBus, ILogger<OrderEventPublisher> logger)
        {
            _messageBus = messageBus;
            _logger = logger;
        }

        /// <summary>
        /// 發布訂單創建事件
        /// </summary>
        /// <param name="order">訂單對象</param>
        /// <returns>異步任務</returns>
        public async Task PublishOrderCreatedEventAsync(Order order)
        {
            try
            {
                _logger.LogInformation("準備發布訂單創建事件: OrderId={OrderId}, OrderNumber={OrderNumber}", 
                    order.Id, order.OrderNumber);

                var message = new OrderCreatedMessage
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    UserId = order.UserId,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    OrderDate = order.CreatedAt,
                    Sender = "order-service",
                    Items = order.Items.Select(item => new OrderItemMessage
                    {
                        ProductId = item.ProductId,
                        VariantId = item.VariantId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        SubTotal = item.SubTotal
                    }).ToList()
                };

                await _messageBus.PublishAsync(message);

                _logger.LogInformation("訂單創建事件已發布: OrderId={OrderId}, MessageId={MessageId}", 
                    order.Id, message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發布訂單創建事件失敗: OrderId={OrderId}", order.Id);
                throw;
            }
        }

        /// <summary>
        /// 發布訂單狀態更新事件
        /// </summary>
        /// <param name="order">訂單對象</param>
        /// <param name="oldStatus">舊狀態</param>
        /// <param name="newStatus">新狀態</param>
        /// <param name="reason">更新原因</param>
        /// <returns>異步任務</returns>
        public async Task PublishOrderStatusChangedEventAsync(Order order, string oldStatus, string newStatus, string? reason = null)
        {
            try
            {
                _logger.LogInformation("準備發布訂單狀態更新事件: OrderId={OrderId}, OldStatus={OldStatus}, NewStatus={NewStatus}", 
                    order.Id, oldStatus, newStatus);

                var message = new OrderStatusChangedMessage
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    UserId = order.UserId,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    Reason = reason,
                    ChangedAt = DateTime.UtcNow,
                    Sender = "order-service"
                };

                await _messageBus.PublishAsync(message);

                _logger.LogInformation("訂單狀態更新事件已發布: OrderId={OrderId}, MessageId={MessageId}", 
                    order.Id, message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發布訂單狀態更新事件失敗: OrderId={OrderId}", order.Id);
                throw;
            }
        }
    }

    /// <summary>
    /// 訂單狀態更新消息
    /// </summary>
    public class OrderStatusChangedMessage : BaseMessage
    {
        /// <summary>
        /// 訂單ID
        /// </summary>
        public string OrderId { get; set; } = null!;

        /// <summary>
        /// 訂單編號
        /// </summary>
        public string OrderNumber { get; set; } = null!;

        /// <summary>
        /// 用戶ID
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// 舊狀態
        /// </summary>
        public string OldStatus { get; set; } = null!;

        /// <summary>
        /// 新狀態
        /// </summary>
        public string NewStatus { get; set; } = null!;

        /// <summary>
        /// 更新原因
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// 狀態更新時間
        /// </summary>
        public DateTime ChangedAt { get; set; }
    }
}