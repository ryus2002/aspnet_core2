using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs
{
    public class CreatePaymentRequest
    {
        [Required]
        public required string OrderId { get; set; }

        [Required]
        public required string UserId { get; set; }

        [Required]
        public required string PaymentMethodId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public required string Currency { get; set; } = "TWD";
        public string? Description { get; set; }

        [Required]
        public required string ClientIp { get; set; }

        public string? ClientDevice { get; set; }

        [Required]
        public required string SuccessUrl { get; set; }

        [Required]
        public required string FailureUrl { get; set; }

        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class CreateRefundRequest
    {
        [Required]
        public required string PaymentTransactionId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "退款金額必須大於0")]
        public decimal Amount { get; set; }

        [Required]
        public required string Reason { get; set; }
    }

    /// <summary>
    /// 取消支付請求
    /// </summary>
    public class CancelPaymentRequest
    {
        /// <summary>
        /// 取消原因
        /// </summary>
        [Required]
        public string? Reason { get; set; }
    }

    /// <summary>
    /// 支付響應DTO
    /// </summary>
    public class PaymentResponse
    {
        /// <summary>
        /// 交易ID
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// 訂單ID
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// 交易金額
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 貨幣代碼
        /// </summary>
        public string CurrencyCode { get; set; } = "TWD";

        /// <summary>
        /// 交易狀態
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 支付方式代碼
        /// </summary>
        public string PaymentMethodCode { get; set; } = string.Empty;

        /// <summary>
        /// 支付方式ID
        /// </summary>
        public string PaymentMethodId { get; set; } = string.Empty;

        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 完成時間
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 重定向URL
        /// </summary>
        public string? RedirectUrl { get; set; }

        /// <summary>
        /// 支付URL
        /// </summary>
        public string? PaymentUrl { get; set; }

        /// <summary>
        /// 錯誤消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 貨幣（兼容舊版屬性）
        /// </summary>
        public string Currency { get; set; } = "TWD";
    }

    public class PaginatedResponse<T>
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public required List<T> Items { get; set; }
    }

    public class PaymentMethodResponse
    {
        public required string Id { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string IconUrl { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class PaymentTransactionResponse
    {
        public required string Id { get; set; }
        public required string OrderId { get; set; }
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string Status { get; set; }
        public required string PaymentMethodId { get; set; }
        public required string PaymentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PaymentTransactionDetailResponse : PaymentTransactionResponse
    {
        public required string PaymentMethodName { get; set; }
        public required string TransactionReference { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? ClientIp { get; set; }
        public string? ClientDevice { get; set; }
        public required string ErrorMessage { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public required List<StatusHistoryItem> StatusHistory { get; set; }
    }

    public class StatusHistoryItem
    {
        public required string PreviousStatus { get; set; }
        public required string NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public required string Reason { get; set; }
    }

    public class RefundResponse
    {
        public required string Id { get; set; }
        public required string PaymentTransactionId { get; set; }
        public decimal Amount { get; set; }
        public required string Status { get; set; }
        public required string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public required string RequestedBy { get; set; }
    }

    public class RefundDetailResponse : RefundResponse
    {
        public required string ExternalRefundId { get; set; }
        public required string Notes { get; set; }
        public string? ResponseData { get; set; }
    }
}