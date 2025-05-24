using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProductService.Data;
using ProductService.Services;
using ProductService.Settings;
using ProductService.Messaging.Publishers;
using Shared.Messaging;
using System.Reflection;
using ProductService.Messaging.Handlers;
using Shared.Logging;
using Shared.Logging.Middleware;
using Shared.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// 添加統一日誌系統
builder.Services.AddUnifiedLogging(builder.Configuration, "product-service");

// 添加日誌查看工具
builder.Services.AddLogViewer();

// 添加服務到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.EnableAnnotations(); // 啟用 Swagger 註解
});

// 配置 MongoDB 設定
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

// 註冊 MongoDB 資料庫上下文
builder.Services.AddSingleton<IProductDbContext, ProductDbContext>();

// 註冊服務
builder.Services.AddScoped<IProductService, ProductService.Services.ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// 註冊消息處理器
builder.Services.AddScoped<OrderCreatedHandler>();

// 註冊消息發布器
builder.Services.AddScoped<InventoryEventPublisher>();

// 註冊消息總線
builder.Services.AddSingleton<IMessageBus>(sp => {
    var logger = sp.GetRequiredService<ILogger<RabbitMQMessageBus>>();
    var configuration = builder.Configuration;
    var rabbitMQSettings = configuration.GetSection("RabbitMQ");
    var host = rabbitMQSettings["Host"] ?? "localhost";
    var port = int.Parse(rabbitMQSettings["Port"] ?? "5672");
    var username = rabbitMQSettings["Username"] ?? "guest";
    var password = rabbitMQSettings["Password"] ?? "guest";
    
    var connectionString = $"amqp://{username}:{password}@{host}:{port}";
    return new RabbitMQMessageBus(connectionString, logger);
});

// 添加健康檢查
builder.Services.AddBasicHealthChecks("ProductService")
    .AddMongoDB(builder.Configuration.GetSection("MongoDB:ConnectionString").Value ?? "mongodb://localhost:27017")
    .AddRabbitMQ($"amqp://{builder.Configuration["RabbitMQ:Username"] ?? "guest"}:{builder.Configuration["RabbitMQ:Password"] ?? "guest"}@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}:{builder.Configuration["RabbitMQ:Port"] ?? "5672"}")
    .AddCheck("InventoryService", () => 
    {
        try
        {
            // 檢查庫存服務是否正常運作
            var serviceProvider = builder.Services.BuildServiceProvider();
            var inventoryService = serviceProvider.GetRequiredService<IInventoryService>();
            
            // 這裡可以添加更多具體的檢查邏輯
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("庫存服務運作正常");
        }
        catch (Exception ex)
        {
            return new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult(
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                "庫存服務檢查失敗",
                ex);
        }
    }, new[] { "service", "inventory" });

var app = builder.Build();

// 配置HTTP請求管道

// 添加全局異常處理中間件
app.UseGlobalExceptionHandling(app.Environment.IsDevelopment());

// 添加請求日誌中間件
app.UseRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// 啟用健康檢查端點
app.UseHealthChecks();

app.MapControllers();

// 啟用日誌查看工具
app.UseLogViewer();

// 註冊消息處理器
var messageBus = app.Services.GetRequiredService<IMessageBus>();
var serviceProvider = app.Services;

// 手動註冊訂單創建處理器
using (var scope = serviceProvider.CreateScope())
{
    var handler = scope.ServiceProvider.GetRequiredService<OrderCreatedHandler>();
    // 添加 await 關鍵字以解決 CS4014 警告
    await messageBus.SubscribeAsync<OrderCreatedMessage>(
        "ecommerce",
        "product-service.orders.created",
        "order.created",
        message => handler.HandleAsync(message));
}

// 使用 await 運行應用
await app.RunAsync();