using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Reflection;
using AuthService.Middleware;
using AuthService.Services;
using Shared.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// 添加服務到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 配置 Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "認證服務 API", 
        Version = "v1",
        Description = "提供用戶認證、授權和權限管理功能",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        }
    });
    
    // 添加JWT認證支持
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // 啟用 XML 註解
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// 註冊服務
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

// 配置 JWT 認證
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"))
        )
    };
});

// 添加健康檢查
builder.Services.AddBasicHealthChecks("AuthService")
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "")
    .AddCheck("TokenService", () => 
    {
        try
        {
            // 檢查JWT服務配置是否正確
            var jwtSecret = builder.Configuration["Jwt:Secret"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];
            
            if (string.IsNullOrEmpty(jwtSecret) || 
                string.IsNullOrEmpty(jwtIssuer) || 
                string.IsNullOrEmpty(jwtAudience))
            {
                return new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult(
                    Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
                    "JWT配置不完整");
            }
            
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("JWT服務配置正確");
        }
        catch (Exception ex)
        {
            return new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult(
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                "JWT服務檢查失敗",
                ex);
        }
    }, new[] { "service", "security" });

var app = builder.Build();

// 配置 HTTP 請求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "認證服務 API v1");
        c.RoutePrefix = string.Empty; // 設置 Swagger UI 在根路徑
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// 在管道中添加授權中間件
app.UseMiddleware<AuthorizationMiddleware>();

// 啟用健康檢查端點
app.UseHealthChecks();

app.MapControllers();

app.Run();