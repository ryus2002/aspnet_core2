using System;
using System.Collections.Concurrent;
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
        private readonly string _serviceName;
        private readonly ConcurrentDictionary<string, IModel> _consumerChannels = new ConcurrentDictionary<string, IModel>();

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="connectionString">RabbitMQ 連接字符串</param>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="serviceName">服務名稱，用於生成隊列名稱</param>
        public RabbitMQMessageBus(string connectionString, ILogger<RabbitMQMessageBus> logger, string serviceName = "service")
        {
            _logger = logger;
            _serviceName = serviceName;
            
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
        /// 發布消息 - 基本方法
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
        /// 發布消息 - 簡化方法
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="message">消息內容</param>
        /// <param name="exchangeName">交換機名稱，默認為 "ecommerce"</param>
        /// <param name="routingKey">路由鍵，默認為消息類型名稱</param>
        /// <returns>異步任務</returns>
        public Task PublishAsync<T>(T message, string exchangeName = "ecommerce", string? routingKey = null) where T : BaseMessage
        {
            // 如果沒有提供路由鍵，使用消息類型名稱
            routingKey ??= typeof(T).Name;
            
            return PublishAsync(message, exchangeName, routingKey);
        }

        /// <summary>
        /// 訂閱消息 - 基本方法
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
        /// 訂閱消息 - 簡化方法
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="handler">消息處理器</param>
        /// <param name="exchangeName">交換機名稱，默認為 "ecommerce"</param>
        /// <param name="queueName">隊列名稱，默認為消費者名稱 + 消息類型名稱</param>
        /// <param name="routingKey">路由鍵，默認為消息類型名稱</param>
        /// <param name="consumerName">消費者名稱，默認為當前服務名稱</param>
        /// <returns>異步任務</returns>
        public Task SubscribeAsync<T>(
            Func<T, Task> handler,
            string exchangeName = "ecommerce",
            string? queueName = null,
            string? routingKey = null,
            string? consumerName = null) where T : BaseMessage
        {
            // 設置默認值
            var messageType = typeof(T).Name;
            routingKey ??= messageType;
            consumerName ??= _serviceName;
            queueName ??= $"{consumerName}.{messageType}";
            
            return SubscribeAsync(exchangeName, queueName, routingKey, handler);
        }

        /// <summary>
        /// 同步訂閱指定主題的消息
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="handler">消息處理器</param>
        /// <param name="topic">主題名稱，默認使用消息類型名稱</param>
        public void Subscribe<T>(Func<T, Task> handler, string? topic = null) where T : BaseMessage
        {
            // 使用異步方法的實現，但同步調用
            var messageType = typeof(T).Name;
            topic ??= messageType;
            
            // 創建新的通道用於此訂閱
            var channel = _connection.CreateModel();
            var exchangeName = "ecommerce";
            var queueName = $"{_serviceName}.{messageType}";
            
            // 保存通道以便後續取消訂閱
            _consumerChannels[$"{typeof(T).FullName}:{topic}"] = channel;
            
            // 確保交換機存在
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

            // 確保隊列存在
            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // 綁定隊列到交換機
            channel.QueueBind(
                queue: queueName,
                exchange: exchangeName,
                routingKey: topic);

            // 創建消費者
            var consumer = new AsyncEventingBasicConsumer(channel);

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
                        channel.BasicAck(args.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize message");
                        channel.BasicNack(args.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    channel.BasicNack(args.DeliveryTag, false, true);
                }
            };

            // 開始消費
            channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Subscribed to topic {Topic} with queue {Queue}", topic, queueName);
        }

        /// <summary>
        /// 取消訂閱指定主題的消息
        /// </summary>
        /// <typeparam name="T">消息類型</typeparam>
        /// <param name="topic">主題名稱，默認使用消息類型名稱</param>
        public void Unsubscribe<T>(string? topic = null) where T : BaseMessage
        {
            var messageType = typeof(T).Name;
            topic ??= messageType;
            
            var key = $"{typeof(T).FullName}:{topic}";
            
            if (_consumerChannels.TryRemove(key, out var channel))
            {
                try
                {
                    channel.Close();
                    channel.Dispose();
                    _logger.LogInformation("Unsubscribed from topic {Topic}", topic);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error unsubscribing from topic {Topic}", topic);
                }
            }
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            // 關閉所有消費者通道
            foreach (var channel in _consumerChannels.Values)
            {
                try
                {
                    channel.Close();
                    channel.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing consumer channel");
                }
            }
            
            _consumerChannels.Clear();
            
            // 關閉主通道和連接
            _channel?.Close();
            _connection?.Close();
        }
    }
}