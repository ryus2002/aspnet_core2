using Microsoft.AspNetCore.Builder;

namespace Shared.Logging.Middleware
{
    /// <summary>
    /// 中間件擴展方法
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// 添加請求日誌中間件
        /// </summary>
        /// <param name="builder">應用程序構建器</param>
        /// <returns>應用程序構建器</returns>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}