using System;
using System.Threading.Tasks;
using Shared.Messaging.Messages;

namespace Shared.Messaging.MessageBus
{
    /// <summary>
    /// 消息總線接口
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// 發布消息
        /// </summary>
        /// <param name="message">消息對象</param>
        /// <param name="exchangeName">交換機名稱，默認為 "ecommerce"</param>
        /// <param name="routingKey">路由鍵，默認為消息類型名稱</param>
        /// <returns>異步任務</returns>
        Task PublishAsync(BaseMessage message, string exchangeName = "ecommerce", string? routingKey = null);
        
        /// <summary>
        /// 訂閱消息
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="handler">消息處理器</param>
        /// <param name="exchangeName">交換機名稱，默認為 "ecommerce"</param>
        /// <param name="queueName">隊列名稱，默認為消費者名稱 + 消息類型名稱</param>
        /// <param name="routingKey">路由鍵，默認為消息類型名稱</param>
        /// <param name="consumerName">消費者名稱，默認為當前服務名稱</param>
        /// <returns>異步任務</returns>
        Task SubscribeAsync<T>(
            Func<T, Task> handler,
            string exchangeName = "ecommerce",
            string? queueName = null,
            string? routingKey = null,
            string? consumerName = null) where T : BaseMessage;
    }
}