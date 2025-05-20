using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.DTOs; // 修改這裡，使用新的命名空間

namespace PaymentService.Services
{
    /// <summary>
    /// 退款服務實現
    /// </summary>
    public class RefundService : IRefundService
    {
        private readonly PaymentDbContext _dbContext;
        private readonly ILogger<RefundService> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        public RefundService(PaymentDbContext dbContext, ILogger<RefundService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 創建退款請求
        /// </summary>
        public async Task<Refund> CreateRefundRequestAsync(CreateRefundRequest request, string userId)
        {
            _logger.LogInformation("創建退款請求 - 交易ID: {TransactionId}, 金額: {Amount}", request.PaymentTransactionId, request.Amount);
            // 檢查支付交易是否存在
            var transaction = await _dbContext.PaymentTransactions
                .Include(p => p.Refunds)
                .FirstOrDefaultAsync(p => p.Id == request.PaymentTransactionId);
                
            if (transaction == null)
            {
                _logger.LogWarning("支付交易不存在: {TransactionId}", request.PaymentTransactionId);
                throw new Exception($"支付交易不存在: {request.PaymentTransactionId}");
            }
            
            // 檢查交易狀態是否允許退款
            if (transaction.Status != "Completed")
            {
                _logger.LogWarning("交易狀態不允許退款: {Status}", transaction.Status);
                throw new Exception($"只有已完成的交易才能退款: {transaction.Status}");
            }
            
            // 檢查退款金額
            var totalRefundedAmount = transaction.Refunds.Sum(r => r.Amount);
            if (totalRefundedAmount + request.Amount > transaction.Amount)
            {
                _logger.LogWarning("退款總額超過交易金額: {TotalRefunded} + {RequestAmount} > {TransactionAmount}", 
                    totalRefundedAmount, request.Amount, transaction.Amount);
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
            
            _logger.LogInformation("退款請求創建成功 - 退款ID: {RefundId}", refund.Id);
            
            return refund;
        }

        /// <summary>
        /// 獲取退款詳情
        /// </summary>
        public async Task<Refund?> GetRefundByIdAsync(string id)
        {
            return await _dbContext.Refunds
                .Include(r => r.PaymentTransaction)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// 獲取交易的所有退款
        /// </summary>
        public async Task<IEnumerable<Refund>> GetRefundsByTransactionIdAsync(string transactionId)
        {
            return await _dbContext.Refunds
                .Where(r => r.PaymentTransactionId == transactionId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 處理退款
        /// </summary>
        public async Task<Refund> ProcessRefundAsync(string id, bool isSuccess, string? responseData = null)
        {
            var refund = await _dbContext.Refunds
                .Include(r => r.PaymentTransaction)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refund == null)
            {
                throw new Exception($"退款記錄不存在: {id}");
            }

            if (refund.Status != "Pending")
            {
                throw new Exception($"退款狀態不允許處理: {refund.Status}");
            }

            // 更新退款狀態
            var previousStatus = refund.Status;
            refund.Status = isSuccess ? "Completed" : "Failed";
            refund.ResponseData = responseData ?? "{}";
            refund.ProcessedAt = DateTime.UtcNow;
            refund.UpdatedAt = DateTime.UtcNow;

            // 如果成功，更新交易狀態
            if (isSuccess)
            {
                var transaction = refund.PaymentTransaction;
                if (transaction != null)
                {
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
                        PaymentTransactionId = transaction.Id,
                        PreviousStatus = "Refunding",
                        CurrentStatus = transaction.Status,
                        Reason = $"退款完成: {refund.Amount} {transaction.Currency}",
                        AdditionalData = "{}"
                    };

                    await _dbContext.PaymentStatusHistories.AddAsync(statusHistory);
                }
            }
            else
            {
                // 退款失敗，恢復交易狀態
                var transaction = refund.PaymentTransaction;
                if (transaction != null && transaction.Status == "Refunding")
                {
                    transaction.Status = "Completed";
                    transaction.UpdatedAt = DateTime.UtcNow;

                    // 添加狀態歷史記錄
                    var statusHistory = new PaymentStatusHistory
                    {
                        PaymentTransactionId = transaction.Id,
                        PreviousStatus = "Refunding",
                        CurrentStatus = "Completed",
                        Reason = $"退款失敗: {refund.Amount} {transaction.Currency}",
                        AdditionalData = "{}"
                    };

                    await _dbContext.PaymentStatusHistories.AddAsync(statusHistory);
                }
            }

            await _dbContext.SaveChangesAsync();

            return refund;
        }
    }
}