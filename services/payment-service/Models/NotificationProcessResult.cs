namespace PaymentService.Models
{
    /// <summary>
    /// 通知處理結果
    /// </summary>
    public class NotificationProcessResult
    {
        /// <summary>
        /// 處理是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 處理結果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}