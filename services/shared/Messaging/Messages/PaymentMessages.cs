namespace Shared.Messaging.Messages
{
    /// <summary>
    /// 支付完成消息，當支付成功處理後發送
    /// </summary>
    public class PaymentCompletedMessage : BaseMessage
    {
        /// <summary>
        /// 支付交易ID
        /// </summary>
        public string TransactionId { get; set; } = null!;

        /// <summary>
        /// 訂單ID
        /// </summary>
        public string OrderId { get; set; } = null!;

        /// <summary>
        /// 用戶ID
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// 支付金額
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public string PaymentMethod { get; set; } = null!;

        /// <summary>
        /// 支付提供商
        /// </summary>
        public string Provider { get; set; } = null!;

        /// <summary>
        /// 支付時間
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// 支付狀態
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// 交易參考號（支付提供商返回的）
        /// </summary>
        public string? TransactionReference { get; set; }
    }

    /// <summary>
    /// 支付失敗消息，當支付處理失敗時發送
    /// </summary>
    public class PaymentFailedMessage : BaseMessage
    {
        /// <summary>
        /// 支付交易ID
        /// </summary>
        public string TransactionId { get; set; } = null!;

        /// <summary>
        /// 訂單ID
        /// </summary>
        public string OrderId { get; set; } = null!;

        /// <summary>
        /// 用戶ID
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// 支付金額
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 失敗原因
        /// </summary>
        public string FailureReason { get; set; } = null!;

        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 失敗時間
        /// </summary>
        public DateTime FailureDate { get; set; }

        /// <summary>
        /// 是否可以重試
        /// </summary>
        public bool CanRetry { get; set; }
    }

    /// <summary>
    /// 退款處理消息，當退款被處理時發送
    /// </summary>
    public class RefundProcessedMessage : BaseMessage
    {
        /// <summary>
        /// 退款ID
        /// </summary>
        public string RefundId { get; set; } = null!;

        /// <summary>
        /// 原支付交易ID
        /// </summary>
        public string TransactionId { get; set; } = null!;

        /// <summary>
        /// 訂單ID
        /// </summary>
        public string OrderId { get; set; } = null!;

        /// <summary>
        /// 用戶ID
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// 退款金額
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 退款原因
        /// </summary>
        public string Reason { get; set; } = null!;

        /// <summary>
        /// 退款狀態
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// 退款時間
        /// </summary>
        public DateTime RefundDate { get; set; }

        /// <summary>
        /// 處理人ID（操作員或系統）
        /// </summary>
        public string? ProcessedBy { get; set; }
    }
}