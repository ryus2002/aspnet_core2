using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using System.Text.Json;
using HealthChecks.UI.Client;

namespace Shared.HealthChecks
{
    /// <summary>
    /// 健康檢查擴展類，提供統一的健康檢查配置方法
    /// </summary>
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// 添加基本健康檢查
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="serviceName">服務名稱</param>
        /// <returns>IHealthChecksBuilder實例，用於進一步配置</returns>
        public static IHealthChecksBuilder AddBasicHealthChecks(this IServiceCollection services, string serviceName)
        {
            return services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy($"{serviceName} 服務運行正常"), 
                    new[] { "service" });
        }

        /// <summary>
        /// 添加SQL Server健康檢查
        /// </summary>
        /// <param name="builder">健康檢查構建器</param>
        /// <param name="connectionString">SQL Server連接字符串</param>
        /// <returns>更新後的健康檢查構建器</returns>
        public static IHealthChecksBuilder AddSqlServer(this IHealthChecksBuilder builder, string connectionString)
        {
            return builder.AddSqlServer(
                connectionString: connectionString,
                name: "sql-server-database",
                tags: new[] { "database", "sql", "sqlserver" });
        }

        /// <summary>
        /// 添加MongoDB健康檢查
        /// </summary>
        /// <param name="builder">健康檢查構建器</param>
        /// <param name="connectionString">MongoDB連接字符串</param>
        /// <returns>更新後的健康檢查構建器</returns>
        public static IHealthChecksBuilder AddMongoDB(this IHealthChecksBuilder builder, string connectionString)
        {
            return builder.AddMongoDb(
                connectionString: connectionString,
                name: "mongodb-database",
                tags: new[] { "database", "mongodb" });
        }

        /// <summary>
        /// 添加RabbitMQ健康檢查
        /// </summary>
        /// <param name="builder">健康檢查構建器</param>
        /// <param name="connectionString">RabbitMQ連接字符串</param>
        /// <returns>更新後的健康檢查構建器</returns>
        public static IHealthChecksBuilder AddRabbitMQ(this IHealthChecksBuilder builder, string connectionString)
        {
            return builder.AddRabbitMQ(
                rabbitConnectionString: connectionString,
                name: "rabbitmq",
                tags: new[] { "messaging", "rabbitmq" });
        }

        /// <summary>
        /// 添加Redis健康檢查
        /// </summary>
        /// <param name="builder">健康檢查構建器</param>
        /// <param name="connectionString">Redis連接字符串</param>
        /// <returns>更新後的健康檢查構建器</returns>
        public static IHealthChecksBuilder AddRedis(this IHealthChecksBuilder builder, string connectionString)
        {
            return builder.AddRedis(
                redisConnectionString: connectionString,
                name: "redis",
                tags: new[] { "cache", "redis" });
        }

        /// <summary>
        /// 添加外部服務健康檢查
        /// </summary>
        /// <param name="builder">健康檢查構建器</param>
        /// <param name="serviceName">服務名稱</param>
        /// <param name="serviceUri">服務URI</param>
        /// <returns>更新後的健康檢查構建器</returns>
        public static IHealthChecksBuilder AddExternalService(
            this IHealthChecksBuilder builder, 
            string serviceName, 
            Uri serviceUri)
        {
            return builder.AddUrlGroup(
                uri: serviceUri,
                name: serviceName,
                tags: new[] { "service", "external", serviceName.ToLowerInvariant() });
        }

        /// <summary>
        /// 使用健康檢查端點
        /// </summary>
        /// <param name="app">應用程序構建器</param>
        /// <returns>應用程序構建器</returns>
        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
        {
            // 添加基本健康檢查端點
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // 添加詳細健康檢查端點
            app.UseHealthChecks("/health/detail", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = WriteDetailedHealthCheckResponse
            });

            // 添加特定類型的健康檢查端點
            app.UseHealthChecks("/health/services", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("service"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecks("/health/databases", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("database"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecks("/health/live", new HealthCheckOptions
            {
                // 僅檢查服務是否在運行，不檢查依賴項
                Predicate = check => check.Tags.Contains("service"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            return app;
        }

        /// <summary>
        /// 寫入詳細的健康檢查響應
        /// </summary>
        private static Task WriteDetailedHealthCheckResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonWriterOptions { Indented = true };
            using var memoryStream = new MemoryStream();
            using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("status", report.Status.ToString());
                jsonWriter.WriteString("timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                
                jsonWriter.WriteStartObject("results");

                foreach (var entry in report.Entries)
                {
                    jsonWriter.WriteStartObject(entry.Key);
                    
                    jsonWriter.WriteString("status", entry.Value.Status.ToString());
                    jsonWriter.WriteString("description", entry.Value.Description ?? "");
                    jsonWriter.WriteString("duration", entry.Value.Duration.ToString());

                    if (entry.Value.Exception != null)
                    {
                        jsonWriter.WriteStartObject("error");
                        jsonWriter.WriteString("message", entry.Value.Exception.Message);
                        jsonWriter.WriteString("stackTrace", entry.Value.Exception.StackTrace ?? "");
                        jsonWriter.WriteEndObject();
                    }

                    if (entry.Value.Data.Count > 0)
                    {
                        jsonWriter.WriteStartObject("data");
                        foreach (var item in entry.Value.Data)
                        {
                            jsonWriter.WritePropertyName(item.Key);
                            JsonSerializer.Serialize(jsonWriter, item.Value);
                        }
                        jsonWriter.WriteEndObject();
                    }

                    jsonWriter.WriteEndObject();
                }

                jsonWriter.WriteEndObject(); // results

                jsonWriter.WriteEndObject(); // root
            }

            return context.Response.WriteAsync(
                Encoding.UTF8.GetString(memoryStream.ToArray()));
        }
    }
}