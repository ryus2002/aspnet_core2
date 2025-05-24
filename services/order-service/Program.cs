using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Services;
using OrderService.Messaging.Publishers;
using Shared.Messaging.Extensions;
using System.Reflection;
using Shared.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// 添加服務到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加數據庫上下文
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 註冊服務
builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();
builder.Services.AddScoped<ICartService, CartService>();

// 註冊消息發布器
builder.Services.AddScoped<OrderEventPublisher>();

// 添加消息總線和相關服務
builder.Services.AddMessageBus(
    builder.Configuration,
    Assembly.GetExecutingAssembly()); // 使用當前程序集中的消息處理器

// 添加健康檢查
builder.Services.AddBasicHealthChecks("OrderService")
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "")
    .AddRabbitMQ($"amqp://{builder.Configuration["RabbitMQ:Username"] ?? "guest"}:{builder.Configuration["RabbitMQ:Password"] ?? "guest"}@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}:{builder.Configuration["RabbitMQ:Port"] ?? "5672"}")
    .AddExternalService("ProductService", new Uri(builder.Configuration["ServiceUrls:ProductService"] ?? "http://product-service/health"))
    .AddExternalService("PaymentService", new Uri(builder.Configuration["ServiceUrls:PaymentService"] ?? "http://payment-service/health"))
    .AddCheck("OrderProcessing", () => 
    {
        try
        {
            // 檢查訂單處理服務是否正常運作
            var serviceProvider = builder.Services.BuildServiceProvider();
            var orderService = serviceProvider.GetRequiredService<IOrderService>();
            
            // 這裡可以添加更多具體的檢查邏輯
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("訂單處理服務運作正常");
        }
        catch (Exception ex)
        {
            return new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult(
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                "訂單處理服務檢查失敗",
                ex);
        }
    }, new[] { "service", "order-processing" });

var app = builder.Build();

// 配置HTTP請求管道
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

// 啟動消息處理器
app.Services.StartMessageHandlers(Assembly.GetExecutingAssembly());

app.Run();