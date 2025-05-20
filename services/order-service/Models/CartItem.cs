using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OrderService.Models
{
    /// <summary>
    /// 購物車項目模型
    /// </summary>
    public class CartItem
    {
        /// <summary>
        /// 購物車項目ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        /// <summary>
        /// 購物車ID
        /// </summary>
        [Required]
        public int CartId { get; set; }
        
        /// <summary>
        /// 商品ID
        /// </summary>
        [Required]
        [MaxLength(36)]
        public string ProductId { get; set; } = null!;
        
        /// <summary>
        /// 商品變體ID
        /// </summary>
        [MaxLength(36)]
        public string? VariantId { get; set; }
        
        /// <summary>
        /// 數量
        /// </summary>
        [Required]
        public int Quantity { get; set; } = 1;
        
        /// <summary>
        /// 單價
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// 商品名稱 (快照)
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// 商品屬性 (顏色、尺寸等) (JSON格式)
        /// </summary>
        public string? Attributes { get; set; }
        
        /// <summary>
        /// 添加時間
        /// </summary>
        [Required]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 更新時間
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 所屬購物車
        /// </summary>
        [JsonIgnore]
        public virtual Cart Cart { get; set; } = null!;
    }
}