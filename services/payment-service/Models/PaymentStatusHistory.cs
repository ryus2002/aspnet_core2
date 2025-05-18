using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Models
{
    /// <summary>
    /// 支付狀態歷史
    /// </summary>
    [Table("payment_status_histories")]
    public class PaymentStatusHistory
    {
        /// <summary>
        /// 歷史ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 支付交易ID
        /// </summary>
        [Required]
        public string PaymentTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// 前一個狀態
        /// </summary>
        [Required]
        public string PreviousStatus { get; set; } = string.Empty;

        /// <summary>
        /// 當前狀態
        /// </summary>
        [Required]
        [StringLength(50)]
        public string CurrentStatus { get; set; } = string.Empty;

        /// <summary>
        /// 狀態變更原因
        /// </summary>
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        /// <summary>
        /// 狀態變更時間
        /// </summary>
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 創建時間（與ChangedAt同義，為了保持一致性）
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 操作人
        /// </summary>
        public string? ChangedBy { get; set; }

        /// <summary>
        /// 附加數據
        /// </summary>
        public string AdditionalData { get; set; } = "{}";

        /// <summary>
        /// 關聯的支付交易
        /// </summary>
        [ForeignKey("PaymentTransactionId")]
        public virtual PaymentTransaction? PaymentTransaction { get; set; }
    }
}