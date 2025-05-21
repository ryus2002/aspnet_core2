using Shared.Messaging.Messages;

namespace Shared.Messaging.MessageBus
{
    /// <summary>
    /// 消息總線接口，用於發布和訂閱消息
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// 發布消息到指定主題
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="message">消息內容</param>
        /// <param name="topic">主題名稱，默認使用消息類型名稱</param>
        /// <returns>異步任務</returns>
        Task PublishAsync<T>(T message, string? topic = null) where T : BaseMessage;

        /// <summary>
        /// 訂閱指定主題的消息
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