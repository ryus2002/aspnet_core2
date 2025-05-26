using Microsoft.AspNetCore.Builder;

namespace Shared.Logging.Middleware
{
    /// <summary>
    /// 全局異常處理中間件擴展方法
    /// </summary>
    public static class GlobalExceptionHandlingExtensions
    {
        /// <summary>
        /// 添加全局異常處理中間件
        /// </summary>
        /// <param name="builder">應用程序構建器</param>
        /// <param name="includeExceptionDetails">是否在響應中包含異常詳情</param>
        /// <returns>應用程序構建器</returns>
        public static IApplicationBuilder UseGlobalExceptionHandling(
            this IApplicationBuilder builder,
            bool includeExceptionDetails = false)
        {
            return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>(includeExceptionDetails);
        }
    }
}