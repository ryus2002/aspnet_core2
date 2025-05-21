using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProductService.Data;
using ProductService.Services;
using ProductService.Settings;
using ProductService.Messaging.Publishers;
using Shared.Messaging.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 添加服務到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置 MongoDB 設定
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

// 註冊 MongoDB 資料庫上下文
builder.Services.AddSingleton<IProductDbContext>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = new MongoClient(settings.ConnectionString);
    var database = client.GetDatabase(settings.DatabaseName);
    return new ProductDbContext(database);
});

// 註冊服務
builder.Services.AddScoped<IProductService, ProductService.Services.ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// 註冊消息發布器
builder.Services.AddScoped<InventoryEventPublisher>();

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