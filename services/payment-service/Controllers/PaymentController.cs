using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Services;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        
        /// <summary>
        /// 建構函數
        /// </summary>
        /// <param name="paymentService">支付服務</param>
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// 創建支付交易
        /// </summary>
        /// <remarks>
        /// 創建新的支付交易
        /// </remarks>
        /// <param name="request">創建支付請求</param>
        /// <returns>創建的支付交易</returns>
        /// <response code="201">支付交易創建成功</response>
        /// <response code="400">請求參數無效</response>
        /// <response code="401">未認證</response>
        [Authorize]
        [HttpPost]
        [SwaggerOperation(
            Summary = "創建支付交易",
            Description = "創建新的支付交易",
            OperationId = "CreatePayment",
            Tags = new[] { "支付" }
        )]
        [SwaggerResponse(201, "支付交易創建成功", typeof(PaymentTransactionResponse))]
        [SwaggerResponse(400, "請求參數無效")]
        [SwaggerResponse(401, "未認證")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            var payment = await _paymentService.CreatePaymentAsync(request);
            return CreatedAtAction(nameof(GetPayment), new { id = payment.TransactionId }, payment);
        }

        /// <summary>
        /// 獲取支付交易
        /// </summary>
        /// <remarks>
        /// 根據ID獲取支付交易詳情
        /// </remarks>
        /// <param name="id">支付交易ID</param>
        /// <returns>支付交易詳情</returns>
        /// <response code="200">成功</response>
        /// <response code="404">支付交易不存在</response>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "獲取支付交易",
            Description = "根據ID獲取支付交易詳情",
            OperationId = "GetPayment",
            Tags = new[] { "支付" }
        )]
        [SwaggerResponse(200, "成功", typeof(PaymentTransactionDetailResponse))]
        [SwaggerResponse(404, "支付交易不存在")]
        public async Task<IActionResult> GetPayment(string id)
        {
            var payment = await _paymentService.GetPaymentAsync(id);
            if (payment == null)
                return NotFound();
            return Ok(payment);
        }

        /// <summary>
        /// 根據訂單ID獲取支付交易
        /// </summary>
        /// <remarks>
        /// 根據訂單ID獲取相關的支付交易
        /// </remarks>
        /// <param name="orderId">訂單ID</param>
        /// <returns>支付交易列表</returns>
        /// <response code="200">成功</response>
        /// <response code="404">訂單不存在</response>
        [HttpGet("order/{orderId}")]
        [SwaggerOperation(
            Summary = "根據訂單ID獲取支付交易",
            Description = "根據訂單ID獲取相關的支付交易",
            OperationId = "GetPaymentByOrderId",
            Tags = new[] { "支付" }
        )]
        [SwaggerResponse(200, "成功", typeof(PaymentTransactionResponse[]))]
        [SwaggerResponse(404, "訂單不存在")]
        public async Task<IActionResult> GetPaymentByOrderId(string orderId)
        {
            var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
            return Ok(payments);
        }

        /// <summary>
        /// 獲取支付方式
        /// </summary>
        /// <remarks>
        /// 獲取所有可用的支付方式
        /// </remarks>
        /// <returns>支付方式列表</returns>
        /// <response code="200">成功</response>
        [HttpGet("methods")]
        [SwaggerOperation(
            Summary = "獲取支付方式",
            Description = "獲取所有可用的支付方式",
            OperationId = "GetPaymentMethods",
            Tags = new[] { "支付" }
        )]
        [SwaggerResponse(200, "成功", typeof(PaymentMethodResponse[]))]
        public async Task<IActionResult> GetPaymentMethods()
        {
            var methods = await _paymentService.GetActivePaymentMethods();
            return Ok(methods);
        }

        /// <summary>
        /// 完成支付
        /// </summary>
        /// <remarks>
        /// 完成指定的支付交易
        /// </remarks>
        /// <param name="id">支付交易ID</param>
        /// <returns>更新後的支付交易</returns>
        /// <response code="200">支付完成成功</response>
        /// <response code="400">請求參數無效</response>
        /// <response code="401">未認證</response>
        /// <response code="404">支付交易不存在</response>
        [Authorize]
        [HttpPost("{id}/capture")]
        [SwaggerOperation(
            Summary = "完成支付",
            Description = "完成指定的支付交易",
            OperationId = "CapturePayment",
            Tags = new[] { "支付" }
        )]
        [SwaggerResponse(200, "支付完成成功", typeof(PaymentTransactionResponse))]
        [SwaggerResponse(400, "請求參數無效")]
        [SwaggerResponse(401, "未認證")]
        [SwaggerResponse(404, "支付交易不存在")]
        public async Task<IActionResult> CapturePayment(string id)
        {
            var payment = await _paymentService.CapturePaymentAsync(id);
            return Ok(payment);
        }

        /// <summary>
        /// 取消支付
        /// </summary>
        /// <remarks>
        /// 取消指定的支付交易
        /// </remarks>
        /// <param name="id">支付交易ID</param>
        /// <param name="request">取消支付請求</param>
        /// <returns>更新後的支付交易</returns>
        /// <response code="200">支付取消成功</response>
        /// <response code="400">請求參數無效</response>
        /// <response code="401">未認證</response>
        /// <response code="404">支付交易不存在</response>
        [Authorize]
        [HttpPost("{id}/cancel")]
        [SwaggerOperation(
            Summary = "取消支付",
            Description = "取消指定的支付交易",
            OperationId = "CancelPayment",
            Tags = new[] { "支付" }
        )]
        [SwaggerResponse(200, "支付取消成功", typeof(PaymentTransactionResponse))]
        [SwaggerResponse(400, "請求參數無效")]
        [SwaggerResponse(401, "未認證")]
        [SwaggerResponse(404, "支付交易不存在")]
        public async Task<IActionResult> CancelPayment(string id, [FromBody] CancelPaymentRequest request)
        {
            var payment = await _paymentService.CancelPaymentAsync(id, request.Reason);
            return Ok(payment);
        }

        /// <summary>
        /// 模擬完成支付
        /// </summary>
        /// <remarks>
        /// 僅用於本地開發環境，模擬完成支付
        /// </remarks>
        /// <param name="id">支付交易ID</param>
        /// <param name="success">是否成功</param>
        /// <returns>操作結果</returns>
        /// <response code="200">操作成功</response>
        /// <response code="400">請求參數無效</response>
        /// <response code="404">支付交易不存在</response>
        [HttpPost("{id}/mock-complete")]
        [SwaggerOperation(
            Summary = "模擬完成支付",
            Description = "僅用於本地開發環境，模擬完成支付",
            OperationId = "MockCompletePayment",
            Tags = new[] { "開發工具" }
        )]
        [SwaggerResponse(200, "操作成功")]
        [SwaggerResponse(400, "請求參數無效")]
        [SwaggerResponse(404, "支付交易不存在")]
        public async Task<IActionResult> MockCompletePayment(string id, [FromQuery] bool success = true)
        {
            var result = await _paymentService.MockCompletePayment(id, success);
            if (!result)
                return NotFound();
            return Ok(new { success = true, message = "模擬支付處理完成" });
        }
    }
}