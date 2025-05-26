using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Reflection;

namespace Shared.Logging
{
    /// <summary>
    /// 日誌系統擴展方法
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// 為服務添加統一的日誌系統
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="configuration">配置</param>
        /// <param name="serviceName">服務名稱</param>
        /// <returns>服務集合</returns>
        public static IServiceCollection AddUnifiedLogging(
            this IServiceCollection services, 
            IConfiguration configuration, 
            string? serviceName = null)
        {
            // 如果未提供服務名稱，則從程序集名稱獲取
            if (string.IsNullOrEmpty(serviceName))
            {
                serviceName = Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownService";
            }

            // 從配置中獲取日誌設置
            var logConfig = configuration.GetSection("Logging");
            var logFilePath = logConfig["FilePath"] ?? $"logs/{serviceName}/log-.json";
            var seqServerUrl = logConfig["SeqServerUrl"];

            // 創建 Serilog 日誌配置
            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("ServiceName", serviceName)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    new JsonFormatter(), 
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30);

            // 如果配置了 Seq 服務器，則添加 Seq 接收器
            if (!string.IsNullOrEmpty(seqServerUrl))
            {
                loggerConfig.WriteTo.Seq(seqServerUrl);
            }

            // 設置最小日誌級別
            var minimumLevel = logConfig["MinimumLevel"] ?? "Information";
            loggerConfig.MinimumLevel.Is(GetLogEventLevel(minimumLevel));

            // 設置特定命名空間的日誌級別
            loggerConfig.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
            loggerConfig.MinimumLevel.Override("System", LogEventLevel.Warning);
            loggerConfig.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);

            // 創建 Serilog 日誌實例
            var logger = loggerConfig.CreateLogger();

            // 將 Serilog 設置為默認日誌提供程序
            Log.Logger = logger;

            // 添加 Serilog 到 .NET Core 日誌框架
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(logger, dispose: true);
            });

            // 添加日誌上下文訪問器
            services.AddSingleton<ILogContextAccessor, LogContextAccessor>();

            return services;
        }

        /// <summary>
        /// 將字符串轉換為 Serilog 日誌級別
        /// </summary>
        /// <param name="level">日誌級別字符串</param>
        /// <returns>Serilog 日誌級別</returns>
        private static LogEventLevel GetLogEventLevel(string level)
        {
            return level?.ToLower() switch
            {
                "verbose" => LogEventLevel.Verbose,
                "debug" => LogEventLevel.Debug,
                "information" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }
    }
}