using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Models
{
    /// <summary>
    /// 支付系統設置
    /// </summary>
    [Table("payment_settings")]
    public class PaymentSetting
    {
        /// <summary>
        /// 設置鍵
        /// </summary>
        [Key]
        [StringLength(100)]
        public required string Key { get; set; }

        /// <summary>
        /// 設置值
        /// </summary>
        [Required]
        public required string Value { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500)]
        public required string Description { get; set; }

        /// <summary>
        /// 是否加密
        /// </summary>
        public bool IsEncrypted { get; set; }

        /// <summary>
        /// 是否系統設置
        /// </summary>
        public bool IsSystemSetting { get; set; }
        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 最後修改人
        /// </summary>
        [StringLength(100)]
        public required string LastModifiedBy { get; set; }
    }
}