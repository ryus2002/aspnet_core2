using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Shared.Monitoring;
namespace ProductService.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly IMetricsCollector _metricsCollector;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IMetricsCollector metricsCollector, ILogger<HealthController> logger)
        {
            _metricsCollector = metricsCollector;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Check()
        {
            try
            {
                // 記錄健康檢查事件
                _metricsCollector.IncrementCounter("system.health.check");

                // 在實際應用中，這裡應該檢查資料庫連接、外部服務等
                var healthStatus = new
                {
                    status = "healthy",
                    version = "1.0.0",
                    timestamp = DateTime.UtcNow,
                    checks = new List<object>
                    {
                        new
                        {
                            name = "database",
                            status = "healthy",
                            message = "MongoDB 連接正常"
                        },
                        new
                        {
                            name = "message_bus",
                            status = "healthy",
                            message = "RabbitMQ 連接正常"
                        }
                    }
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康檢查失敗");
                
                return StatusCode(500, new
                {
                    status = "unhealthy",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("metrics")]
        public IActionResult Metrics()
        {
            try
            {
                // 這個端點會由 App.Metrics 自動處理，返回 Prometheus 格式的指標
                // 這裡只是為了提供一個簡單的說明
                return Ok(new
                {
                    message = "請使用 /metrics 端點獲取 Prometheus 格式的指標數據"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取指標失敗");
                
                return StatusCode(500, new
                {
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
