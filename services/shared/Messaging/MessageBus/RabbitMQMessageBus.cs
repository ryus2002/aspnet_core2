using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Messaging.Messages;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging.MessageBus
{
    /// <summary>
    /// 基於 RabbitMQ 的消息總線實現
    /// </summary>
    public class RabbitMQMessageBus : IMessageBus, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQMessageBus> _logger;
        private readonly string _exchangeName;
        private readonly Dictionary<string, List<object>> _handlers = new Dictionary<string, List<object>>();
        private readonly Dictionary<string, AsyncEventingBasicConsumer> _consumers = new Dictionary<string, AsyncEventingBasicConsumer>();

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="connectionString">RabbitMQ 連接字符串</param>
        /// <param name="exchangeName">交換機名稱</param>
        /// <param name="logger">日誌記錄器</param>
        public RabbitMQMessageBus(string connectionString, string exchangeName, ILogger<RabbitMQMessageBus> logger)
        {
            _logger = logger;
            _exchangeName = exchangeName;

            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(connectionString),
                    DispatchConsumersAsync = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // 聲明交換機
                _channel.ExchangeDeclare(
                    exchange: _exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _logger.LogInformation("RabbitMQ 連接已建立，交換機: {Exchange}", _exchangeName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化 RabbitMQ 連接失敗");
                throw;
            }
        }

        /// <summary>
        /// 發布消息到指定主題
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="message">消息內容</param>
        /// <param name="topic">主題名稱，默認使用消息類型名稱</param>
        /// <returns>異步任務</returns>
        public Task PublishAsync<T>(T message, string? topic = null) where T : BaseMessage
        {
            try
            {
                var routingKey = topic ?? typeof(T).Name;
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel.CreateBasicProperties();
                properties.DeliveryMode = 2; // 持久化消息
                properties.MessageId = message.MessageId;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.Headers = new Dictionary<string, object>
                {
                    { "MessageType", typeof(T).AssemblyQualifiedName ?? typeof(T).Name }
                };

                if (!string.IsNullOrEmpty(message.CorrelationId))
                {
                    properties.CorrelationId = message.CorrelationId;
                }

                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("消息已發布: Type={MessageType}, Id={MessageId}, Topic={Topic}",
                    typeof(T).Name, message.MessageId, routingKey);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發布消息失敗: Type={MessageType}, Id={MessageId}",
                    typeof(T).Name, message.MessageId);
                throw;
            }
        }

        /// <summary>
        /// 訂閱指定主題的消息
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="handler">消息處理器</param>
        /// <param name="topic">主題名稱，默認使用消息類型名稱</param>
        public void Subscribe<T>(Func<T, Task> handler, string? topic = null) where T : BaseMessage
        {
            var routingKey = topic ?? typeof(T).Name;
            var queueName = $"{routingKey}-{Guid.NewGuid()}";

            try
            {
                // 聲明隊列
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: true,
                    arguments: null);

                // 綁定隊列到交換機
                _channel.QueueBind(
                    queue: queueName,
                    exchange: _exchangeName,
                    routingKey: routingKey);

                // 創建消費者
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        var message = JsonSerializer.Deserialize<T>(json);

                        if (message != null)
                        {
                            _logger.LogInformation("收到消息: Type={MessageType}, Id={MessageId}, Topic={Topic}",
                                typeof(T).Name, message.MessageId, routingKey);

                            await handler(message);
                            _channel.BasicAck(ea.DeliveryTag, false);
                        }
                        else
                        {
                            _logger.LogWarning("無法反序列化消息: Topic={Topic}, Body={Body}",
                                routingKey, json);
                            _channel.BasicNack(ea.DeliveryTag, false, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "處理消息失敗: Topic={Topic}", routingKey);
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };

                // 開始消費
                _channel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);

                // 保存處理器和消費者引用
                if (!_handlers.ContainsKey(routingKey))
                {
                    _handlers[routingKey] = new List<object>();
                }
                _handlers[routingKey].Add(handler);
                _consumers[queueName] = consumer;

                _logger.LogInformation("已訂閱主題: {Topic}, 隊列: {Queue}", routingKey, queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "訂閱主題失敗: {Topic}", routingKey);
                throw;
            }
        }

        /// <summary>
        /// 取消訂閱指定主題的消息
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="topic">主題名稱，默認使用消息類型名稱</param>
        public void Unsubscribe<T>(string? topic = null) where T : BaseMessage
        {
            var routingKey = topic ?? typeof(T).Name;

            if (_handlers.ContainsKey(routingKey))
            {
                _handlers.Remove(routingKey);
                
                // 找到並取消相關的消費者
                var queuesToRemove = new List<string>();
                foreach (var pair in _consumers)
                {
                    if (pair.Key.StartsWith(routingKey))
                    {
                        try
                        {
                            _channel.BasicCancel(pair.Value.ConsumerTags.FirstOrDefault() ?? "");
                            queuesToRemove.Add(pair.Key);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "取消消費者失敗: {ConsumerTag}", 
                                pair.Value.ConsumerTags.FirstOrDefault());
                        }
                    }
                }

                foreach (var queue in queuesToRemove)
                {
                    _consumers.Remove(queue);
                }

                _logger.LogInformation("已取消訂閱主題: {Topic}", routingKey);
            }
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _logger.LogInformation("RabbitMQ 連接已關閉");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "關閉 RabbitMQ 連接時發生錯誤");
            }
        }
    }
}