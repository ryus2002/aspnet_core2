using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models.DTOs
{
    /// <summary>
    /// 取消支付請求
    /// </summary>
    public class CancelPaymentRequest
    {
        /// <summary>
        /// 取消原因
        /// </summary>
        [Required]
        public string? Reason { get; set; } = string.Empty;
    }
}