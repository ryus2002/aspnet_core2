using Shared.Messaging.Messages;

namespace Shared.Messaging.Handlers
{
    /// <summary>
    /// 消息處理器接口
    /// </summary>
    /// <typeparam name="T">消息類型</typeparam>
    public interface IMessageHandler<T> where T : BaseMessage
    {
        /// <summary>
        /// 處理消息
        /// </summary>
        /// <param name="message">消息內容</param>
        /// <returns>異步任務</returns>
        Task HandleAsync(T message);
    }
}