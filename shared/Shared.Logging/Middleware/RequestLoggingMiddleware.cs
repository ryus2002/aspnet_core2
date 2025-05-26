using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Shared.Logging.Middleware
{
    /// <summary>
    /// HTTP 請求日誌中間件
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly ILogContextAccessor _logContextAccessor;

        /// <summary>
        /// 初始化請求日誌中間件
        /// </summary>
        /// <param name="next">請求處理管道中的下一個中間件</param>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="logContextAccessor">日誌上下文訪問器</param>
        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger,
            ILogContextAccessor logContextAccessor)
        {
            _next = next;
            _logger = logger;
            _logContextAccessor = logContextAccessor;
        }

        /// <summary>
        /// 處理 HTTP 請求
        /// </summary>
        /// <param name="context">HTTP 上下文</param>
        /// <returns>異步任務</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // 生成請求 ID
            var requestId = Guid.NewGuid().ToString();
            _logContextAccessor.SetProperty("RequestId", requestId);
            context.Response.Headers["X-Request-Id"] = requestId;

            // 記錄請求開始
            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;
            var requestQueryString = context.Request.QueryString.ToString();
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            _logger.LogInformation(
                "開始處理請求 {RequestMethod} {RequestPath}{RequestQueryString} 來自 {ClientIp} 使用 {UserAgent}",
                requestMethod, requestPath, requestQueryString, clientIp, userAgent);

            // 允許讀取請求體
            context.Request.EnableBuffering();

            // 捕獲請求體
            if (context.Request.ContentLength > 0 && context.Request.ContentLength < 10240) // 限制大小為 10KB
            {
                var buffer = new byte[context.Request.ContentLength.Value];
                await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                var requestBody = Encoding.UTF8.GetString(buffer);
                _logger.LogDebug("請求體: {RequestBody}", requestBody);

                // 重置請求體位置
                context.Request.Body.Position = 0;
            }

            // 捕獲響應
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                // 調用下一個中間件
                await _next(context);

                // 捕獲響應體
                responseBodyStream.Position = 0;
                var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
                responseBodyStream.Position = 0;

                // 將響應體複製回原始流
                await responseBodyStream.CopyToAsync(originalBodyStream);

                // 記錄響應
                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                // 根據狀態碼選擇日誌級別
                if (statusCode >= 500)
                {
                    _logger.LogError(
                        "完成請求 {RequestMethod} {RequestPath} - {StatusCode} 在 {ElapsedMs}ms",
                        requestMethod, requestPath, statusCode, elapsedMs);

                    if (responseBody.Length < 10240) // 限制大小為 10KB
                    {
                        _logger.LogError("響應體: {ResponseBody}", responseBody);
                    }
                }
                else if (statusCode >= 400)
                {
                    _logger.LogWarning(
                        "完成請求 {RequestMethod} {RequestPath} - {StatusCode} 在 {ElapsedMs}ms",
                        requestMethod, requestPath, statusCode, elapsedMs);

                    if (responseBody.Length < 10240) // 限制大小為 10KB
                    {
                        _logger.LogWarning("響應體: {ResponseBody}", responseBody);
                    }
                }
                else
                {
                    _logger.LogInformation(
                        "完成請求 {RequestMethod} {RequestPath} - {StatusCode} 在 {ElapsedMs}ms",
                        requestMethod, requestPath, statusCode, elapsedMs);

                    if (responseBody.Length < 10240 && _logger.IsEnabled(LogLevel.Debug)) // 限制大小為 10KB
                    {
                        _logger.LogDebug("響應體: {ResponseBody}", responseBody);
                    }
                }
            }
            catch (Exception ex)
            {
                // 記錄異常
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                _logger.LogError(
                    ex,
                    "處理請求 {RequestMethod} {RequestPath} 時發生異常，耗時 {ElapsedMs}ms",
                    requestMethod, requestPath, elapsedMs);

                // 重新拋出異常，讓全局異常處理器處理
                throw;
            }
            finally
            {
                // 清理上下文
                _logContextAccessor.RemoveProperty("RequestId");
                context.Response.Body = originalBodyStream;
            }
        }
    }
}