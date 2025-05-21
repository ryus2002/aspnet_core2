namespace Shared.Messaging.Configuration
{
    /// <summary>
    /// RabbitMQ 連接設定
    /// </summary>
    public class RabbitMQSettings
    {
        /// <summary>
        /// 主機地址
        /// </summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        /// 連接埠
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// 虛擬主機
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// 用戶名
        /// </summary>
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// 密碼
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// 交換機名稱
        /// </summary>
        public string ExchangeName { get; set; } = "ecommerce_event_bus";

        /// <summary>
        /// 獲取連接字符串
        /// </summary>
        /// <returns>RabbitMQ 連接字符串</returns>
        public string GetConnectionString()
        {
            return $"amqp://{UserName}:{Password}@{HostName}:{Port}/{VirtualHost}";
        }
    }
}