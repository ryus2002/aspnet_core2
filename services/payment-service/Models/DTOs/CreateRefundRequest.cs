using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models.DTOs
{
    /// <summary>
    /// 創建退款請求
    /// </summary>
    public class CreateRefundRequest
    {
        /// <summary>
        /// 支付交易ID
        /// </summary>
        [Required]
        public string PaymentTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// 退款金額
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "退款金額必須大於零")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 退款原因
        /// </summary>
        [Required]
        public string? Reason { get; set; } = string.Empty;
    }
}