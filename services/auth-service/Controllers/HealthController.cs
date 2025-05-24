using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.HealthChecks.Controllers;

namespace AuthService.Controllers
{
    /// <summary>
    /// 認證服務健康檢查控制器
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthController : HealthCheckController
    {
        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="healthCheckService">健康檢查服務</param>
        public HealthController(
            ILogger<HealthController> logger,
            HealthCheckService healthCheckService)
            : base(logger, healthCheckService, "AuthService")
        {
        }
    }
}