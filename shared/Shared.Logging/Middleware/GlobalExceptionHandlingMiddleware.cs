using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Shared.Logging.Middleware
{
    /// <summary>
    /// 全局異常處理中間件
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly ILogContextAccessor _logContextAccessor;
        private readonly bool _includeExceptionDetails;

        /// <summary>
        /// 初始化全局異常處理中間件
        /// </summary>
        /// <param name="next">請求處理管道中的下一個中間件</param>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="logContextAccessor">日誌上下文訪問器</param>
        /// <param name="includeExceptionDetails">是否在響應中包含異常詳情</param>
        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger,
            ILogContextAccessor logContextAccessor,
            bool includeExceptionDetails = false)
        {
            _next = next;
            _logger = logger;
            _logContextAccessor = logContextAccessor;
            _includeExceptionDetails = includeExceptionDetails;
        }

        /// <summary>
        /// 處理 HTTP 請求
        /// </summary>
        /// <param name="context">HTTP 上下文</param>
        /// <returns>異步任務</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// 處理異常
        /// </summary>
        /// <param name="context">HTTP 上下文</param>
        /// <param name="exception">異常</param>
        /// <returns>異步任務</returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // 獲取請求 ID
            var requestId = _logContextAccessor.GetProperty("RequestId") as string ?? Guid.NewGuid().ToString();

            // 記錄異常
            _logger.LogError(
                exception,
                "處理請求 {RequestMethod} {RequestPath} 時發生未處理的異常 (RequestId: {RequestId})",
                context.Request.Method,
                context.Request.Path,
                requestId);

            // 設置響應
            context.Response.StatusCode = (int)GetStatusCodeForException(exception);
            context.Response.ContentType = "application/json";

            // 創建錯誤響應
            var errorResponse = new
            {
                error = new
                {
                    message = GetUserFriendlyMessage(exception),
                    requestId,
                    details = _includeExceptionDetails ? exception.ToString() : null
                }
            };

            // 序列化錯誤響應
            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            // 寫入響應
            await context.Response.WriteAsync(json);
        }

        /// <summary>
        /// 獲取異常對應的 HTTP 狀態碼
        /// </summary>
        /// <param name="exception">異常</param>
        /// <returns>HTTP 狀態碼</returns>
        private static HttpStatusCode GetStatusCodeForException(Exception exception)
        {
            // 根據異常類型返回適當的狀態碼
            return exception switch
            {
                ArgumentException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                InvalidOperationException => HttpStatusCode.BadRequest,
                NotImplementedException => HttpStatusCode.NotImplemented,
                _ => HttpStatusCode.InternalServerError
            };
        }

        /// <summary>
        /// 獲取用戶友好的錯誤消息
        /// </summary>
        /// <param name="exception">異常</param>
        /// <returns>用戶友好的錯誤消息</returns>
        private static string GetUserFriendlyMessage(Exception exception)
        {
            // 根據異常類型返回適當的消息
            return exception switch
            {
                ArgumentException => exception.Message,
                KeyNotFoundException => exception.Message,
                UnauthorizedAccessException => "您沒有權限執行此操作",
                _ => "處理您的請求時發生錯誤，請稍後再試"
            };
        }
    }
}