using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OrderService.Models
{
    /// <summary>
    /// 訂單狀態歷史模型
    /// </summary>
    public class OrderStatusHistory
    {
        /// <summary>
        /// 歷史記錄ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        /// <summary>
        /// 訂單ID
        /// </summary>
        [Required]
        [MaxLength(36)]
        public string OrderId { get; set; } = null!;
        
        /// <summary>
        /// 訂單狀態
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = null!;
        
        /// <summary>
        /// 狀態變更說明
        /// </summary>
        public string? Comment { get; set; }
        
        /// <summary>
        /// 操作人ID
        /// </summary>
        [MaxLength(36)]
        public string? ChangedBy { get; set; }
        
        /// <summary>
        /// 變更時間
        /// </summary>
        [Required]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 所屬訂單
        /// </summary>
        [JsonIgnore]
        public virtual Order Order { get; set; } = null!;
    }
}