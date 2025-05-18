using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace PaymentService.Models
{
    /// <summary>
    /// 支付提供商
    /// </summary>
    [Table("payment_providers")]
    public class PaymentProvider
    {
        /// <summary>
        /// 支付提供商代碼
        /// </summary>
        [Key]
        [StringLength(50)]
        public required string Code { get; set; }

        /// <summary>
        /// 支付提供商名稱
        /// </summary>
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500)]
        public required string Description { get; set; }

        /// <summary>
        /// 配置信息（JSON格式）
        /// </summary>
        public required string Configuration { get; set; }

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
        /// 支付方式提供商關聯
        /// </summary>
        public virtual ICollection<PaymentMethodProvider> PaymentMethodProviders { get; set; } = new List<PaymentMethodProvider>();
    }
}