using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Models
{
    /// <summary>
    /// 退款記錄
    /// </summary>
    [Table("refunds")]
    public class Refund
    {
        /// <summary>
        /// 退款ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 支付交易ID
        /// </summary>
        [Required]
        public string PaymentTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// 退款金額
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 退款原因
        /// </summary>
        [Required]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// 退款狀態
        /// </summary>
        [Required]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// 外部退款ID
        /// </summary>
        public string ExternalRefundId { get; set; } = string.Empty;

        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 處理時間
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// 請求人
        /// </summary>
        [Required]
        public string RequestedBy { get; set; } = string.Empty;

        /// <summary>
        /// 響應數據
        /// </summary>
        public string ResponseData { get; set; } = string.Empty;

        /// <summary>
        /// 關聯的支付交易
        /// </summary>
        [ForeignKey("PaymentTransactionId")]
        public virtual PaymentTransaction? PaymentTransaction { get; set; }
    }
}