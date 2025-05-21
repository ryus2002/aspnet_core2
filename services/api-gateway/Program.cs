using ApiGateway.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

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

// 配置JWT认证
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

var app = builder.Build();

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway V1");
        c.OAuthClientId("swagger");
        c.OAuthAppName("Swagger UI");
    });
    
    // 在开发环境中输出详细日志
    app.Logger.LogInformation("API Gateway running in Development environment");
}

// 使用CORS
app.UseCors();

// 添加请求日志中间件
app.Use(async (context, next) =>
{
    // 记录请求开始
    var requestPath = context.Request.Path;
    var requestMethod = context.Request.Method;
    var requestTime = DateTime.UtcNow;
    
    app.Logger.LogInformation($"Request started: {requestMethod} {requestPath} at {requestTime}");
    
    try
    {
        await next();
    }
    finally
    {
        // 记录请求结束
        var responseTime = DateTime.UtcNow;
        var duration = responseTime - requestTime;
        var statusCode = context.Response.StatusCode;
        
        app.Logger.LogInformation($"Request completed: {requestMethod} {requestPath} with status {statusCode} in {duration.TotalMilliseconds}ms");
    }
});

// 使用认证中间件
app.UseAuthentication();
app.UseAuthorization();

// 使用自定义认证中间件（可选，如果需要更精细的控制）
// app.UseCustomAuthentication();

// 使用自定义请求转发中间件（可选，用于特殊场景）
// app.UseRequestForwarding();

// 映射控制器路由
app.MapControllers();

// 使用Ocelot中间件
await app.UseOcelot();

app.Logger.LogInformation("API Gateway started successfully");

app.Run();