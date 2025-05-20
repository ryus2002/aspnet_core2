using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentService.DTOs;
using PaymentService.Services;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/refunds")]
    [Produces("application/json")]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _refundService;
        private readonly ILogger<RefundController> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        public RefundController(IRefundService refundService, ILogger<RefundController> logger)
        {
            _refundService = refundService;
            _logger = logger;
        }

        /// <summary>
        /// 創建退款請求
        /// </summary>
        /// <param name="request">退款請求</param>
        /// <returns>創建的退款記錄</returns>
        [Authorize]
        [HttpPost]
        [SwaggerOperation(
            Summary = "創建退款請求",
            Description = "創建新的退款請求",
            OperationId = "CreateRefund",
            Tags = new[] { "退款" }
        )]
        [SwaggerResponse(201, "退款請求創建成功", typeof(RefundResponse))]
        [SwaggerResponse(400, "請求參數無效")]
        [SwaggerResponse(401, "未認證")]
        public async Task<IActionResult> CreateRefund([FromBody] CreateRefundRequest request)
        {
            // 在實際應用中，應該從認證令牌中獲取用戶ID
            var userId = User.Identity?.Name ?? "system";
            
            _logger.LogInformation("創建退款請求 - 交易ID: {TransactionId}, 金額: {Amount}", 
                request.PaymentTransactionId, request.Amount);
            
            var refund = await _refundService.CreateRefundRequestAsync(request, userId);
            
            return CreatedAtAction(nameof(GetRefund), new { id = refund.Id }, new RefundResponse
            {
                Id = refund.Id,
                PaymentTransactionId = refund.PaymentTransactionId,
                Amount = refund.Amount,
                Status = refund.Status,
                Reason = refund.Reason,
                CreatedAt = refund.CreatedAt,
                ProcessedAt = refund.ProcessedAt,
                RequestedBy = refund.RequestedBy
            });
        }

        /// <summary>
        /// 獲取退款詳情
        /// </summary>
        /// <param name="id">退款ID</param>
        /// <returns>退款詳情</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "獲取退款詳情",
            Description = "根據ID獲取退款詳情",
            OperationId = "GetRefund",
            Tags = new[] { "退款" }
        )]
        [SwaggerResponse(200, "成功", typeof(RefundDetailResponse))]
        [SwaggerResponse(404, "退款記錄不存在")]
        public async Task<IActionResult> GetRefund(string id)
        {
            var refund = await _refundService.GetRefundByIdAsync(id);
            if (refund == null)
                return NotFound();
            
            return Ok(new RefundDetailResponse
            {
                Id = refund.Id,
                PaymentTransactionId = refund.PaymentTransactionId,
                Amount = refund.Amount,
                Status = refund.Status,
                Reason = refund.Reason,
                CreatedAt = refund.CreatedAt,
                ProcessedAt = refund.ProcessedAt,
                RequestedBy = refund.RequestedBy,
                ExternalRefundId = refund.ExternalRefundId ?? "",
                Notes = "", // 使用空字符串，因為 Refund 模型中沒有 Notes 屬性
                ResponseData = refund.ResponseData
            });
        }

        /// <summary>
        /// 獲取交易的退款列表
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <returns>退款列表</returns>
        [HttpGet("transaction/{transactionId}")]
        [SwaggerOperation(
            Summary = "獲取交易的退款列表",
            Description = "獲取指定交易的所有退款記錄",
            OperationId = "GetRefundsByTransactionId",
            Tags = new[] { "退款" }
        )]
        [SwaggerResponse(200, "成功", typeof(RefundResponse[]))]
        public async Task<IActionResult> GetRefundsByTransactionId(string transactionId)
        {
            var refunds = await _refundService.GetRefundsByTransactionIdAsync(transactionId);
            
            // 將 Refund 模型轉換為 RefundResponse DTO
            var response = refunds.Select(r => new RefundResponse
            {
                Id = r.Id,
                PaymentTransactionId = r.PaymentTransactionId,
                Amount = r.Amount,
                Status = r.Status,
                Reason = r.Reason,
                CreatedAt = r.CreatedAt,
                ProcessedAt = r.ProcessedAt,
                RequestedBy = r.RequestedBy
            }).ToList();
            
            return Ok(response);
    }
}
}