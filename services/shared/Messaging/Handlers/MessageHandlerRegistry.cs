using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Messaging.MessageBus;
using Shared.Messaging.Messages;
using System.Reflection;

namespace Shared.Messaging.Handlers
{
    /// <summary>
    /// 消息處理器註冊表，用於註冊和管理消息處理器
    /// </summary>
    public class MessageHandlerRegistry
    {
        private readonly IMessageBus _messageBus;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageHandlerRegistry> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="messageBus">消息總線</param>
        /// <param name="serviceProvider">服務提供者</param>
        /// <param name="logger">日誌記錄器</param>
        public MessageHandlerRegistry(
            IMessageBus messageBus,
            IServiceProvider serviceProvider,
            ILogger<MessageHandlerRegistry> logger)
        {
            _messageBus = messageBus;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// 註冊所有消息處理器
        /// </summary>
        /// <param name="assemblies">包含處理器的程序集</param>
        public void RegisterAllHandlers(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                assemblies = new[] { Assembly.GetEntryAssembly()! };
            }

            foreach (var assembly in assemblies)
            {
                RegisterHandlersFromAssembly(assembly);
            }
        }

        /// <summary>
        /// 從程序集中註冊所有消息處理器
        /// </summary>
        /// <param name="assembly">程序集</param>
        private void RegisterHandlersFromAssembly(Assembly assembly)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
                .Where(x => x.Interface.IsGenericType && 
                            x.Interface.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                .ToList();

            foreach (var handler in handlerTypes)
            {
                var messageType = handler.Interface.GetGenericArguments()[0];
                var registerMethod = typeof(MessageHandlerRegistry)
                    .GetMethod(nameof(RegisterHandler), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(messageType);

                registerMethod.Invoke(this, new object[] { handler.Type });
                
                _logger.LogInformation("已註冊消息處理器: {HandlerType} 用於消息類型: {MessageType}", 
                    handler.Type.Name, messageType.Name);
            }
        }

        /// <summary>
        /// 註冊特定類型的消息處理器
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="handlerType">處理器類型</param>
        private void RegisterHandler<T>(Type handlerType) where T : BaseMessage
        {
            _messageBus.Subscribe<T>(async message =>
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetService(handlerType);

                if (handler == null)
                {
                    _logger.LogError("無法解析消息處理器: {HandlerType}", handlerType.Name);
                    return;
                }

                try
                {
                    _logger.LogInformation("處理消息: Type={MessageType}, Id={MessageId}", 
                        typeof(T).Name, message.MessageId);
                    
                    await ((IMessageHandler<T>)handler).HandleAsync(message);
                    
                    _logger.LogInformation("消息處理完成: Type={MessageType}, Id={MessageId}", 
                        typeof(T).Name, message.MessageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "處理消息時發生錯誤: Type={MessageType}, Id={MessageId}", 
                        typeof(T).Name, message.MessageId);
                    throw;
                }
            });
        }
    }
}