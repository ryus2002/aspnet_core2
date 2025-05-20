using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.DTOs;

namespace PaymentService.Services
{
    /// <summary>
    /// 模擬支付服務
    /// 用於本地開發環境，模擬支付處理流程
    /// </summary>
    public class MockPaymentService : IMockPaymentService
    {
        private readonly PaymentDbContext _dbContext;
        private readonly ILogger<MockPaymentService> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        public MockPaymentService(PaymentDbContext dbContext, ILogger<MockPaymentService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 模擬支付處理
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <param name="shouldSucceed">是否成功</param>
        /// <param name="delaySeconds">延遲秒數</param>
        /// <returns>處理結果</returns>
        public async Task<MockPaymentResult> ProcessPaymentAsync(string transactionId, bool shouldSucceed = true, int delaySeconds = 3)
        {
            _logger.LogInformation("開始模擬支付處理 - 交易ID: {TransactionId}, 預期結果: {ShouldSucceed}", 
                transactionId, shouldSucceed ? "成功" : "失敗");

            // 獲取交易
            var transaction = await _dbContext.PaymentTransactions
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.Id == transactionId);

            if (transaction == null)
            {
                _logger.LogWarning("交易不存在: {TransactionId}", transactionId);
                return new MockPaymentResult
                {
                    Success = false,
                    Message = "交易不存在",
                    TransactionId = transactionId
                };
            }

            if (transaction.Status != "Pending")
            {
                _logger.LogWarning("交易狀態不允許處理: {Status}", transaction.Status);
                return new MockPaymentResult
                {
                    Success = false,
                    Message = $"交易狀態不允許處理: {transaction.Status}",
                    TransactionId = transactionId
                };
            }

            // 模擬處理延遲
            _logger.LogInformation("模擬支付處理延遲 {DelaySeconds} 秒", delaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

            // 更新交易狀態
            var previousStatus = transaction.Status;
            transaction.Status = shouldSucceed ? "Completed" : "Failed";
            transaction.UpdatedAt = DateTime.UtcNow;
            
            if (shouldSucceed)
            {
                transaction.PaidAt = DateTime.UtcNow;
                transaction.TransactionReference = $"mock-{Guid.NewGuid():N}";
                transaction.PaymentProviderResponse = System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = true,
                    provider = "模擬支付",
                    timestamp = DateTime.UtcNow,
                    reference = transaction.TransactionReference
                });
            }
            else
            {
                transaction.ErrorMessage = "模擬支付失敗";
                transaction.PaymentProviderResponse = System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    provider = "模擬支付",
                    timestamp = DateTime.UtcNow,
                    error = "模擬支付失敗"
                });
            }

            // 添加狀態歷史記錄
            var statusHistory = new PaymentStatusHistory
            {
                Id = Guid.NewGuid().ToString(),
                PaymentTransactionId = transaction.Id,
                PreviousStatus = previousStatus,
                CurrentStatus = transaction.Status,
                Reason = shouldSucceed ? "模擬支付成功" : "模擬支付失敗",
                AdditionalData = "{}"
            };

            await _dbContext.PaymentStatusHistories.AddAsync(statusHistory);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("模擬支付處理完成 - 交易ID: {TransactionId}, 結果: {Result}", 
                transactionId, shouldSucceed ? "成功" : "失敗");

            // 創建交易記錄
            await CreateTransactionRecord(transaction, shouldSucceed);

            return new MockPaymentResult
            {
                Success = true,
                Message = shouldSucceed ? "支付成功" : "支付失敗",
                TransactionId = transactionId,
                TransactionStatus = transaction.Status,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                CompletedAt = shouldSucceed ? transaction.PaidAt : null,
                ErrorMessage = shouldSucceed ? null : transaction.ErrorMessage
            };
        }

        /// <summary>
        /// 創建交易記錄
        /// </summary>
        private async Task CreateTransactionRecord(PaymentTransaction transaction, bool success)
        {
            // 在實際系統中，這裡可能會寫入更詳細的交易記錄
            // 例如寫入專門的交易日誌表或發送事件到消息隊列
            
            // 這裡簡單記錄一個支付通知
            var notification = new PaymentNotification
            {
                Id = Guid.NewGuid().ToString(),
                PaymentTransactionId = transaction.Id,
                ProviderCode = "mock",
                RawData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    transaction_id = transaction.Id,
                    order_id = transaction.OrderId,
                    amount = transaction.Amount,
                    currency = transaction.Currency,
                    status = transaction.Status,
                    type = success ? "payment_success" : "payment_failed",
                    timestamp = DateTime.UtcNow
                }),
                RequestHeaders = "{}",
                IpAddress = "127.0.0.1",
                IsProcessed = true,
                ProcessedAt = DateTime.UtcNow,
                ProcessingResult = success ? "支付成功" : "支付失敗"
            };

            await _dbContext.PaymentNotifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 模擬退款處理
        /// </summary>
        /// <param name="refundId">退款ID</param>
        /// <param name="shouldSucceed">是否成功</param>
        /// <param name="delaySeconds">延遲秒數</param>
        /// <returns>處理結果</returns>
        public async Task<MockRefundResult> ProcessRefundAsync(string refundId, bool shouldSucceed = true, int delaySeconds = 3)
        {
            _logger.LogInformation("開始模擬退款處理 - 退款ID: {RefundId}, 預期結果: {ShouldSucceed}", 
                refundId, shouldSucceed ? "成功" : "失敗");

            // 獲取退款記錄
            var refund = await _dbContext.Refunds
                .Include(r => r.PaymentTransaction)
                .FirstOrDefaultAsync(r => r.Id == refundId);

            if (refund == null)
            {
                _logger.LogWarning("退款記錄不存在: {RefundId}", refundId);
                return new MockRefundResult
                {
                    Success = false,
                    Message = "退款記錄不存在",
                    RefundId = refundId
                };
            }

            if (refund.Status != "Pending")
            {
                _logger.LogWarning("退款狀態不允許處理: {Status}", refund.Status);
                return new MockRefundResult
                {
                    Success = false,
                    Message = $"退款狀態不允許處理: {refund.Status}",
                    RefundId = refundId
                };
            }

            // 模擬處理延遲
            _logger.LogInformation("模擬退款處理延遲 {DelaySeconds} 秒", delaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

            // 更新退款狀態
            refund.Status = shouldSucceed ? "Completed" : "Failed";
            refund.ProcessedAt = DateTime.UtcNow;
            refund.UpdatedAt = DateTime.UtcNow;
            refund.ResponseData = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = shouldSucceed,
                provider = "模擬退款",
                timestamp = DateTime.UtcNow,
                reference = $"mock-refund-{Guid.NewGuid():N}"
            });

            // 如果退款成功，更新交易狀態
            if (shouldSucceed && refund.PaymentTransaction != null)
            {
                var transaction = refund.PaymentTransaction;
                var previousStatus = transaction.Status;
                
                // 檢查是否為全額退款
                var totalRefundedAmount = await _dbContext.Refunds
                    .Where(r => r.PaymentTransactionId == transaction.Id && r.Status == "Completed")
                    .SumAsync(r => r.Amount) + refund.Amount;

                if (totalRefundedAmount >= transaction.Amount)
                {
                    transaction.Status = "Refunded";
                }
                else
                {
                    transaction.Status = "PartiallyRefunded";
                }
                transaction.UpdatedAt = DateTime.UtcNow;

                // 添加狀態歷史記錄
                var statusHistory = new PaymentStatusHistory
                {
                    Id = Guid.NewGuid().ToString(),
                    PaymentTransactionId = transaction.Id,
                    PreviousStatus = previousStatus,
                    CurrentStatus = transaction.Status,
                    Reason = $"模擬退款完成: {refund.Amount} {transaction.Currency}",
                    AdditionalData = "{}"
                };

                await _dbContext.PaymentStatusHistories.AddAsync(statusHistory);
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("模擬退款處理完成 - 退款ID: {RefundId}, 結果: {Result}", 
                refundId, shouldSucceed ? "成功" : "失敗");

            return new MockRefundResult
            {
                Success = true,
                Message = shouldSucceed ? "退款成功" : "退款失敗",
                RefundId = refundId,
                RefundStatus = refund.Status,
                Amount = refund.Amount,
                ProcessedAt = refund.ProcessedAt
            };
        }
    }

    /// <summary>
    /// 模擬支付結果
    /// </summary>
    public class MockPaymentResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 交易ID
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// 交易狀態
        /// </summary>
        public string? TransactionStatus { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// 貨幣
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// 完成時間
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 錯誤消息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 模擬退款結果
    /// </summary>
    public class MockRefundResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 退款ID
        /// </summary>
        public string RefundId { get; set; } = string.Empty;

        /// <summary>
        /// 退款狀態
        /// </summary>
        public string? RefundStatus { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// 處理時間
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
    }
}