using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Logging.Services;

namespace Shared.Logging
{
    /// <summary>
    /// 日誌查看工具擴展方法
    /// </summary>
    public static class LogViewerExtensions
    {
        /// <summary>
        /// 添加日誌查看工具服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合</returns>
        public static IServiceCollection AddLogViewer(this IServiceCollection services)
        {
            // 註冊日誌查詢服務
            services.AddSingleton<ILogQueryService, LogQueryService>();
            
            // 添加控制器
            services.AddControllers();
            
            return services;
        }

        /// <summary>
        /// 使用日誌查看工具
        /// </summary>
        /// <param name="app">應用程序構建器</param>
        /// <returns>應用程序構建器</returns>
        public static IApplicationBuilder UseLogViewer(this IApplicationBuilder app)
        {
            // 使用靜態文件
            app.UseStaticFiles();
            
            // 使用端點路由
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            return app;
        }
    }
}