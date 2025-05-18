namespace PaymentService.Models.DTOs
{
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
}