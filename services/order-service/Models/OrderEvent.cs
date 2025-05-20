using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OrderService.Models
{
    /// <summary>
    /// 訂單事件模型
    /// </summary>
    public class OrderEvent
    {
        /// <summary>
        /// 事件ID
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
        /// 事件類型: created, updated, cancelled, etc.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string EventType { get; set; } = null!;
        
        /// <summary>
        /// 事件數據 (JSON格式)
        /// </summary>
        [Required]
        public string Payload { get; set; } = null!;
        
        /// <summary>
        /// 是否已處理
        /// </summary>
        [Required]
        public bool Processed { get; set; } = false;
        
        /// <summary>
        /// 創建時間
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 處理時間
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
        
        /// <summary>
        /// 所屬訂單
        /// </summary>
        [JsonIgnore]
        public virtual Order Order { get; set; } = null!;
    }
}