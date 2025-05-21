using System.Threading.Tasks;

namespace Shared.Messaging
{
    /// <summary>
    /// 消息匯流排介面
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// 發布消息
        /// </summary>
        /// <param name="message">要發布的消息</param>
        /// <param name="exchange">交換機名稱</param>
        /// <param name="routingKey">路由鍵</param>
        /// <returns>發布任務</returns>
        Task PublishAsync(BaseMessage message, string exchange, string routingKey);
        
        /// <summary>
        /// 訂閱消息
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="exchange">交換機名稱</param>
        /// <param name="queue">隊列名稱</param>
        /// <param name="routingKey">路由鍵</param>
        /// <param name="handler">消息處理器</param>
        /// <returns>訂閱任務</returns>
        Task SubscribeAsync<T>(string exchange, string queue, string routingKey, Func<T, Task> handler) where T : BaseMessage;
    }
}