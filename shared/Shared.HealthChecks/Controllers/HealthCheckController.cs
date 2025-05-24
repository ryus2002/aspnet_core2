using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace Shared.HealthChecks.Controllers
{
    /// <summary>
    /// 健康檢查控制器基類，提供基本的健康狀態查詢功能
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly ILogger<HealthCheckController> _logger;
        private readonly HealthCheckService _healthCheckService;
        private readonly string _serviceName;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="healthCheckService">健康檢查服務</param>
        /// <param name="serviceName">服務名稱</param>
        public HealthCheckController(
            ILogger<HealthCheckController> logger,
            HealthCheckService healthCheckService,
            string serviceName)
        {
            _logger = logger;
            _healthCheckService = healthCheckService;
            _serviceName = serviceName;
        }

        /// <summary>
        /// 獲取服務的基本健康狀態
        /// </summary>
        /// <returns>健康狀態資訊</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("健康檢查請求已接收");
            
            var report = await _healthCheckService.CheckHealthAsync();
            
            return Ok(new
            {
                Status = report.Status.ToString(),
                ServiceName = _serviceName,
                Timestamp = DateTime.UtcNow,
                Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }

        /// <summary>
        /// 獲取服務的詳細健康狀態
        /// </summary>
        /// <returns>詳細健康狀態資訊</returns>
        [HttpGet("details")]
        public async Task<IActionResult> GetDetails()
        {
            _logger.LogInformation("詳細健康檢查請求已接收");
            
            var report = await _healthCheckService.CheckHealthAsync();
            var entries = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration.TotalMilliseconds,
                    Error = entry.Value.Exception?.Message,
                    Data = entry.Value.Data
                });
            
            return Ok(new
            {
                Status = report.Status.ToString(),
                ServiceName = _serviceName,
                Timestamp = DateTime.UtcNow,
                Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Entries = entries,
                SystemInfo = GetSystemInfo()
            });
        }

        /// <summary>
        /// 獲取系統資訊
        /// </summary>
        /// <returns>系統資訊</returns>
        private object GetSystemInfo()
        {
            var process = Process.GetCurrentProcess();
            
            return new
            {
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                Memory = new
                {
                    TotalMemoryMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
                    ManagedMemoryMB = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2)
                },
                Process = new
                {
                    Id = process.Id,
                    StartTime = process.StartTime.ToUniversalTime(),
                    Uptime = (DateTime.Now - process.StartTime).ToString(@"dd\.hh\:mm\:ss"),
                    ThreadCount = process.Threads.Count
                }
            };
        }
    }
}