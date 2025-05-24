using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// 健康檢查控制器，用於監控API Gateway和所有微服務的狀態
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="configuration">配置</param>
        /// <param name="httpClientFactory">HTTP客戶端工廠</param>
        public HealthController(
            ILogger<HealthController> logger, 
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 獲取API Gateway的健康狀態
        /// </summary>
        /// <returns>健康狀態資訊</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("健康檢查請求已接收");
            
            return Ok(new
            {
                Status = "Healthy",
                ServiceName = "ApiGateway",
                Timestamp = DateTime.UtcNow,
                Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }

        /// <summary>
        /// 獲取API Gateway和所有微服務的詳細健康狀態
        /// </summary>
        /// <returns>詳細健康狀態資訊</returns>
        [HttpGet("details")]
        public async Task<IActionResult> GetDetails()
        {
            _logger.LogInformation("詳細健康檢查請求已接收");
            
            // 獲取所有微服務的健康狀態
            var services = await GetServicesHealthStatus();
            
            return Ok(new
            {
                Status = services.All(s => s.Status == "Healthy") ? "Healthy" : "Unhealthy",
                ServiceName = "ApiGateway",
                Timestamp = DateTime.UtcNow,
                Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Services = services,
                SystemInfo = GetSystemInfo()
            });
        }

        /// <summary>
        /// 獲取所有微服務的健康狀態
        /// </summary>
        /// <returns>服務健康狀態列表</returns>
        private async Task<List<ServiceHealthStatus>> GetServicesHealthStatus()
        {
            var services = new List<ServiceHealthStatus>
            {
                new ServiceHealthStatus 
                { 
                    Name = "ApiGateway", 
                    Status = "Healthy", 
                    Url = GetServiceUrl("api-gateway"),
                    Details = new Dictionary<string, object>
                    {
                        { "MemoryUsage", GetMemoryUsage() }
                    }
                }
            };

            // 定義要檢查的微服務
            var microservices = new[]
            {
                new { Name = "AuthService", Key = "auth-service" },
                new { Name = "ProductService", Key = "product-service" },
                new { Name = "OrderService", Key = "order-service" },
                new { Name = "PaymentService", Key = "payment-service" }
            };

            // 創建 HTTP 客戶端
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            // 檢查每個微服務的健康狀態
            foreach (var service in microservices)
            {
                var serviceUrl = GetServiceUrl(service.Key);
                var healthUrl = $"{serviceUrl}/health";
                var serviceStatus = new ServiceHealthStatus
                {
                    Name = service.Name,
                    Url = serviceUrl,
                    Status = "Unknown",
                    Details = new Dictionary<string, object>()
                };

                try
                {
                    var response = await httpClient.GetAsync(healthUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var healthData = JsonSerializer.Deserialize<JsonElement>(content);
                        
                        serviceStatus.Status = healthData.GetProperty("status").GetString() ?? "Unknown";
                        
                        // 嘗試獲取更多詳細資訊
                        try
                        {
                            if (healthData.TryGetProperty("timestamp", out var timestamp))
                            {
                                serviceStatus.Details["Timestamp"] = timestamp.GetString() ?? "";
                            }
                            
                            if (healthData.TryGetProperty("version", out var version))
                            {
                                serviceStatus.Details["Version"] = version.GetString() ?? "";
                            }
                        }
                        catch
                        {
                            // 忽略解析詳細資訊時的錯誤
                        }
                    }
                    else
                    {
                        serviceStatus.Status = "Unhealthy";
                        serviceStatus.Details["Error"] = $"HTTP錯誤: {(int)response.StatusCode} {response.ReasonPhrase}";
                    }
                }
                catch (Exception ex)
                {
                    serviceStatus.Status = "Unhealthy";
                    serviceStatus.Details["Error"] = $"連接錯誤: {ex.Message}";
                    _logger.LogWarning(ex, "檢查服務 {ServiceName} 的健康狀態時發生錯誤", service.Name);
                }

                services.Add(serviceStatus);
            }

            return services;
        }

        /// <summary>
        /// 獲取服務URL
        /// </summary>
        /// <param name="serviceName">服務名稱</param>
        /// <returns>服務URL</returns>
        private string GetServiceUrl(string serviceName)
        {
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            
            if (isDevelopment)
            {
                return serviceName switch
                {
                    "auth-service" => "http://localhost:5001",
                    "product-service" => "http://localhost:5002",
                    "order-service" => "http://localhost:5003",
                    "payment-service" => "http://localhost:5004",
                    "api-gateway" => "http://localhost:5000",
                    _ => "http://localhost:5000"
                };
            }
            else
            {
                return $"http://{serviceName}";
            }
        }

        /// <summary>
        /// 獲取內存使用情況
        /// </summary>
        /// <returns>內存使用資訊</returns>
        private object GetMemoryUsage()
        {
            var process = Process.GetCurrentProcess();
            
            return new
            {
                TotalMemoryMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
                ManagedMemoryMB = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2)
            };
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

    /// <summary>
    /// 服務健康狀態類
    /// </summary>
    public class ServiceHealthStatus
    {
        /// <summary>
        /// 服務名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 服務狀態
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// 服務URL
        /// </summary>
        public string Url { get; set; } = string.Empty;
        
        /// <summary>
        /// 詳細資訊
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new();
    }
}