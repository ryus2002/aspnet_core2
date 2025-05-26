using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Services
{
    /// <summary>
    /// 訂單服務實現 - 訂單取消功能
    /// </summary>
    public partial class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 取消訂單（增強版）
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
                var order = await _dbContext.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == id);
                
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

                // 保存訂單狀態變更
                await _dbContext.SaveChangesAsync();

                // 處理庫存回滾
                bool inventoryRollbackSuccess = await RollbackInventoryAsync(order);
                if (!inventoryRollbackSuccess)
                {
                    _logger.LogWarning("訂單 {OrderId} 的庫存回滾失敗，但訂單仍將被取消", id);
                }

                // 處理退款（如果訂單已支付）
                bool refundProcessed = false;
                if (order.Status == "paid" || order.Status == "processing" || order.Status == "shipped")
                {
                    if (!string.IsNullOrEmpty(order.PaymentId))
                    {
                        refundProcessed = await ProcessRefundAsync(order);
                        if (!refundProcessed)
                        {
                            _logger.LogWarning("訂單 {OrderId} 的退款處理失敗，但訂單仍將被取消", id);
                        }
                    }
                }

                // 提交事務
                await transaction.CommitAsync();

                _logger.LogInformation("取消訂單 {OrderId} 成功，庫存回滾: {InventoryStatus}，退款處理: {RefundStatus}", 
                    id, 
                    inventoryRollbackSuccess ? "成功" : "失敗", 
                    refundProcessed ? "成功" : "不需要或失敗");

                return await GetOrderResponseAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "取消訂單 {OrderId} 時發生錯誤", id);
                throw;
            }
        }

        /// <summary>
        /// 回滾訂單相關庫存
        /// </summary>
        /// <param name="order">訂單實體</param>
        /// <returns>是否成功</returns>
        private async Task<bool> RollbackInventoryAsync(Order order)
        {
            try
            {
                _logger.LogInformation("開始回滾訂單 {OrderId} 的庫存", order.Id);

                // 檢查是否有庫存預留ID
                var metadata = order.Metadata != null ? 
                    JsonSerializer.Deserialize<Dictionary<string, object>>(order.Metadata) : 
                    new Dictionary<string, object>();

                string? reservationId = null;
                if (metadata.ContainsKey("inventoryReservationId"))
                {
                    reservationId = metadata["inventoryReservationId"].ToString();
                }

                // 如果有庫存預留ID，嘗試取消預留
                if (!string.IsNullOrEmpty(reservationId))
                {
                    // 呼叫庫存服務的取消預留API
                    var response = await _httpClient.PostAsync(
                        $"{_configuration["Services:ProductService"]}/api/inventory/reservations/{reservationId}/cancel",
                        null);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("成功取消訂單 {OrderId} 的庫存預留 {ReservationId}", order.Id, reservationId);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("取消訂單 {OrderId} 的庫存預留 {ReservationId} 失敗: {StatusCode}", 
                            order.Id, reservationId, response.StatusCode);
                        return false;
                    }
                }
                else
                {
                    // 如果沒有庫存預留ID，創建庫存回滾請求
                    var rollbackItems = order.Items.Select(item => new 
                    {
                        ProductId = item.ProductId,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity
                    }).ToList();

                    var content = new StringContent(
                        JsonSerializer.Serialize(new { OrderId = order.Id, Items = rollbackItems }),
                        Encoding.UTF8,
                        "application/json");

                    var response = await _httpClient.PostAsync(
                        $"{_configuration["Services:ProductService"]}/api/inventory/rollback",
                        content);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("成功回滾訂單 {OrderId} 的庫存", order.Id);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("回滾訂單 {OrderId} 的庫存失敗: {StatusCode}", 
                            order.Id, response.StatusCode);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "回滾訂單 {OrderId} 的庫存時發生錯誤", order.Id);
                return false;
            }
        }

        /// <summary>
        /// 處理訂單退款
        /// </summary>
        /// <param name="order">訂單實體</param>
        /// <returns>是否成功</returns>
        private async Task<bool> ProcessRefundAsync(Order order)
        {
            try
            {
                _logger.LogInformation("開始處理訂單 {OrderId} 的退款，支付ID: {PaymentId}", order.Id, order.PaymentId);

                if (string.IsNullOrEmpty(order.PaymentId))
                {
                    _logger.LogWarning("訂單 {OrderId} 沒有關聯的支付ID，無法處理退款", order.Id);
                    return false;
                }

                // 創建退款請求
                var refundRequest = new
                {
                    PaymentTransactionId = order.PaymentId,
                    Amount = order.TotalAmount,
                    Reason = $"訂單取消: {order.CancellationReason}"
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(refundRequest),
                    Encoding.UTF8,
                    "application/json");

                // 呼叫支付服務的退款API
                var response = await _httpClient.PostAsync(
                    $"{_configuration["Services:PaymentService"]}/api/payments/refunds",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("成功為訂單 {OrderId} 創建退款請求", order.Id);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("為訂單 {OrderId} 創建退款請求失敗: {StatusCode}, {Error}", 
                        order.Id, response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理訂單 {OrderId} 的退款時發生錯誤", order.Id);
                return false;
            }
        }
    }
}