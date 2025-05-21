using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Services;
using OrderService.Messaging.Publishers;
using Shared.Messaging.Extensions;
using System.Reflection;

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

var app = builder.Build();

// 配置HTTP請求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// 啟動消息處理器
app.Services.StartMessageHandlers(Assembly.GetExecutingAssembly());

app.Run();