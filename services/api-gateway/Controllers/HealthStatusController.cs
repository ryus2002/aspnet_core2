using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// 健康狀態頁面控制器，用於提供健康狀態監控頁面
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthStatusController : ControllerBase
    {
        private readonly ILogger<HealthStatusController> _logger;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="logger">日誌記錄器</param>
        public HealthStatusController(ILogger<HealthStatusController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 獲取健康狀態監控頁面
        /// </summary>
        /// <returns>健康狀態監控頁面</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("健康狀態監控頁面請求已接收");
            return Redirect("/health-status.html");
        }
    }
}