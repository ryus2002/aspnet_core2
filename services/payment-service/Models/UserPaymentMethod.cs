using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Models
{
    /// <summary>
    /// 用戶支付方式
    /// </summary>
    public class UserPaymentMethod
    {
        /// <summary>
        /// 用戶支付方式ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required]
        [StringLength(100)]
        public required string UserId { get; set; }

        /// <summary>
        /// 支付方式ID
        /// </summary>
        [Required]
        public required string PaymentMethodId { get; set; }

        /// <summary>
        /// 卡號或賬號後四位
        /// </summary>
        [StringLength(4)]
        public required string Last4 { get; set; }

        /// <summary>
        /// 支付令牌（加密存儲）
        /// </summary>
        public required string PaymentToken { get; set; }

        /// <summary>
        /// 過期年份
        /// </summary>
        public int? ExpiryYear { get; set; }

        /// <summary>
        /// 過期月份
        /// </summary>
        public int? ExpiryMonth { get; set; }

        /// <summary>
        /// 卡片類型
        /// </summary>
        [StringLength(50)]
        public required string CardType { get; set; }

        /// <summary>
        /// 是否為默認支付方式
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 關聯的支付方式
        /// </summary>
        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod? PaymentMethod { get; set; }
    }
}