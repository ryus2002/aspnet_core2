using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Shared.Monitoring;
using Shared.Messaging;
using Shared.Logging;
using System;
var builder = WebApplication.CreateBuilder(args);

// 添加服務到容器
builder.Services.AddControllers();

// 添加 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// 添加 MongoDB 設定
var mongoDbSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoDbSettings.ConnectionString));
builder.Services.AddSingleton<IMongoDatabase>(sp => 
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDbSettings.DatabaseName));

// 添加服務註冊
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// 註冊庫存預警服務
builder.Services.AddScoped<IInventoryAlertService, InventoryAlertService>();

// 註冊庫存監控背景服務
builder.Services.AddHostedService<InventoryMonitoringService>();

// 添加訊息總線
builder.Services.AddMessageBus(builder.Configuration);

// 添加統一日誌
builder.Services.AddUnifiedLogging(builder.Configuration);

// 添加指標監控
builder.Services.AddMetricsMonitoring(builder.Configuration, "product-service");

// 添加監控儀表板
builder.Services.AddMonitoringDashboard();

var app = builder.Build();

// 配置 HTTP 請求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 使用全局異常處理
app.UseGlobalExceptionHandling();

// 使用請求日誌
app.UseRequestLogging();

// 使用指標監控
app.UseMetricsMonitoring();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 啟動消息處理器
app.Services.StartMessageHandlers();

app.Run();

// 為了讓 BusinessMetricsCollector 能夠使用
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
