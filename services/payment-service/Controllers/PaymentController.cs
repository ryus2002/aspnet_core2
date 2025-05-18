using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentService.Models.DTOs;
using PaymentService.Services;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// 獲取支付交易詳情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(string id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentAsync(id);
            if (payment == null)
            {
                    return NotFound();
                }
            return Ok(payment);
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取支付交易失敗");
                return StatusCode(500, new { message = "獲取支付交易失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 創建支付交易
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] Models.DTOs.CreatePaymentRequest request)
        {
            try
            {
                var result = await _paymentService.CreatePaymentAsync(request);
                return Ok(result);
                }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建支付交易失敗");
                return StatusCode(500, new { message = "創建支付交易失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 根據訂單ID獲取支付交易
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetPaymentByOrderId(string orderId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
                return Ok(payments);
    }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取訂單支付交易失敗");
                return StatusCode(500, new { message = "獲取訂單支付交易失敗", error = ex.Message });
}
        }

        /// <summary>
        /// 完成支付交易
        /// </summary>
        [HttpPost("{id}/capture")]
        public async Task<IActionResult> CapturePayment(string id)
        {
            try
            {
                var result = await _paymentService.CapturePaymentAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "完成支付交易失敗");
                return StatusCode(500, new { message = "完成支付交易失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 取消支付交易
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelPayment(string id, [FromBody] Models.DTOs.CancelPaymentRequest request)
        {
            try
            {
                var result = await _paymentService.CancelPaymentAsync(id, request?.Reason);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消支付交易失敗");
                return StatusCode(500, new { message = "取消支付交易失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 獲取所有活躍的支付方式
        /// </summary>
        [HttpGet("methods")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            try
            {
                var methods = await _paymentService.GetActivePaymentMethods();
                return Ok(methods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取支付方式失敗");
                return StatusCode(500, new { message = "獲取支付方式失敗", error = ex.Message });
            }
        }

        /// <summary>
        /// 模擬完成支付（僅用於本地開發環境）
        /// </summary>
        [HttpPost("{id}/mock-complete")]
        public async Task<IActionResult> MockCompletePayment(string id, [FromQuery] bool success = true)
        {
            try
            {
                var result = await _paymentService.MockCompletePayment(id, success);
                if (!result)
                {
                    return BadRequest(new { message = "無法模擬完成支付" });
                }
                return Ok(new { message = success ? "模擬支付成功" : "模擬支付失敗" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "模擬完成支付失敗");
                return StatusCode(500, new { message = "模擬完成支付失敗", error = ex.Message });
            }
        }
    }
}