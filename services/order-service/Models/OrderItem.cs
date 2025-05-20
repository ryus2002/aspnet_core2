using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OrderService.Models
{
    /// <summary>
    /// 訂單項目模型
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// 訂單項目ID
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
        /// 商品名稱 (快照)
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// 數量
        /// </summary>
        [Required]
        public int Quantity { get; set; }
        
        /// <summary>
        /// 單價 (快照)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// 總價
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalPrice { get; set; }
        
        /// <summary>
        /// 商品屬性 (顏色、尺寸等) (JSON格式)
        /// </summary>
        public string? Attributes { get; set; }
        
        /// <summary>
        /// 庫存單位 (快照)
        /// </summary>
        [MaxLength(50)]
        public string? SKU { get; set; }
        
        /// <summary>
        /// 商品圖片URL (快照)
        /// </summary>
        [MaxLength(255)]
        public string? ImageUrl { get; set; }
        
        /// <summary>
        /// 所屬訂單
        /// </summary>
        [JsonIgnore]
        public virtual Order Order { get; set; } = null!;
    }
}