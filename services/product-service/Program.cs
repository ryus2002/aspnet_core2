using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using ProductService.Data;
using ProductService.Services;
using Serilog;
using System.Text;
using MongoDB.Bson;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// 添加服務到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 配置 Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "商品服務 API", 
        Version = "v1",
        Description = "提供商品管理、類別管理和庫存管理功能",
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
    
    // 自定義 Swagger 文檔
    c.EnableAnnotations(); // 啟用 Swashbuckle.AspNetCore.Annotations
    
    // 添加分組標籤
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
});

// 配置 MongoDB
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb");
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton<IProductDbContext, ProductDbContext>();

// 註冊服務
builder.Services.AddScoped<IProductService, ProductService.Services.ProductService>();
builder.Services.AddScoped<ICategoryService, ProductService.Services.CategoryService>();
builder.Services.AddScoped<IInventoryService, ProductService.Services.InventoryService>();

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
builder.Services.AddHealthChecks()
    .AddMongoDb(mongoConnectionString ?? throw new InvalidOperationException("MongoDB connection string not configured"), 
        name: "mongodb", 
        timeout: TimeSpan.FromSeconds(3));

var app = builder.Build();

// 配置 HTTP 請求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c => {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "商品服務 API v1");
        c.RoutePrefix = string.Empty; // 設置 Swagger UI 在根路徑
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // 預設折疊所有操作
        c.DefaultModelsExpandDepth(-1); // 隱藏 Models 部分
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// 添加健康檢查端點
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();