using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PaymentService.Models;
using PaymentService.DTOs; // 修改這裡，使用新的命名空間

namespace PaymentService.Services
{
    /// <summary>
    /// 支付服務接口
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// 創建支付交易
        /// </summary>
        Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request);

        /// <summary>
        /// 根據ID獲取支付交易
        /// </summary>
        Task<PaymentTransaction?> GetPaymentAsync(string id);

        /// <summary>
        /// 獲取訂單的支付交易列表
        /// </summary>
        Task<IEnumerable<PaymentTransaction>> GetPaymentsByOrderIdAsync(string orderId);

        /// <summary>
        /// 完成支付
        /// </summary>
        Task<PaymentResponse> CapturePaymentAsync(string id);

        /// <summary>
        /// 取消支付
        /// </summary>
        Task<PaymentResponse> CancelPaymentAsync(string id, string? reason);

        /// <summary>
        /// 處理支付通知
        /// </summary>
        Task<NotificationProcessResult> ProcessPaymentNotification(string providerCode, string payload, IHeaderDictionary headers);

        /// <summary>
        /// 創建退款
        /// </summary>
        Task<Refund> CreateRefund(CreateRefundRequest request, string userId);

        /// <summary>
        /// 根據ID獲取退款
        /// </summary>
        Task<Refund?> GetRefundById(string id);

        /// <summary>
        /// 獲取活躍的支付方式
        /// </summary>
        Task<List<PaymentMethod>> GetActivePaymentMethods();

        /// <summary>
        /// 模擬完成支付（僅用於本地開發環境）
        /// </summary>
        Task<bool> MockCompletePayment(string transactionId, bool success);
    }
}