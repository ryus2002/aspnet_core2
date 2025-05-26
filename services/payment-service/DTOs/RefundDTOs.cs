namespace PaymentService.DTOs
{
    /// <summary>
    /// 創建退款請求
    /// </summary>
    public class CreateRefundRequest
    {
        /// <summary>
        /// 支付交易ID
        /// </summary>
        [Required]
        public string PaymentTransactionId { get; set; } = null!;
        
        /// <summary>
        /// 退款金額
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        /// <summary>
        /// 退款原因
        /// </summary>
        public string? Reason { get; set; }
    }
    
    /// <summary>
    /// 退款響應
    /// </summary>
    public class RefundResponse
    {
        /// <summary>
        /// 退款ID
        /// </summary>
        public string Id { get; set; } = null!;
        
        /// <summary>
        /// 支付交易ID
        /// </summary>
        public string PaymentTransactionId { get; set; } = null!;
        
        /// <summary>
        /// 退款金額
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// 退款狀態
        /// </summary>
        public string Status { get; set; } = null!;
        
        /// <summary>
        /// 退款原因
        /// </summary>
        public string? Reason { get; set; }
        
        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 處理時間
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
    }
    
    /// <summary>
    /// 處理退款請求
    /// </summary>
    public class ProcessRefundRequest
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; } = true;
        
        /// <summary>
        /// 回應數據
        /// </summary>
        public string? ResponseData { get; set; }
    }
}