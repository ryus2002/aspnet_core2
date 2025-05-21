using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiGateway.Middleware
{
    /// <summary>
    /// 认证中间件，用于验证JWT令牌并处理认证逻辑
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// 处理请求的主方法
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <returns>异步任务</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // 检查是否需要跳过认证
            if (ShouldSkipAuthentication(context.Request.Path))
            {
                await _next(context);
                return;
            }

            try
            {
                // 从请求头中获取JWT令牌
                string? authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader != null && authHeader.StartsWith("Bearer "))
                {
                    string token = authHeader.Substring("Bearer ".Length).Trim();
                    
                    // 验证并解析令牌
                    var principal = await ValidateToken(token);
                    if (principal != null)
                    {
                        // 设置当前用户
                        context.User = principal;
                        
                        // 记录认证成功
                        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                        _logger.LogInformation($"Authentication successful for user: {userId}");
                        
                        // 添加用户ID到请求头，以便下游服务使用
                        // 使用Append而不是Add，以避免重复键异常
                        if (!string.IsNullOrEmpty(userId))
                        {
                            context.Request.Headers.Append("X-UserId", userId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid token provided");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication error");
            }

            await _next(context);
        }

        /// <summary>
        /// 验证JWT令牌
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>声明主体</returns>
        private Task<ClaimsPrincipal?> ValidateToken(string token)
        {
            try
            {
                // 解析JWT令牌
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                // 检查令牌是否过期
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    return Task.FromResult<ClaimsPrincipal?>(null);
                }
                
                // 创建声明身份
                var identity = new ClaimsIdentity(jwtToken.Claims, JwtBearerDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                
                return Task.FromResult<ClaimsPrincipal?>(principal);
            }
            catch
            {
                return Task.FromResult<ClaimsPrincipal?>(null);
            }
        }

        /// <summary>
        /// 检查是否应该跳过认证
        /// </summary>
        /// <param name="path">请求路径</param>
        /// <returns>是否跳过</returns>
        private bool ShouldSkipAuthentication(PathString path)
        {
            // 这些路径不需要认证
            var publicPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/refresh-token",
                "/swagger",
                "/health",
                "/api/products", // 允许未认证用户浏览商品
                "/api/categories" // 允许未认证用户浏览分类
            };

            return publicPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// 认证中间件扩展方法
    /// </summary>
    public static class AuthenticationMiddlewareExtensions
    {
        /// <summary>
        /// 使用自定义认证中间件
        /// </summary>
        /// <param name="builder">应用程序构建器</param>
        /// <returns>应用程序构建器</returns>
        public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}