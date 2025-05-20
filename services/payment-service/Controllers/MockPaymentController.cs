using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentService.Services;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace PaymentService.Controllers
{
    /// <summary>
    /// 模擬支付控制器
    /// 僅用於本地開發環境
    /// </summary>
    [ApiController]
    [Route("api/mock-payments")]
    [Produces("application/json")]
    public class MockPaymentController : ControllerBase
    {
        private readonly IMockPaymentService _mockPaymentService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<MockPaymentController> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        public MockPaymentController(
            IMockPaymentService mockPaymentService,
            IPaymentService paymentService,
            ILogger<MockPaymentController> logger)
        {
            _mockPaymentService = mockPaymentService;
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// 模擬支付處理
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <param name="shouldSucceed">是否成功</param>
        /// <param name="delaySeconds">延遲秒數</param>
        /// <returns>處理結果</returns>
        [HttpPost("{transactionId}/process")]
        [SwaggerOperation(
            Summary = "模擬支付處理",
            Description = "模擬處理支付交易，可指定成功或失敗",
            OperationId = "ProcessMockPayment",
            Tags = new[] { "模擬支付" }
        )]
        [SwaggerResponse(200, "處理成功", typeof(MockPaymentResult))]
        [SwaggerResponse(404, "交易不存在")]
        [SwaggerResponse(400, "處理失敗")]
        public async Task<IActionResult> ProcessPayment(
            string transactionId, 
            [FromQuery] bool shouldSucceed = true, 
            [FromQuery] int delaySeconds = 3)
        {
            _logger.LogInformation("收到模擬支付處理請求 - 交易ID: {TransactionId}", transactionId);

            var result = await _mockPaymentService.ProcessPaymentAsync(transactionId, shouldSucceed, delaySeconds);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 模擬退款處理
        /// </summary>
        /// <param name="refundId">退款ID</param>
        /// <param name="shouldSucceed">是否成功</param>
        /// <param name="delaySeconds">延遲秒數</param>
        /// <returns>處理結果</returns>
        [HttpPost("refunds/{refundId}/process")]
        [SwaggerOperation(
            Summary = "模擬退款處理",
            Description = "模擬處理退款請求，可指定成功或失敗",
            OperationId = "ProcessMockRefund",
            Tags = new[] { "模擬支付" }
        )]
        [SwaggerResponse(200, "處理成功", typeof(MockRefundResult))]
        [SwaggerResponse(404, "退款記錄不存在")]
        [SwaggerResponse(400, "處理失敗")]
        public async Task<IActionResult> ProcessRefund(
            string refundId, 
            [FromQuery] bool shouldSucceed = true, 
            [FromQuery] int delaySeconds = 3)
        {
            _logger.LogInformation("收到模擬退款處理請求 - 退款ID: {RefundId}", refundId);

            var result = await _mockPaymentService.ProcessRefundAsync(refundId, shouldSucceed, delaySeconds);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 獲取支付方式列表
        /// </summary>
        /// <returns>支付方式列表</returns>
        [HttpGet("methods")]
        [SwaggerOperation(
            Summary = "獲取支付方式列表",
            Description = "獲取可用的支付方式列表",
            OperationId = "GetMockPaymentMethods",
            Tags = new[] { "模擬支付" }
        )]
        [SwaggerResponse(200, "成功")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            var methods = await _paymentService.GetActivePaymentMethods();
            return Ok(methods);
        }

        /// <summary>
        /// 模擬支付頁面
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <returns>模擬支付頁面HTML</returns>
        [HttpGet("{transactionId}/page")]
        [Produces("text/html")]
        [SwaggerOperation(
            Summary = "獲取模擬支付頁面",
            Description = "獲取用於模擬支付的HTML頁面",
            OperationId = "GetMockPaymentPage",
            Tags = new[] { "模擬支付" }
        )]
        [SwaggerResponse(200, "成功")]
        [SwaggerResponse(404, "交易不存在")]
        public async Task<IActionResult> GetPaymentPage(string transactionId)
        {
            var transaction = await _paymentService.GetPaymentAsync(transactionId);
            if (transaction == null)
            {
                return NotFound();
            }

            // 生成簡單的HTML頁面用於模擬支付
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>模擬支付頁面</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 5px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #333; }}
        .info {{ margin: 20px 0; }}
        .info div {{ margin: 10px 0; }}
        .label {{ font-weight: bold; display: inline-block; width: 120px; }}
        .buttons {{ margin-top: 30px; }}
        .btn {{ padding: 10px 20px; border: none; border-radius: 4px; cursor: pointer; font-size: 16px; margin-right: 10px; }}
        .btn-success {{ background-color: #4CAF50; color: white; }}
        .btn-danger {{ background-color: #f44336; color: white; }}
        .result {{ margin-top: 20px; padding: 10px; border-radius: 4px; display: none; }}
        .success {{ background-color: #dff0d8; color: #3c763d; }}
        .error {{ background-color: #f2dede; color: #a94442; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>模擬支付頁面</h1>
        <div class='info'>
            <div><span class='label'>訂單編號:</span> {transaction.OrderId}</div>
            <div><span class='label'>金額:</span> {transaction.Amount} {transaction.Currency}</div>
            <div><span class='label'>支付方式:</span> {transaction.PaymentMethod?.Name ?? "未知"}</div>
            <div><span class='label'>交易狀態:</span> <span id='status'>{transaction.Status}</span></div>
        </div>
        <div class='buttons'>
            <button class='btn btn-success' id='btnSuccess'>模擬支付成功</button>
            <button class='btn btn-danger' id='btnFail'>模擬支付失敗</button>
        </div>
        <div id='resultSuccess' class='result success'>
            支付成功！正在跳轉到商店...
        </div>
        <div id='resultError' class='result error'>
            支付失敗！請重試或選擇其他支付方式...
        </div>
    </div>

    <script>
        document.getElementById('btnSuccess').addEventListener('click', function() {{
            processPayment(true);
        }});

        document.getElementById('btnFail').addEventListener('click', function() {{
            processPayment(false);
        }});

        function processPayment(success) {{
            // 禁用按鈕，防止重複點擊
            document.getElementById('btnSuccess').disabled = true;
            document.getElementById('btnFail').disabled = true;
            
            // 顯示處理中
            document.getElementById('status').textContent = '處理中...';
            
            // 發送請求到模擬支付處理API
            fetch('/api/mock-payments/{transactionId}/process?shouldSucceed=' + success, {{
                method: 'POST',
                headers: {{
                    'Content-Type': 'application/json'
                }}
            }})
            .then(response => response.json())
            .then(data => {{
                console.log('Payment result:', data);
                
                // 更新狀態
                document.getElementById('status').textContent = data.transactionStatus;
                
                // 顯示結果
                if (success) {{
                    document.getElementById('resultSuccess').style.display = 'block';
                    // 成功後跳轉
                    setTimeout(function() {{
                        window.location.href = '{transaction.SuccessUrl}';
                    }}, 3000);
                }} else {{
                    document.getElementById('resultError').style.display = 'block';
                    // 失敗後跳轉
                    setTimeout(function() {{
                        window.location.href = '{transaction.FailureUrl}';
                    }}, 3000);
                }}
            }})
            .catch(error => {{
                console.error('Error:', error);
                document.getElementById('status').textContent = '處理錯誤';
                document.getElementById('resultError').style.display = 'block';
                document.getElementById('resultError').textContent = '處理請求時出錯: ' + error.message;
            }});
        }}
    </script>
</body>
</html>";

            return Content(html, "text/html");
        }
    }
}