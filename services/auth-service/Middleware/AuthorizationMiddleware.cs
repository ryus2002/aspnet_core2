using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthService.Attributes;
using AuthService.Services;
using AuthService.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Middleware
{
    /// <summary>
    /// 授權中間件，用於驗證請求中的JWT令牌並檢查用戶權限
    /// </summary>
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthorizationMiddleware> _logger;
        private readonly JwtSettings _jwtSettings;
        
        /// <summary>
        /// 構造函數，注入依賴項
        /// </summary>
        /// <param name="next">請求處理管道中的下一個中間件</param>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="jwtSettings">JWT設置</param>
        public AuthorizationMiddleware(
            RequestDelegate next,
            ILogger<AuthorizationMiddleware> logger,
            IOptions<JwtSettings> jwtSettings)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        }
        
        /// <summary>
        /// 處理HTTP請求
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <param name="permissionService">權限服務</param>
        /// <returns>異步任務</returns>
        public async Task InvokeAsync(HttpContext context, IPermissionService permissionService)
        {
            try
            {
                // 檢查請求路徑是否需要跳過授權
                if (ShouldSkipAuthorization(context.Request.Path))
                {
                    await _next(context);
                    return;
                }
                
                // 從請求標頭中獲取授權令牌
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    _logger.LogWarning("請求缺少有效的授權標頭");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("未授權：缺少有效的令牌");
                    return;
                }
                
                // 提取令牌
                var token = authHeader.Substring("Bearer ".Length).Trim();
                
                // 驗證令牌
                var principal = ValidateToken(token);
                if (principal == null)
                {
                    _logger.LogWarning("無效的JWT令牌");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("未授權：無效的令牌");
                    return;
                }
                
                // 從令牌中提取用戶ID
                var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("JWT令牌中缺少用戶ID");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("未授權：令牌中缺少用戶ID");
                    return;
                }
                
                // 設置用戶身份
                context.User = principal;
                
                // 獲取請求的資源和操作
                var endpoint = context.GetEndpoint();
                if (endpoint != null)
                {
                    // 檢查是否有RequirePermission屬性
                    var requirePermissionAttribute = endpoint.Metadata.GetMetadata<RequirePermissionAttribute>();
                    if (requirePermissionAttribute != null)
                    {
                        // 檢查用戶是否擁有所需的權限
                        bool hasPermission;
                        
                        if (!string.IsNullOrEmpty(requirePermissionAttribute.PermissionName))
                        {
                            // 根據權限名稱檢查
                            hasPermission = await permissionService.UserHasPermissionByName(
                                userId, requirePermissionAttribute.PermissionName);
                        }
                        else
                        {
                            // 根據資源和操作檢查
                            hasPermission = await permissionService.UserHasPermission(
                                userId, requirePermissionAttribute.Resource, requirePermissionAttribute.Action);
                        }
                        
                        if (!hasPermission)
                        {
                            _logger.LogWarning("用戶缺少所需權限: {UserId}, {Resource}, {Action}",
                                userId, requirePermissionAttribute.Resource, requirePermissionAttribute.Action);
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("禁止訪問：缺少所需權限");
                            return;
                        }
                    }
                }
                
                // 繼續處理請求
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "授權中間件處理請求時發生錯誤");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("處理授權時發生錯誤");
            }
        }
        
        /// <summary>
        /// 驗證JWT令牌
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>聲明主體，如果令牌無效則返回null</returns>
        private ClaimsPrincipal? ValidateToken(string token)  // 添加 ? 表示可能返回 null
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
                
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                
                // 檢查令牌類型
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
                
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證JWT令牌時發生錯誤");
                return null;
            }
        }
        
        /// <summary>
        /// 檢查是否應該跳過授權
        /// </summary>
        /// <param name="path">請求路徑</param>
        /// <returns>是否應該跳過授權</returns>
        private bool ShouldSkipAuthorization(PathString path)
        {
            // 定義不需要授權的路徑列表
            var skipPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/refresh-token",
                "/health",
                "/metrics",
                "/swagger"
            };
            
            return skipPaths.Any(p => path.StartsWithSegments(new PathString(p)));
        }
    }
}