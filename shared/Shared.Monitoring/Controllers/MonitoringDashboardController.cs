using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Shared.Monitoring.Controllers
{
    /// <summary>
    /// 監控儀表板控制器
    /// </summary>
    [ApiController]
    [Route("monitoring")]
    public class MonitoringDashboardController : Controller
    {
        private readonly IMonitoringDashboardService _dashboardService;
        private readonly ILogger<MonitoringDashboardController> _logger;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="dashboardService">儀表板服務</param>
        /// <param name="logger">日誌記錄器</param>
        public MonitoringDashboardController(
            IMonitoringDashboardService dashboardService,
            ILogger<MonitoringDashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// 顯示監控儀表板
        /// </summary>
        /// <returns>儀表板視圖</returns>
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return File("~/dashboard.html", "text/html");
        }

        /// <summary>
        /// 獲取系統指標數據
        /// </summary>
        /// <returns>系統指標數據</returns>
        [HttpGet("api/system-metrics")]
        public async Task<IActionResult> GetSystemMetrics()
        {
            try
            {
                var metrics = await _dashboardService.GetSystemMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取系統指標數據時發生錯誤");
                return StatusCode(500, new { error = "獲取系統指標數據時發生錯誤" });
            }
        }

        /// <summary>
        /// 獲取應用程式指標數據
        /// </summary>
        /// <returns>應用程式指標數據</returns>
        [HttpGet("api/application-metrics")]
        public async Task<IActionResult> GetApplicationMetrics()
        {
            try
            {
                var metrics = await _dashboardService.GetApplicationMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取應用程式指標數據時發生錯誤");
                return StatusCode(500, new { error = "獲取應用程式指標數據時發生錯誤" });
            }
        }

        /// <summary>
        /// 獲取業務指標數據
        /// </summary>
        /// <returns>業務指標數據</returns>
        [HttpGet("api/business-metrics")]
        public async Task<IActionResult> GetBusinessMetrics()
        {
            try
            {
                var metrics = await _dashboardService.GetBusinessMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取業務指標數據時發生錯誤");
                return StatusCode(500, new { error = "獲取業務指標數據時發生錯誤" });
            }
        }
    }
}