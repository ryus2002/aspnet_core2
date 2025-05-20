using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Models
{
    /// <summary>
    /// 地址模型
    /// </summary>
    public class Address
    {
        /// <summary>
        /// 地址ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required]
        [MaxLength(36)]
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// 收件人/帳單人姓名
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// 電話
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = null!;
        
        /// <summary>
        /// 地址行1
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string AddressLine1 { get; set; } = null!;
        
        /// <summary>
        /// 地址行2
        /// </summary>
        [MaxLength(255)]
        public string? AddressLine2 { get; set; }
        
        /// <summary>
        /// 城市
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = null!;
        
        /// <summary>
        /// 州/省
        /// </summary>
        [MaxLength(100)]
        public string? State { get; set; }
        
        /// <summary>
        /// 郵政編碼
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; } = null!;
        
        /// <summary>
        /// 國家
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Country { get; set; } = null!;
        
        /// <summary>
        /// 是否為默認地址
        /// </summary>
        [Required]
        public bool IsDefault { get; set; } = false;
        
        /// <summary>
        /// 地址類型: shipping, billing, both
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string AddressType { get; set; } = "both";
        
        /// <summary>
        /// 創建時間
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 更新時間
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}