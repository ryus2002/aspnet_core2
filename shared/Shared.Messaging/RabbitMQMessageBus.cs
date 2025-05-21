using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared.Messaging
{
    /// <summary>
    /// RabbitMQ 消息匯流排實現
    /// </summary>
    public class RabbitMQMessageBus : IMessageBus, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQMessageBus> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="connectionString">RabbitMQ 連接字符串</param>
        /// <param name="logger">日誌記錄器</param>
        public RabbitMQMessageBus(string connectionString, ILogger<RabbitMQMessageBus> logger)
        {
            _logger = logger;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(connectionString),
                    DispatchConsumersAsync = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                
                _logger.LogInformation("Connected to RabbitMQ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ");
                throw;
            }
        }

        /// <summary>
        /// 發布消息
        /// </summary>
        /// <param name="message">要發布的消息</param>
        /// <param name="exchange">交換機名稱</param>
        /// <param name="routingKey">路由鍵</param>
        public Task PublishAsync(BaseMessage message, string exchange, string routingKey)
        {
            try
            {
                _logger.LogInformation("Publishing message {MessageType} to {Exchange} with routing key {RoutingKey}",
                    message.MessageType, exchange, routingKey);

                // 確保交換機存在
                _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

                // 序列化消息
                var jsonMessage = JsonSerializer.Serialize(message, message.GetType(), _jsonOptions);
                var body = Encoding.UTF8.GetBytes(jsonMessage);

                // 發布消息
                var properties = _channel.CreateBasicProperties();
                properties.MessageId = message.Id;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.ContentType = "application/json";
                properties.DeliveryMode = 2; // 持久化

                _channel.BasicPublish(
                    exchange: exchange,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Message {MessageId} published successfully", message.Id);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message {MessageType}", message.MessageType);
                throw;
            }
        }

        /// <summary>
        /// 訂閱消息
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="exchange">交換機名稱</param>
        /// <param name="queue">隊列名稱</param>
        /// <param name="routingKey">路由鍵</param>
        /// <param name="handler">消息處理器</param>
        public Task SubscribeAsync<T>(string exchange, string queue, string routingKey, Func<T, Task> handler) where T : BaseMessage
        {
            try
            {
                _logger.LogInformation("Subscribing to {Queue} for messages with routing key {RoutingKey}", queue, routingKey);

                // 確保交換機存在
                _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

                // 確保隊列存在
                _channel.QueueDeclare(
                    queue: queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                // 綁定隊列到交換機
                _channel.QueueBind(
                    queue: queue,
                    exchange: exchange,
                    routingKey: routingKey);

                // 創建消費者
                var consumer = new AsyncEventingBasicConsumer(_channel);

                // 處理接收到的消息
                consumer.Received += async (sender, args) =>
                {
                    try
                    {
                        var body = args.Body.ToArray();
                        var jsonMessage = Encoding.UTF8.GetString(body);

                        _logger.LogInformation("Received message: {Message}", jsonMessage);

                        // 反序列化消息
                        var message = JsonSerializer.Deserialize<T>(jsonMessage, _jsonOptions);

                        if (message != null)
                        {
                            await handler(message);
                            _channel.BasicAck(args.DeliveryTag, false);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to deserialize message");
                            _channel.BasicNack(args.DeliveryTag, false, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message");
                        _channel.BasicNack(args.DeliveryTag, false, true);
                    }
                };

                // 開始消費
                _channel.BasicConsume(
                    queue: queue,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("Subscribed to {Queue}", queue);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to {Queue}", queue);
                throw;
            }
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}