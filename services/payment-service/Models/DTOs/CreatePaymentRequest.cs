using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PaymentService.Models.DTOs
{
    /// <summary>
    /// 創建支付請求的DTO
    /// </summary>
    public class CreatePaymentRequest
    {
        /// <summary>
        /// 訂單ID
        /// </summary>
        [Required]
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;
        /// <summary>
        /// 支付金額
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於0")]
        public decimal Amount { get; set; }
        /// <summary>
        /// 貨幣代碼 (例如: TWD, USD)
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "TWD";

        /// <summary>
        /// 支付方式ID
        /// </summary>
        [Required]
        public string PaymentMethodId { get; set; } = string.Empty;

        /// <summary>
        /// 支付方式代碼
        /// </summary>
        public string PaymentMethodCode { get; set; } = string.Empty;

        /// <summary>
        /// 支付描述
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 客戶端IP地址
        /// </summary>
        public string? ClientIp { get; set; }

        /// <summary>
        /// 客戶端設備信息
        /// </summary>
        public string? ClientDevice { get; set; }

        /// <summary>
        /// 支付成功後的回調URL
        /// </summary>
        public string? SuccessUrl { get; set; }

        /// <summary>
        /// 支付失敗後的回調URL
        /// </summary>
        public string? FailureUrl { get; set; }

        /// <summary>
        /// 元數據，用於存儲額外信息
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }
}