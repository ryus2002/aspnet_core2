using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Configuration;
using Shared.Messaging.Handlers;
using Shared.Messaging.MessageBus;
using System.Reflection;

namespace Shared.Messaging.Extensions
{
    /// <summary>
    /// 服務集合擴展方法
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加消息總線和相關服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="configuration">配置</param>
        /// <param name="assemblies">包含消息處理器的程序集</param>
        /// <returns>服務集合</returns>
        public static IServiceCollection AddMessageBus(
            this IServiceCollection services,
            IConfiguration configuration,
            params Assembly[] assemblies)
        {
            // 註冊 RabbitMQ 設定
            var rabbitMQSettings = new RabbitMQSettings();
            configuration.GetSection("RabbitMQ").Bind(rabbitMQSettings);
            services.AddSingleton(rabbitMQSettings);

            // 註冊消息總線
            services.AddSingleton<IMessageBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RabbitMQMessageBus>>();
                return new RabbitMQMessageBus(
                    rabbitMQSettings.GetConnectionString(),
                    rabbitMQSettings.ExchangeName,
                    logger);
            });

            // 註冊消息處理器註冊表
            services.AddSingleton<MessageHandlerRegistry>();

            // 註冊所有消息處理器
            foreach (var assembly in assemblies)
            {
                RegisterMessageHandlers(services, assembly);
            }

            return services;
        }

        /// <summary>
        /// 註冊指定程序集中的所有消息處理器
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="assembly">程序集</param>
        private static void RegisterMessageHandlers(IServiceCollection services, Assembly assembly)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
                .Where(x => x.Interface.IsGenericType &&
                           x.Interface.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                .ToList();

            foreach (var handler in handlerTypes)
            {
                services.AddTransient(handler.Type);
            }
        }

        /// <summary>
        /// 啟動消息處理器
        /// </summary>
        /// <param name="serviceProvider">服務提供者</param>
        /// <param name="assemblies">包含消息處理器的程序集</param>
        public static void StartMessageHandlers(this IServiceProvider serviceProvider, params Assembly[] assemblies)
        {
            var registry = serviceProvider.GetRequiredService<MessageHandlerRegistry>();
            registry.RegisterAllHandlers(assemblies);
        }
    }
}