using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.DTOs; // 修改這裡，使用新的命名空間

namespace PaymentService.Services
{
    /// <summary>
    /// 支付服務實現
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly PaymentDbContext _dbContext;
        private readonly ILogger<PaymentService> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        public PaymentService(PaymentDbContext dbContext, ILogger<PaymentService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 創建支付交易
        /// </summary>
        public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request)
        {
            _logger.LogInformation("創建支付交易 - 訂單ID: {OrderId}, 金額: {Amount}", request.OrderId, request.Amount);

            // 檢查支付方式是否存在並啟用
            var paymentMethod = await _dbContext.PaymentMethods
                .FirstOrDefaultAsync(m => m.Id == request.PaymentMethodId && m.IsActive);

            if (paymentMethod == null)
            {
                _logger.LogWarning("支付方式不存在或未啟用: {PaymentMethodId}", request.PaymentMethodId);
                throw new Exception($"支付方式不存在或未啟用: {request.PaymentMethodId}");
            }
            
            // 創建支付交易記錄
            var transaction = new PaymentTransaction
            {
                OrderId = request.OrderId,
                UserId = request.UserId,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethodId = request.PaymentMethodId,
                Description = request.Description,
                ClientIp = request.ClientIp ?? "0.0.0.0",
                ClientDevice = request.ClientDevice,
                SuccessUrl = request.SuccessUrl,
                FailureUrl = request.FailureUrl,
                Status = "Pending",
                TransactionReference = Guid.NewGuid().ToString(),
                PaymentProviderResponse = "{}",
                ErrorMessage = "",
                PaymentIntentId = Guid.NewGuid().ToString(),
                Metadata = System.Text.Json.JsonSerializer.Serialize(request.Metadata ?? new Dictionary<string, string>())
            };
            
            // 添加狀態歷史記錄
            var statusHistory = new PaymentStatusHistory
            {
                PaymentTransactionId = transaction.Id,
                PreviousStatus = "",
                CurrentStatus = "Pending",
                Reason = "交易創建",
                AdditionalData = "{}"
            };
            
            await _dbContext.PaymentTransactions.AddAsync(transaction);
            await _dbContext.PaymentStatusHistories.AddAsync(statusHistory);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("支付交易創建成功 - 交易ID: {TransactionId}", transaction.Id);

            // 返回支付響應
            return new PaymentResponse
            {
                TransactionId = transaction.Id,
                OrderId = transaction.OrderId,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                CurrencyCode = transaction.Currency,
                Status = transaction.Status,
                PaymentMethodId = transaction.PaymentMethodId,
                PaymentMethodCode = paymentMethod.Code,
                CreatedAt = transaction.CreatedAt,
                PaymentUrl = $"/api/payment/process/{transaction.Id}"
            };
        }

