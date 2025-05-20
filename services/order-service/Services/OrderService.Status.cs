using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Services
{
    /// <summary>
    /// 訂單服務實現 - 狀態管理功能
    /// </summary>
    public partial class OrderService : IOrderService
    {
        /// <summary>
        /// 更新訂單狀態
        /// </summary>
        public async Task<OrderResponse> UpdateOrderStatusAsync(
            string id, 
            UpdateOrderStatusRequest request, 
            string? userId = null)
        {
            // 使用事務確保數據一致性
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 獲取訂單
                var order = await _dbContext.Orders.FindAsync(id);
                if (order == null)
                {
                    throw new KeyNotFoundException($"訂單不存在: {id}");
                }

                // 驗證狀態轉換是否合法
                if (!IsValidStatusTransition(order.Status, request.Status))
                {
                    throw new InvalidOperationException($"不允許從 {order.Status} 狀態轉換為 {request.Status} 狀態");
                }

                // 更新訂單狀態
                string oldStatus = order.Status;
                order.Status = request.Status;
                order.UpdatedAt = DateTime.UtcNow;

                // 根據新狀態設置特定時間戳
                if (request.Status == "completed")
                {
                    order.CompletedAt = DateTime.UtcNow;
                }
                else if (request.Status == "cancelled")
                {
                    order.CancelledAt = DateTime.UtcNow;
                    order.CancellationReason = "由管理員取消";
                }

                // 創建訂單狀態歷史
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = id,
                    Status = request.Status,
                    Comment = request.Comment,
                    ChangedBy = userId,
                    ChangedAt = DateTime.UtcNow
                };
                _dbContext.OrderStatusHistories.Add(statusHistory);

                // 創建訂單事件
                var orderEvent = new OrderEvent
                {
                    OrderId = id,
                    EventType = "status_updated",
                    Payload = JsonSerializer.Serialize(new 
                    { 
                        OrderId = id, 
                        OldStatus = oldStatus,
                        NewStatus = request.Status,
                        ChangedBy = userId
                    }),
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.OrderEvents.Add(orderEvent);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Updated order {OrderId} status from {OldStatus} to {NewStatus}", 
                    id, oldStatus, request.Status);

                return await GetOrderResponseAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating order status for order {OrderId}", id);
                throw;
            }
        }

        /// <summary>
        /// 取消訂單
        /// </summary>
        public async Task<OrderResponse> CancelOrderAsync(
            string id, 
            CancelOrderRequest request, 
            string? userId = null)
        {
            // 使用事務確保數據一致性
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 獲取訂單
                var order = await _dbContext.Orders.FindAsync(id);
                if (order == null)
                {
                    throw new KeyNotFoundException($"訂單不存在: {id}");
                }

                // 驗證訂單是否可以取消
                if (!CanCancelOrder(order.Status))
                {
                    throw new InvalidOperationException($"無法取消 {order.Status} 狀態的訂單");
                }

                // 更新訂單狀態
                string oldStatus = order.Status;
                order.Status = "cancelled";
                order.UpdatedAt = DateTime.UtcNow;
                order.CancelledAt = DateTime.UtcNow;
                order.CancellationReason = request.Reason;

                // 創建訂單狀態歷史
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = id,
                    Status = "cancelled",
                    Comment = $"取消原因: {request.Reason}",
                    ChangedBy = userId,
                    ChangedAt = DateTime.UtcNow
                };
                _dbContext.OrderStatusHistories.Add(statusHistory);

                // 創建訂單事件
                var orderEvent = new OrderEvent
                {
                    OrderId = id,
                    EventType = "cancelled",
                    Payload = JsonSerializer.Serialize(new 
                    { 
                        OrderId = id, 
                        Reason = request.Reason,
                        CancelledBy = userId
                    }),
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.OrderEvents.Add(orderEvent);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Cancelled order {OrderId} with reason: {Reason}", id, request.Reason);

                return await GetOrderResponseAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                throw;
            }
        }

        /// <summary>
        /// 更新訂單支付信息
        /// </summary>
        public async Task<OrderResponse> UpdateOrderPaymentAsync(string id, string paymentId)
        {
            // 使用事務確保數據一致性
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 獲取訂單
                var order = await _dbContext.Orders.FindAsync(id);
                if (order == null)
                {
                    throw new KeyNotFoundException($"訂單不存在: {id}");
                }

                // 更新支付ID
                order.PaymentId = paymentId;
                order.UpdatedAt = DateTime.UtcNow;

                // 如果訂單狀態為pending，更新為paid
                if (order.Status == "pending")
                {
                    string oldStatus = order.Status;
                    order.Status = "paid";

                    // 創建訂單狀態歷史
                    var statusHistory = new OrderStatusHistory
                    {
                        OrderId = id,
                        Status = "paid",
                        Comment = $"支付完成，支付ID: {paymentId}",
                        ChangedAt = DateTime.UtcNow
                    };
                    _dbContext.OrderStatusHistories.Add(statusHistory);

                    // 創建訂單事件
                    var orderEvent = new OrderEvent
                    {
                        OrderId = id,
                        EventType = "payment_completed",
                        Payload = JsonSerializer.Serialize(new 
                        { 
                            OrderId = id, 
                            PaymentId = paymentId,
                            OldStatus = oldStatus,
                            NewStatus = "paid"
                        }),
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.OrderEvents.Add(orderEvent);

                    _logger.LogInformation("Updated order {OrderId} status from {OldStatus} to paid after payment", 
                        id, oldStatus);
                }
                else
                {
                    // 創建訂單事件
                    var orderEvent = new OrderEvent
                    {
                        OrderId = id,
                        EventType = "payment_updated",
                        Payload = JsonSerializer.Serialize(new 
                        { 
                            OrderId = id, 
                            PaymentId = paymentId
                        }),
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.OrderEvents.Add(orderEvent);

                    _logger.LogInformation("Updated payment information for order {OrderId}", id);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetOrderResponseAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating payment information for order {OrderId}", id);
                throw;
            }
        }

        /// <summary>
        /// 驗證訂單狀態轉換是否合法
        /// </summary>
        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            // 定義合法的狀態轉換
            var validTransitions = new Dictionary<string, string[]>
            {
                { "pending", new[] { "paid", "processing", "cancelled" } },
                { "paid", new[] { "processing", "cancelled", "refunded" } },
                { "processing", new[] { "shipped", "cancelled", "refunded" } },
                { "shipped", new[] { "delivered", "returned" } },
                { "delivered", new[] { "returned", "completed" } },
                { "returned", new[] { "refunded" } },
                { "cancelled", new[] { "refunded" } }
                // completed和refunded是終態，不能再轉換
            };

            // 檢查當前狀態是否有定義轉換規則
            if (!validTransitions.ContainsKey(currentStatus))
            {
                return false;
            }

            // 檢查新狀態是否在允許的轉換列表中
            return validTransitions[currentStatus].Contains(newStatus);
        }

        /// <summary>
        /// 檢查訂單是否可以取消
        /// </summary>
        private bool CanCancelOrder(string status)
        {
            // 只有以下狀態的訂單可以取消
            var cancellableStatuses = new[] { "pending", "paid", "processing" };
            return cancellableStatuses.Contains(status);
        }
    }
}