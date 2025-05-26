using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductService.Data;
using ProductService.Services;
using Xunit;
using Xunit.Abstractions;
using ProductService.Messaging.Publishers;
using Shared.Messaging;

namespace ProductService.IntegrationTests.Infrastructure
{
    public class IntegrationTestBase : IAsyncLifetime
    {
        protected MongoDbFixture MongoFixture { get; }
        protected IServiceProvider? ServiceProvider { get; private set; }
        protected IProductService? ProductService { get; private set; }
        protected IProductDbContext? DbContext { get; private set; }
        protected ILogger Logger { get; }
        protected ITestOutputHelper? Output { get; }

        // 添加属性来访问分类ID
        protected string ElectronicsCategoryId => MongoFixture.ElectronicsCategoryId;
        protected string FurnitureCategoryId => MongoFixture.FurnitureCategoryId;
        
        // 添加属性来访问产品ID
        protected string IPhoneProductId => MongoFixture.IPhoneProductId;
        protected string MacBookProductId => MongoFixture.MacBookProductId;
        protected string SofaProductId => MongoFixture.SofaProductId;

        public IntegrationTestBase(ITestOutputHelper? output = null)
        {
            Output = output;
            
            // 创建一个临时的服务集合和提供者，仅用于获取日志记录器
            var tempServices = new ServiceCollection();
            tempServices.AddLogging(builder => 
            {
                builder.AddConsole();
                if (output != null)
                {
                    builder.AddXUnit(output);
                }
            });
            
            var tempProvider = tempServices.BuildServiceProvider();
            var loggerFactory = tempProvider.GetService<ILoggerFactory>();
            Logger = loggerFactory?.CreateLogger<IntegrationTestBase>() 
                ?? throw new InvalidOperationException("無法創建日誌記錄器");
            
            // 創建 MongoDB 容器
            MongoFixture = new MongoDbFixture(Logger);
            
            Logger.LogInformation("Creating MongoDB container...");
            Logger.LogInformation("MongoDB container created.");
        }

        public async Task InitializeAsync()
        {
            try
            {
                Logger.LogInformation("Starting integration test initialization...");
                
                // 初始化 MongoDB 容器
                await MongoFixture.InitializeAsync();
                
                // 獲取數據庫上下文
                DbContext = MongoFixture.GetDbContext();
                
                // 配置服务容器
                var services = new ServiceCollection();
                
                // 註冊日誌服務
                services.AddLogging(builder => 
                {
                    builder.AddConsole();
                    if (Output != null)
                    {
                        builder.AddXUnit(Output);
                    }
                });
                
                // 註冊數據庫上下文
                services.AddSingleton<IProductDbContext>(DbContext);
                
                // 註冊模擬消息總線
                services.AddSingleton<IMessageBus, MockMessageBus>();
                
                // 註冊消息發布器
                services.AddSingleton<InventoryEventPublisher>();
                
                // 解決循環依賴問題：使用服務定位器模式
                services.AddTransient<IInventoryService>(sp => 
                {
                    return new InventoryService(
                        sp.GetRequiredService<IProductDbContext>(),
                        sp.GetRequiredService<ILogger<InventoryService>>(),
                        sp, // 注入服務提供者自身
                        sp.GetRequiredService<InventoryEventPublisher>());
                });
                
                // 註冊產品服務
                services.AddTransient<IProductService, ProductService.Services.ProductService>();
                
                // 建立服务提供者
                ServiceProvider = services.BuildServiceProvider();
                
                // 從服務容器獲取產品服務
                ProductService = ServiceProvider.GetRequiredService<IProductService>();
                
                Logger.LogInformation("Integration test initialization completed.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to initialize integration test.");
                throw;
            }
        }

        public async Task DisposeAsync()
        {
            try
            {
                Logger.LogInformation("Cleaning up integration test resources...");
                await MongoFixture.DisposeAsync();
                Logger.LogInformation("Integration test cleanup completed.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to clean up integration test resources.");
                // 不拋出異常，以免掩蓋測試失敗的原因
            }
        }
    }
}