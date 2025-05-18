using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Models
{
    [Table("payment_transactions")]
    public class PaymentTransaction
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [Column("order_id")]
        public string OrderId { get; set; }
        
        [Required]
        [Column("user_id")]
        public string UserId { get; set; }
        
        [Required]
        [Column("payment_method_id")]
        public string PaymentMethodId { get; set; }
        
        [Required]
        [Column("amount")]
        public decimal Amount { get; set; }
        
        [Column("currency")]
        public string Currency { get; set; } = "TWD";
        
        [Required]
        [Column("status")]
        public string Status { get; set; }
        
        [Column("transaction_reference")]
        public string? TransactionReference { get; set; }
        
        [Column("payment_provider_response")]
        public string? PaymentProviderResponse { get; set; }
        
        [Column("processing_fee")]
        public decimal ProcessingFee { get; set; } = 0;
        
        [Column("error_message")]
        public string? ErrorMessage { get; set; }
        
        [Column("client_ip")]
        public string? ClientIp { get; set; }
        
        [Column("payment_intent_id")]
        public string? PaymentIntentId { get; set; }
        
        [Column("metadata")]
        public string? Metadata { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }
        
        // 導航屬性
        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod? PaymentMethod { get; set; }
    }
}