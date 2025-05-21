using System.Net;
using System.Text;

namespace ApiGateway.Middleware
{
    /// <summary>
    /// 请求转发中间件，用于手动处理特殊的请求转发场景
    /// 注意：大多数请求转发由Ocelot自动处理，此中间件仅用于特殊情况
    /// </summary>
    public class RequestForwardingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestForwardingMiddleware> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RequestForwardingMiddleware(
            RequestDelegate next, 
            ILogger<RequestForwardingMiddleware> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// 处理请求的主方法
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <returns>异步任务</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            
            // 检查是否需要特殊处理的路径
            if (path != null && RequiresCustomForwarding(path))
            {
                await ForwardRequestAsync(context);
                return;
            }

            // 继续处理管道
            await _next(context);
        }

        /// <summary>
        /// 检查路径是否需要自定义转发
        /// </summary>
        /// <param name="path">请求路径</param>
        /// <returns>是否需要自定义转发</returns>
        private bool RequiresCustomForwarding(string path)
        {
            // 这里定义需要特殊处理的路径
            // 例如：多服务聚合、特殊认证要求等
            var specialPaths = new[]
            {
                "/api/dashboard", // 假设这是一个需要从多个服务聚合数据的路径
                "/api/aggregated-data" // 另一个需要特殊处理的路径
            };

            return specialPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 转发请求到目标服务
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <returns>异步任务</returns>
        private async Task ForwardRequestAsync(HttpContext context)
        {
            try
            {
                var path = context.Request.Path.Value ?? "/";
                string targetService = DetermineTargetService(path);
                string targetPath = DetermineTargetPath(path);
                
                // 获取目标服务的基础URL
                string targetBaseUrl = GetServiceBaseUrl(targetService);
                string targetUrl = $"{targetBaseUrl}{targetPath}";
                
                _logger.LogInformation($"Forwarding request to: {targetUrl}");
                
                // 创建HTTP客户端
                var httpClient = _httpClientFactory.CreateClient();
                
                // 创建请求消息
                var requestMessage = new HttpRequestMessage
                {
                    Method = new HttpMethod(context.Request.Method),
                    RequestUri = new Uri(targetUrl)
                };
                
                // 复制请求头
                foreach (var header in context.Request.Headers)
                {
                    if (!requestMessage.Headers.Contains(header.Key))
                    {
                        requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }
                
                // 复制请求体
                if (context.Request.ContentLength > 0)
                {
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                    var body = await reader.ReadToEndAsync();
                    var contentType = context.Request.ContentType ?? "application/json";
                    requestMessage.Content = new StringContent(body, Encoding.UTF8, contentType);
                }
                
                // 发送请求
                var response = await httpClient.SendAsync(requestMessage);
                
                // 设置响应状态码
                context.Response.StatusCode = (int)response.StatusCode;
                
                // 复制响应头
                foreach (var header in response.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }
                
                // 复制响应体
                if (response.Content != null)
                {
                    var responseBody = await response.Content.ReadAsByteArrayAsync();
                    await context.Response.Body.WriteAsync(responseBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding request");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync("Error processing request");
            }
        }

        /// <summary>
        /// 确定目标服务
        /// </summary>
        /// <param name="path">请求路径</param>
        /// <returns>目标服务名称</returns>
        private string DetermineTargetService(string path)
        {
            // 路径不应为null，但为安全起见进行检查
            if (string.IsNullOrEmpty(path))
            {
                return "unknown-service";
            }
            
            // 根据路径确定目标服务
            if (path.StartsWith("/api/dashboard", StringComparison.OrdinalIgnoreCase))
            {
                return "dashboard-service";
            }
            
            if (path.StartsWith("/api/aggregated-data", StringComparison.OrdinalIgnoreCase))
            {
                // 这里可以实现更复杂的逻辑，例如基于请求参数或请求体确定目标服务
                return "data-aggregation-service";
            }
            
            // 默认返回
            return "unknown-service";
        }

        /// <summary>
        /// 确定目标路径
        /// </summary>
        /// <param name="path">请求路径</param>
        /// <returns>目标路径</returns>
        private string DetermineTargetPath(string path)
        {
            // 路径不应为null，但为安全起见进行检查
            if (string.IsNullOrEmpty(path))
            {
                return "/";
            }
            
            // 这里可以实现路径转换逻辑
            // 例如：/api/dashboard/user -> /api/user-dashboard
            
            if (path.StartsWith("/api/dashboard", StringComparison.OrdinalIgnoreCase))
            {
                return path.Replace("/api/dashboard", "/api");
            }
            
            if (path.StartsWith("/api/aggregated-data", StringComparison.OrdinalIgnoreCase))
            {
                return path.Replace("/api/aggregated-data", "/api/data");
            }
            
            return path;
        }

        /// <summary>
        /// 获取服务的基础URL
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>服务的基础URL</returns>
        private string GetServiceBaseUrl(string serviceName)
        {
            // 在生产环境中，这些URL可以从配置中读取
            // 在开发环境中，使用本地URL
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            
            if (isDevelopment)
            {
                return serviceName switch
                {
                    "dashboard-service" => "http://localhost:5010",
                    "data-aggregation-service" => "http://localhost:5011",
                    _ => "http://localhost:5000"
                };
            }
            else
            {
                return serviceName switch
                {
                    "dashboard-service" => "http://dashboard-service",
                    "data-aggregation-service" => "http://data-aggregation-service",
                    _ => "http://api-gateway"
                };
            }
        }
    }

    /// <summary>
    /// 请求转发中间件扩展方法
    /// </summary>
    public static class RequestForwardingMiddlewareExtensions
    {
        /// <summary>
        /// 使用自定义请求转发中间件
        /// </summary>
        /// <param name="builder">应用程序构建器</param>
        /// <returns>应用程序构建器</returns>
        public static IApplicationBuilder UseRequestForwarding(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestForwardingMiddleware>();
        }
    }
}