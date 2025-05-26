using System;
using System.Threading.Tasks;

namespace Shared.Messaging
{
    /// <summary>
    /// 消息匯流排介面 - 統一版本
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// 發布消息 - 基本方法
        /// </summary>
        /// <param name="message">要發布的消息</param>
        /// <param name="exchange">交換機名稱</param>
        /// <param name="routingKey">路由鍵</param>
        /// <returns>發布任務</returns>
        Task PublishAsync(BaseMessage message, string exchange, string routingKey);
        
        /// <summary>
        /// 發布消息 - 簡化方法
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="message">消息內容</param>
        /// <param name="exchangeName">交換機名稱，默認為 "ecommerce"</param>
        /// <param name="routingKey">路由鍵，默認為消息類型名稱</param>
        /// <returns>異步任務</returns>
        Task PublishAsync<T>(T message, string exchangeName = "ecommerce", string? routingKey = null) where T : BaseMessage;
        
        /// <summary>
        /// 訂閱消息 - 基本方法
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="exchange">交換機名稱</param>
        /// <param name="queue">隊列名稱</param>
        /// <param name="routingKey">路由鍵</param>
        /// <param name="handler">消息處理器</param>
        /// <returns>訂閱任務</returns>
        Task SubscribeAsync<T>(string exchange, string queue, string routingKey, Func<T, Task> handler) where T : BaseMessage;
        
        /// <summary>
        /// 訂閱消息 - 簡化方法
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
            
        /// <summary>
        /// 同步訂閱指定主題的消息
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="handler">消息處理器</param>
        /// <param name="topic">主題名稱，默認使用消息類型名稱</param>
        void Subscribe<T>(Func<T, Task> handler, string? topic = null) where T : BaseMessage;

        /// <summary>
        /// 取消訂閱指定主題的消息
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="topic">主題名稱，默認使用消息類型名稱</param>
        void Unsubscribe<T>(string? topic = null) where T : BaseMessage;
    }
}