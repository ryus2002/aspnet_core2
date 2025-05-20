using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OrderService.Models
{
    /// <summary>
    /// 購物車模型
    /// </summary>
    public class Cart
    {
        /// <summary>
        /// 購物車ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        /// <summary>
        /// 用戶ID (已登入用戶)
        /// </summary>
        public string? UserId { get; set; }
        
        /// <summary>
        /// 會話ID (未登入用戶)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SessionId { get; set; } = null!;
        
        /// <summary>
        /// 狀態: active, abandoned, converted
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "active";
        
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
        
        /// <summary>
        /// 購物車過期時間
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        /// <summary>
        /// 額外元數據 (JSON格式)
        /// </summary>
        public string? Metadata { get; set; }
        
        /// <summary>
        /// 購物車項目集合
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}