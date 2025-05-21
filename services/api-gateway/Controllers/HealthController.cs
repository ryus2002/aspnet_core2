using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// 健康检查控制器，用于监控API Gateway的状态
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IConfiguration _configuration;

        public HealthController(ILogger<HealthController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 获取API Gateway的健康状态
        /// </summary>
        /// <returns>健康状态信息</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Health check requested");
            
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }

        /// <summary>
        /// 获取API Gateway的详细状态
        /// </summary>
        /// <returns>详细状态信息</returns>
        [HttpGet("details")]
        public IActionResult GetDetails()
        {
            _logger.LogInformation("Detailed health check requested");
            
            // 获取服务配置信息
            var services = new[]
            {
                new { Name = "AuthService", Status = "Online", Url = GetServiceUrl("auth-service") },
                new { Name = "ProductService", Status = "Online", Url = GetServiceUrl("product-service") },
                new { Name = "OrderService", Status = "Online", Url = GetServiceUrl("order-service") },
                new { Name = "PaymentService", Status = "Online", Url = GetServiceUrl("payment-service") }
            };
            
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Services = services,
                MemoryUsage = GetMemoryUsage()
            });
        }

        /// <summary>
        /// 获取服务URL
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>服务URL</returns>
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
                    _ => "http://localhost:5000"
                };
            }
            else
            {
                return $"http://{serviceName}";
            }
        }

        /// <summary>
        /// 获取内存使用情况
        /// </summary>
        /// <returns>内存使用信息</returns>
        private object GetMemoryUsage()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            
            return new
            {
                TotalMemoryMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
                ManagedMemoryMB = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2)
            };
        }
    }
}