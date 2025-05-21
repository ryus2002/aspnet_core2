using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// Swagger控制器，用于提供Swagger UI的入口点
    /// </summary>
    [ApiController]
    [Route("api-docs")]
    public class SwaggerController : ControllerBase
    {
        private readonly ILogger<SwaggerController> _logger;

        public SwaggerController(ILogger<SwaggerController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取API文档索引
        /// </summary>
        /// <returns>API文档信息</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("API documentation index requested");
            
            var services = new[]
            {
                new { Name = "Authentication API", Url = "/swagger/auth/index.html", Description = "用户认证与授权服务" },
                new { Name = "Product API", Url = "/swagger/products/index.html", Description = "商品管理服务" },
                new { Name = "Order API", Url = "/swagger/orders/index.html", Description = "订单管理服务" },
                new { Name = "Payment API", Url = "/swagger/payments/index.html", Description = "支付处理服务" },
                new { Name = "API Gateway", Url = "/swagger/index.html", Description = "API网关服务" }
            };
            
            return Ok(new
            {
                Title = "电商微服务平台 API 文档",
                Version = "1.0",
                Description = "这是电商微服务平台的API文档入口，您可以通过以下链接访问各个服务的API文档。",
                Services = services
            });
        }
    }
}