using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace Shared.Monitoring
{
    /// <summary>
    /// 監控儀表板中間件
    /// </summary>
    public static class MonitoringDashboardMiddleware
    {
        /// <summary>
        /// 使用監控儀表板
        /// </summary>
        /// <param name="app">應用程序構建器</param>
        /// <returns>應用程序構建器</returns>
        public static IApplicationBuilder UseMonitoringDashboard(this IApplicationBuilder app)
        {
            // 設置靜態文件提供者，使用嵌入資源
            var assembly = Assembly.GetExecutingAssembly();
            var embeddedFileProvider = new EmbeddedFileProvider(assembly, "Shared.Monitoring.wwwroot");

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = embeddedFileProvider,
                RequestPath = "/monitoring/static"
            });

            // 映射監控儀表板路由
            app.Map("/monitoring", builder =>
            {
                builder.UseRouting();
                builder.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });

            return app;
        }

        /// <summary>
        /// 添加監控儀表板服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <returns>服務集合</returns>
        public static IServiceCollection AddMonitoringDashboard(this IServiceCollection services)
        {
            services.AddControllers()
                .AddApplicationPart(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}