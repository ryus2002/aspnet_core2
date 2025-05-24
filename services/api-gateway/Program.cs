using ApiGateway.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;
using Shared.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// 添加配置文件
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// 添加控制器
builder.Services.AddControllers();

// 添加Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API Gateway", Version = "v1" });
});

// 添加HTTP客户端工厂
builder.Services.AddHttpClient();

// 配置JWT认證
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key is not configured");
var issuer = jwtSettings["Issuer"] ?? "ecommerce-api";
var audience = jwtSettings["Audience"] ?? "ecommerce-clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// 添加Ocelot
builder.Services.AddOcelot(builder.Configuration);

// 添加CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 添加健康檢查
builder.Services.AddBasicHealthChecks("ApiGateway")
    .AddExternalService("AuthService", new Uri(builder.Configuration["ServiceUrls:AuthService"] ?? "http://auth-service/health"))
    .AddExternalService("ProductService", new Uri(builder.Configuration["ServiceUrls:ProductService"] ?? "http://product-service/health"))
    .AddExternalService("OrderService", new Uri(builder.Configuration["ServiceUrls:OrderService"] ?? "http://order-service/health"))
    .AddExternalService("PaymentService", new Uri(builder.Configuration["ServiceUrls:PaymentService"] ?? "http://payment-service/health"));

var app = builder.Build();

// 配置HTTP請求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway V1");
        c.OAuthClientId("swagger");
        c.OAuthAppName("Swagger UI");
    });
    
    // 在開發環境中輸出詳細日誌
    app.Logger.LogInformation("API Gateway running in Development environment");
}

// 使用CORS
app.UseCors();

// 添加請求日誌中間件
app.Use(async (context, next) =>
{
    // 記錄請求開始
    var requestPath = context.Request.Path;
    var requestMethod = context.Request.Method;
    var requestTime = DateTime.UtcNow;
    
    app.Logger.LogInformation($"請求開始: {requestMethod} {requestPath} 於 {requestTime}");
    
    try
    {
        await next();
    }
    finally
    {
        // 記錄請求結束
        var responseTime = DateTime.UtcNow;
        var duration = responseTime - requestTime;
        var statusCode = context.Response.StatusCode;
        
        app.Logger.LogInformation($"請求完成: {requestMethod} {requestPath} 狀態碼 {statusCode} 耗時 {duration.TotalMilliseconds}ms");
    }
});

// 使用認證中間件
app.UseAuthentication();
app.UseAuthorization();

// 使用自定義認證中間件（可選，如果需要更精細的控制）
// app.UseCustomAuthentication();

// 使用自定義請求轉發中間件（可選，用於特殊場景）
// app.UseRequestForwarding();

// 添加靜態文件支持，用於健康狀態頁面
app.UseStaticFiles();

// 啟用健康檢查端點
app.UseHealthChecks();

// 映射控制器路由
app.MapControllers();

// 使用Ocelot中間件
await app.UseOcelot();

app.Logger.LogInformation("API Gateway 成功啟動");

app.Run();