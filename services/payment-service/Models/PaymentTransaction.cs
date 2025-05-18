using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace PaymentService.Models
{
    [Table("payment_transactions")]
    public class PaymentTransaction
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string OrderId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string PaymentMethodId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [Required]
        [StringLength(100)]
        public string TransactionReference { get; set; } = string.Empty;
        
        [Column(TypeName = "json")]
        public string PaymentProviderResponse { get; set; } = "{}";

        public string ErrorMessage { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string ClientIp { get; set; } = string.Empty;

        [StringLength(100)]
        public string PaymentIntentId { get; set; } = string.Empty;

        [Column(TypeName = "json")]
        public string Metadata { get; set; } = "{}";
        
        // 導航屬性
        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod? PaymentMethod { get; set; }
        public virtual ICollection<PaymentStatusHistory> StatusHistories { get; set; } = new List<PaymentStatusHistory>();
        public virtual ICollection<PaymentNotification> Notifications { get; set; } = new List<PaymentNotification>();
        public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

        // 需要添加的屬性
        public string Currency { get; set; } = "TWD";
        public string? Description { get; set; }
        public string? ClientDevice { get; set; }
        public string? SuccessUrl { get; set; }
        public string? FailureUrl { get; set; }
        public DateTime? PaidAt { get; set; }
        
        // 計算屬性
        public string CurrencyCode => Currency;
        public string PaymentMethodCode => PaymentMethod?.Code ?? "";
    }
}
