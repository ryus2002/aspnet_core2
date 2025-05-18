using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Models
{
    [Table("refunds")]
    public class Refund
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [Column("payment_transaction_id")]
        public string PaymentTransactionId { get; set; }
        
        [Required]
        [Column("amount")]
        public decimal Amount { get; set; }
        
        [Column("currency")]
        public string Currency { get; set; } = "TWD";
        
        [Required]
        [Column("status")]
        public string Status { get; set; }
        
        [Column("reason")]
        public string? Reason { get; set; }
        
        [Column("notes")]
        public string? Notes { get; set; }
        
        [Column("refund_reference")]
        public string? RefundReference { get; set; }
        
        [Column("refunded_by")]
        public string? RefundedBy { get; set; }
        
        [Column("provider_response")]
        public string? ProviderResponse { get; set; }
        
        [Column("metadata")]
        public string? Metadata { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }
        
        // 導航屬性
        [ForeignKey("PaymentTransactionId")]
        public virtual PaymentTransaction? PaymentTransaction { get; set; }
    }
}