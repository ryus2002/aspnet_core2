using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace ProductService.IntegrationTests.Infrastructure
{
    /// <summary>
    /// 模拟消息总线实现，用于测试
    /// </summary>
    public class MockMessageBus : IMessageBus
    {
        private readonly ILogger<MockMessageBus> _logger;

        public MockMessageBus(ILogger<MockMessageBus> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 发布消息（模拟实现）
        /// </summary>
        public Task PublishAsync(BaseMessage message, string exchange, string routingKey)
        {
            _logger.LogInformation("模拟发布消息: Exchange={Exchange}, RoutingKey={RoutingKey}, MessageType={MessageType}, MessageId={MessageId}",
                exchange, routingKey, message.MessageType, message.Id);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 订阅消息（模拟实现）
        /// </summary>
        public Task SubscribeAsync<T>(string exchange, string queue, string routingKey, Func<T, Task> handler) where T : BaseMessage
        {
            _logger.LogInformation("模拟订阅消息: Exchange={Exchange}, Queue={Queue}, RoutingKey={RoutingKey}, MessageType={MessageType}",
                exchange, queue, routingKey, typeof(T).Name);
            return Task.CompletedTask;
        }
    }
}