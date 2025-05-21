using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Messaging.MessageBus;
using Shared.Messaging.Messages;

namespace Shared.Messaging.Extensions
{
    /// <summary>
    /// 服務集合擴展方法
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加消息總線服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="configuration">配置</param>
        /// <param name="assembly">包含消息處理器的程序集</param>
        /// <returns>服務集合</returns>
        public static IServiceCollection AddMessageBus(
            this IServiceCollection services, 
            IConfiguration configuration, 
            Assembly? assembly = null)
        {
            // 從配置中獲取 RabbitMQ 連接字符串
            var connectionString = configuration.GetConnectionString("RabbitMQ") 
                ?? "amqp://guest:guest@localhost:5672/";

            // 註冊消息總線
            services.AddSingleton<IMessageBus>(sp => 
            {
                var logger = sp.GetRequiredService<ILogger<RabbitMQMessageBus>>();
                return new RabbitMQMessageBus(connectionString, logger);
            });

            return services;
        }

        /// <summary>
        /// 啟動消息處理器
        /// </summary>
        /// <param name="serviceProvider">服務提供者</param>
        /// <param name="assembly">包含消息處理器的程序集</param>
        public static void StartMessageHandlers(this IServiceProvider serviceProvider, Assembly? assembly = null)
        {
            // 這個方法會在應用程序啟動時被調用
            // 實際的消息處理器註冊邏輯將在這裡實現
            // 但為了簡化，我們暫時不實現具體的消息處理器註冊邏輯
        }
    }
}