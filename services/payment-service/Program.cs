using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PaymentService.Data;
using PaymentService.Services;
using System;
using System.IO;
using System.Reflection;
using Shared.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// 添加服務到容器
builder.Services.AddControllers();

// 配置 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "支付服務 API",
        Version = "v1",
        Description = "微服務電商平台的支付服務 API",
        Contact = new OpenApiContact
        {
            Name = "開發團隊",
            Email = "dev@example.com"
        }
    });

    // 啟用 Swagger 註釋
    c.EnableAnnotations();
    
    // 包含 XML 註釋
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// 配置資料庫
builder.Services.AddDbContext<PaymentDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 註冊服務
builder.Services.AddScoped<IPaymentService, PaymentService.Services.PaymentService>();
builder.Services.AddScoped<IRefundService, RefundService>();
builder.Services.AddScoped<IMockPaymentService, MockPaymentService>();

// 添加跨域支持
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// 添加健康檢查
builder.Services.AddBasicHealthChecks("PaymentService")
    .AddNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "")
    .AddExternalService("OrderService", new Uri(builder.Configuration["ServiceUrls:OrderService"] ?? "http://order-service/health"))
    .AddCheck("PaymentProcessors", () => 
    {
        try
        {
            // 檢查支付處理器是否正常運作
            var serviceProvider = builder.Services.BuildServiceProvider();
            var mockPaymentService = serviceProvider.GetRequiredService<IMockPaymentService>();
            
            // 這裡可以添加更多具體的檢查邏輯
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("支付處理服務運作正常");
        }
        catch (Exception ex)
        {
            return new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult(
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                "支付處理服務檢查失敗",
                ex);
        }
    }, new[] { "service", "payment-processing" })
    .AddCheck("RefundService", () => 
    {
        try
        {
            // 檢查退款服務是否正常運作
            var serviceProvider = builder.Services.BuildServiceProvider();
            var refundService = serviceProvider.GetRequiredService<IRefundService>();
            
            // 這裡可以添加更多具體的檢查邏輯
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("退款服務運作正常");
        }
        catch (Exception ex)
        {
            return new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult(
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                "退款服務檢查失敗",
                ex);
        }
    }, new[] { "service", "refund-processing" });

var app = builder.Build();

// 配置 HTTP 請求管道
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "支付服務 API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// 啟用健康檢查端點
app.UseHealthChecks();

app.MapControllers();

// 在開發環境中自動執行遷移
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        dbContext.Database.Migrate();
    }
}

app.Run();