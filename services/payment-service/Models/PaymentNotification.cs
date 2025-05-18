using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Models
{
    /// <summary>
    /// 支付通知
    /// </summary>
    [Table("payment_notifications")]
    public class PaymentNotification
    {
        /// <summary>
        /// 通知ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 支付交易ID
        /// </summary>
        [Required]
        public string PaymentTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// 支付提供商代碼
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ProviderCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 原始數據
        /// </summary>
        [Required]
        public string RawData { get; set; } = string.Empty;

        /// <summary>
        /// 是否已處理
        /// </summary>
        public bool IsProcessed { get; set; }

        /// <summary>
        /// 處理時間
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// 處理結果
        /// </summary>
        public string ProcessingResult { get; set; } = string.Empty;

        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 請求頭
        /// </summary>
        public string RequestHeaders { get; set; } = string.Empty;

        /// <summary>
        /// IP地址
        /// </summary>
        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 關聯的支付交易
        /// </summary>
        [ForeignKey("PaymentTransactionId")]
        public virtual PaymentTransaction? PaymentTransaction { get; set; }
    }
}