        /// <summary>
        /// 根據ID獲取支付交易
        /// </summary>
        public async Task<PaymentTransaction?> GetPaymentAsync(string id)
        {
            return await _dbContext.PaymentTransactions
                .Include(p => p.StatusHistories)
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// 獲取訂單的支付交易列表
        /// </summary>
        public async Task<IEnumerable<PaymentTransaction>> GetPaymentsByOrderIdAsync(string orderId)
        {
            return await _dbContext.PaymentTransactions
                .Include(p => p.PaymentMethod)
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 完成支付
        /// </summary>
        public async Task<PaymentResponse> CapturePaymentAsync(string id)
        {
            var transaction = await _dbContext.PaymentTransactions
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (transaction == null)
            {
                throw new Exception($"支付交易不存在: {id}");
            }

            if (transaction.Status != "Pending" && transaction.Status != "Authorized")
            {
                throw new Exception($"支付交易狀態不允許捕獲: {transaction.Status}");
            }

            // 更新交易狀態
            var previousStatus = transaction.Status;
            transaction.Status = "Completed";
            transaction.PaidAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;
            
            // 添加狀態歷史記錄
            var statusHistory = new PaymentStatusHistory
            {
                PaymentTransactionId = transaction.Id,
                PreviousStatus = previousStatus,
                CurrentStatus = "Completed",
                Reason = "支付完成",
                AdditionalData = "{}"
            };

            await _dbContext.PaymentStatusHistories.AddAsync(statusHistory);
            await _dbContext.SaveChangesAsync();

            // 返回支付響應
            return new PaymentResponse
            {
                TransactionId = transaction.Id,
                OrderId = transaction.OrderId,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                CurrencyCode = transaction.Currency,
                Status = transaction.Status,
                PaymentMethodId = transaction.PaymentMethodId,
                PaymentMethodCode = transaction.PaymentMethod?.Code ?? "",
                CreatedAt = transaction.CreatedAt,
                CompletedAt = transaction.PaidAt
            };
        }

        /// <summary>
        /// 取消支付
        /// </summary>
        public async Task<PaymentResponse> CancelPaymentAsync(string id, string? reason)
        {
            var transaction = await _dbContext.PaymentTransactions
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (transaction == null)
            {
                throw new Exception($"支付交易不存在: {id}");
            }

            if (transaction.Status == "Completed" || transaction.Status == "Cancelled")
            {
                throw new Exception($"支付交易狀態不允許取消: {transaction.Status}");
            }

            // 更新交易狀態
            var previousStatus = transaction.Status;
            transaction.Status = "Cancelled";
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.ErrorMessage = reason ?? "用戶取消";

            // 添加狀態歷史記錄
            var statusHistory = new PaymentStatusHistory
            {
                PaymentTransactionId = transaction.Id,
                PreviousStatus = previousStatus,
                CurrentStatus = "Cancelled",
                Reason = reason ?? "用戶取消",
                AdditionalData = "{}"
            };

            await _dbContext.PaymentStatusHistories.AddAsync(statusHistory);
            await _dbContext.SaveChangesAsync();

            // 返回支付響應
            return new PaymentResponse
            {
                TransactionId = transaction.Id,
                OrderId = transaction.OrderId,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                CurrencyCode = transaction.Currency,
                Status = transaction.Status,
                PaymentMethodId = transaction.PaymentMethodId,
                PaymentMethodCode = transaction.PaymentMethod?.Code ?? "",
                CreatedAt = transaction.CreatedAt,
                ErrorMessage = transaction.ErrorMessage
            };
        }

        /// <summary>
        /// 處理支付通知
        /// </summary>
        public async Task<NotificationProcessResult> ProcessPaymentNotification(string providerCode, string payload, IHeaderDictionary headers)
        {
            _logger.LogInformation("收到支付通知 - 提供商: {ProviderCode}", providerCode);

            // 創建通知記錄
            var notification = new PaymentNotification
            {
                Id = Guid.NewGuid().ToString(),
                ProviderCode = providerCode,
                RawData = payload,
                RequestHeaders = System.Text.Json.JsonSerializer.Serialize(headers.ToDictionary(h => h.Key, h => h.Value.ToString())),
                IpAddress = "127.0.0.1",
                PaymentTransactionId = "unknown",
                ProcessingResult = "Pending"
            };

            await _dbContext.PaymentNotifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();

            // 模擬處理通知
            // 在實際應用中，這裡需要根據不同支付提供商解析通知內容並更新交易狀態
            notification.IsProcessed = true;
            notification.ProcessedAt = DateTime.UtcNow;
            notification.ProcessingResult = "通知已處理";
            await _dbContext.SaveChangesAsync();

            return new NotificationProcessResult
            {
                Success = true,
                Message = "通知已處理"
            };
        }

        /// <summary>
        /// 創建退款
        /// </summary>
        public async Task<Refund> CreateRefund(CreateRefundRequest request, string userId)
        {
            var transaction = await _dbContext.PaymentTransactions
                .Include(p => p.Refunds)
                .FirstOrDefaultAsync(p => p.Id == request.PaymentTransactionId);

            if (transaction == null)
            {
                throw new Exception($"支付交易不存在: {request.PaymentTransactionId}");
            }

            if (transaction.Status != "Completed")
            {
                throw new Exception("只有已完成的交易才能退款");
            }

            // 檢查退款金額
            var totalRefundedAmount = transaction.Refunds.Sum(r => r.Amount);
            if (totalRefundedAmount + request.Amount > transaction.Amount)
            {
                throw new Exception("退款總額不能超過交易金額");
            }

            // 創建退款記錄
            var refund = new Refund
            {
                Id = Guid.NewGuid().ToString(),
                PaymentTransactionId = transaction.Id,
                Amount = request.Amount,
                Reason = request.Reason ?? "用戶申請退款",
                Status = "Pending",
                RequestedBy = userId,
                ExternalRefundId = Guid.NewGuid().ToString(),
                ResponseData = "{}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.Refunds.AddAsync(refund);

            // 添加狀態歷史記錄
            var statusHistory = new PaymentStatusHistory
            {
                PaymentTransactionId = transaction.Id,
                PreviousStatus = transaction.Status,
                CurrentStatus = "Refunding",
                Reason = $"退款申請: {request.Amount} {transaction.Currency}",
                AdditionalData = "{}"
            };

            await _dbContext.PaymentStatusHistories.AddAsync(statusHistory);
            
            // 更新交易狀態
            if (request.Amount == transaction.Amount)
            {
                transaction.Status = "Refunded";
            }
            else
            {
                transaction.Status = "PartiallyRefunded";
            }
            transaction.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return refund;
        }

        /// <summary>
        /// 根據ID獲取退款
        /// </summary>
        public async Task<Refund?> GetRefundById(string id)
        {
            return await _dbContext.Refunds
                .Include(r => r.PaymentTransaction)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// 獲取活躍的支付方式
        /// </summary>
        public async Task<List<PaymentMethod>> GetActivePaymentMethods()
        {
            return await _dbContext.PaymentMethods
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
        }

        /// <summary>
        /// 模擬完成支付（僅用於本地開發環境）
        /// </summary>
        public async Task<bool> MockCompletePayment(string transactionId, bool success)
        {
            var transaction = await _dbContext.PaymentTransactions
                .FirstOrDefaultAsync(p => p.Id == transactionId);

            if (transaction == null)
            {
                return false;
            }

            if (transaction.Status != "Pending")
            {
                return false;
            }

            // 更新交易狀態
            var previousStatus = transaction.Status;
            transaction.Status = success ? "Completed" : "Failed";
            transaction.UpdatedAt = DateTime.UtcNow;
            
            if (success)
            {
                transaction.PaidAt = DateTime.UtcNow;
            }
            else
            {
                transaction.ErrorMessage = "模擬支付失敗";
            }

            // 添加狀態歷史記錄
            var statusHistory = new PaymentStatusHistory
            {
                PaymentTransactionId = transaction.Id,
                PreviousStatus = previousStatus,
                CurrentStatus = transaction.Status,
                Reason = success ? "模擬支付成功" : "模擬支付失敗",
                AdditionalData = "{}"
            };

            await _dbContext.PaymentStatusHistories.AddAsync(statusHistory);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}