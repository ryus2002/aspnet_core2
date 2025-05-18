using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace PaymentService.Models
{
    [Table("payment_method_providers")]
    public class PaymentMethodProvider
    {
        [Required]
        [Column("payment_method_id")]
        public required string PaymentMethodId { get; set; }

        [Required]
        [Column("payment_provider_id")]
        public required string PaymentProviderId { get; set; }

        [Column("is_default")]
        public bool IsDefault { get; set; }

        [Column("config")]
        public string? Config { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // 導覽屬性
        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod? PaymentMethod { get; set; }
        
        [ForeignKey("PaymentProviderId")]
        public virtual PaymentProvider? PaymentProvider { get; set; }

        // 無參數建構函式
        public PaymentMethodProvider()
        {
        }

        // 帶參數建構函式
        public PaymentMethodProvider(string paymentMethodId, string paymentProviderId, bool isDefault = false, string? config = null)
        {
            PaymentMethodId = paymentMethodId;
            PaymentProviderId = paymentProviderId;
            IsDefault = isDefault;
            Config = config;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
