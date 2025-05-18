using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace PaymentService.Models
{
    [Table("payment_methods")]
    public class PaymentMethod
    {
        [Key]
        public required string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(50)]
        public required string Code { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }
        
        [StringLength(500)]
        public required string Description { get; set; }
        
        [StringLength(255)]
        public required string IconUrl { get; set; }
        
        [Column("display_order")]
        public int DisplayOrder { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        
        [Column("config")]
        public required string Config { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // 導航屬性
        public virtual ICollection<PaymentMethodProvider> PaymentMethodProviders { get; set; } = new List<PaymentMethodProvider>();
        public virtual ICollection<UserPaymentMethod> UserPaymentMethods { get; set; } = new List<UserPaymentMethod>();
    }
}