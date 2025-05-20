using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OrderService.Models
{
    /// <summary>
    /// 訂單模型
    /// </summary>
    public class Order
    {
        /// <summary>
        /// 訂單ID (UUID格式)
        /// </summary>
        [Key]
        [MaxLength(36)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 訂單編號 (人類可讀的訂單編號)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string OrderNumber { get; set; } = null!;
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required]
        [MaxLength(36)]
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// 訂單狀態: pending, paid, processing, shipped, delivered, cancelled, refunded
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "pending";
        
        /// <summary>
        /// 訂單總金額
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// 訂單項目數量
        /// </summary>
        [Required]
        public int ItemsCount { get; set; }
        
        /// <summary>
        /// 配送地址ID
        /// </summary>
        public int? ShippingAddressId { get; set; }
        
        /// <summary>
        /// 帳單地址ID
        /// </summary>
        public int? BillingAddressId { get; set; }
        
        /// <summary>
        /// 支付ID
        /// </summary>
        [MaxLength(36)]
        public string? PaymentId { get; set; }
        
        /// <summary>
        /// 配送方式
        /// </summary>
        [MaxLength(50)]
        public string? ShippingMethod { get; set; }
        
        /// <summary>
        /// 配送費用
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ShippingFee { get; set; } = 0;
        
        /// <summary>
        /// 稅額
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TaxAmount { get; set; } = 0;
        
        /// <summary>
        /// 折扣金額
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal DiscountAmount { get; set; } = 0;
        
        /// <summary>
        /// 訂單備註
        /// </summary>
        public string? Notes { get; set; }
        
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
        /// 訂單完成時間
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// 訂單取消時間
        /// </summary>
        public DateTime? CancelledAt { get; set; }
        
        /// <summary>
        /// 取消原因
        /// </summary>
        [MaxLength(255)]
        public string? CancellationReason { get; set; }
        
        /// <summary>
        /// 額外元數據 (JSON格式)
        /// </summary>
        public string? Metadata { get; set; }
        
        /// <summary>
        /// 訂單項目集合
        /// </summary>
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        
        /// <summary>
        /// 訂單狀態歷史集合
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
        
        /// <summary>
        /// 訂單事件集合
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<OrderEvent> Events { get; set; } = new List<OrderEvent>();
    }
}