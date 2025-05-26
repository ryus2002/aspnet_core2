using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using ProductService.Data;
using ProductService.Models;
using ProductService.Settings;
using Testcontainers.MongoDb;

namespace ProductService.IntegrationTests.Infrastructure
{
    public class MongoDbFixture : IAsyncDisposable
    {
        private readonly MongoDbContainer _mongoDbContainer;
        private IMongoClient? _mongoClient;
        private IMongoDatabase? _database;
        private ProductDbContext? _dbContext;
        private readonly ILogger? _logger;

        // 添加公共属性存储分类ID
        public string ElectronicsCategoryId { get; private set; } = string.Empty;
        public string FurnitureCategoryId { get; private set; } = string.Empty;
        
        // 添加公共属性存储产品ID
        public string IPhoneProductId { get; private set; } = string.Empty;
        public string MacBookProductId { get; private set; } = string.Empty;
        public string SofaProductId { get; private set; } = string.Empty;

        public MongoDbFixture(ILogger? logger = null)
        {
            _logger = logger;
            
            _logger?.LogInformation("Creating MongoDB container...");
            
            _mongoDbContainer = new MongoDbBuilder()
                .WithImage("mongo:6.0")
                .WithPortBinding(27017, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
                .Build();
                
            _logger?.LogInformation("MongoDB container created.");
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger?.LogInformation("Starting MongoDB container...");
                await _mongoDbContainer.StartAsync();
                _logger?.LogInformation("MongoDB container started.");
                
                var connectionString = _mongoDbContainer.GetConnectionString();
                _logger?.LogInformation($"MongoDB connection string: {connectionString}");
                
                _mongoClient = new MongoClient(connectionString);
                _database = _mongoClient.GetDatabase("product_test_db");
                
                // 初始化資料庫上下文 - 使用 MongoDbSettings 而不是 ProductDatabaseSettings
                var settings = new MongoDbSettings
                {
                    ConnectionString = connectionString,
                    DatabaseName = "product_test_db"
                };
                
                // 使用 Options 包裝 MongoDbSettings
                var options = Options.Create(settings);
                _dbContext = new ProductDbContext(options);
                
                // 創建集合和索引
                _logger?.LogInformation("Setting up collections...");
                await SetupCollectionsAsync();
                
                // 添加測試數據
                _logger?.LogInformation("Seeding test data...");
                await SeedDataAsync();
                
                _logger?.LogInformation("MongoDB initialization completed.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize MongoDB container.");
                throw;
            }
        }

        private async Task SetupCollectionsAsync()
        {
            try
            {
                if (_database == null)
                {
                    throw new InvalidOperationException("Database is not initialized");
                }
                
                // 確保集合存在
                var collections = await _database.ListCollectionNamesAsync();
                var collectionList = await collections.ToListAsync();
                
                if (!collectionList.Contains("products"))
                    await _database.CreateCollectionAsync("products");
                
                if (!collectionList.Contains("categories"))
                    await _database.CreateCollectionAsync("categories");
                
                if (!collectionList.Contains("inventory_changes"))
                    await _database.CreateCollectionAsync("inventory_changes");
                
                // 只创建非文本索引，避免与已存在的文本索引冲突
                var productsCollection = _database.GetCollection<Product>("products");
                var categoriesCollection = _database.GetCollection<Category>("categories");
                
                // 產品索引 - 只保留非文本索引
                var productIndexKeys = Builders<Product>.IndexKeys.Ascending(p => p.CategoryId);
                await productsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Product>(productIndexKeys));
                
                // 分類索引 - 也移除文本索引
                var categoryIndexKeys = Builders<Category>.IndexKeys.Ascending(c => c.Name);
                await categoriesCollection.Indexes.CreateOneAsync(new CreateIndexModel<Category>(categoryIndexKeys));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to setup collections.");
                throw;
            }
        }

        private async Task SeedDataAsync()
        {
            try
            {
                if (_database == null)
                {
                    throw new InvalidOperationException("Database is not initialized");
                }
                
                // 添加測試分類 - 使用有效的 ObjectId
                ElectronicsCategoryId = ObjectId.GenerateNewId().ToString();
                FurnitureCategoryId = ObjectId.GenerateNewId().ToString();
                
                var categoriesCollection = _database.GetCollection<Category>("categories");
                await categoriesCollection.InsertManyAsync(new[]
                {
                    new Category { 
                        Id = ElectronicsCategoryId, 
                        Name = "電子產品", 
                        Description = "所有電子產品",
                        Slug = "electronics",
                        Level = 0,
                        Path = "/",
                        Status = "active"
                    },
                    new Category { 
                        Id = FurnitureCategoryId, 
                        Name = "家具", 
                        Description = "家具和家居用品",
                        Slug = "furniture",
                        Level = 0,
                        Path = "/",
                        Status = "active"
                    }
                });
                
                // 添加測試產品 - 使用有效的 ObjectId 作为产品ID
                IPhoneProductId = ObjectId.GenerateNewId().ToString();
                MacBookProductId = ObjectId.GenerateNewId().ToString();
                SofaProductId = ObjectId.GenerateNewId().ToString();
                
                _logger?.LogInformation($"Generated product IDs: iPhone={IPhoneProductId}, MacBook={MacBookProductId}, Sofa={SofaProductId}");
                
                var productsCollection = _database.GetCollection<Product>("products");
                await productsCollection.InsertManyAsync(new[]
                {
                    new Product
                    {
                        Id = IPhoneProductId,
                        Name = "iPhone 15",
                        Description = "最新款 iPhone",
                        Price = new PriceInfo
                        {
                            Regular = 35000,
                            Discount = 33000,
                            Currency = "TWD"
                        },
                        CategoryId = ElectronicsCategoryId,
                        Status = "active",
                        Stock = new StockInfo
                        {
                            Quantity = 100,
                            Reserved = 10,
                            Available = 90,
                            LowStockThreshold = 20
                        }
                    },
                    new Product
                    {
                        Id = MacBookProductId,
                        Name = "MacBook Pro",
                        Description = "專業級筆記型電腦",
                        Price = new PriceInfo
                        {
                            Regular = 58000,
                            Discount = 55000,
                            Currency = "TWD"
                        },
                        CategoryId = ElectronicsCategoryId,
                        Status = "active",
                        Stock = new StockInfo
                        {
                            Quantity = 50,
                            Reserved = 5,
                            Available = 45,
                            LowStockThreshold = 10
                        }
                    },
                    new Product
                    {
                        Id = SofaProductId,
                        Name = "沙發",
                        Description = "舒適的三人座沙發",
                        Price = new PriceInfo
                        {
                            Regular = 25000,
                            Discount = 22000,
                            Currency = "TWD"
                        },
                        CategoryId = FurnitureCategoryId,
                        Status = "active",
                        Stock = new StockInfo
                        {
                            Quantity = 30,
                            Reserved = 2,
                            Available = 28,
                            LowStockThreshold = 5
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to seed test data.");
                throw;
            }
        }

        public IProductDbContext GetDbContext() => _dbContext ?? throw new InvalidOperationException("DbContext is not initialized");

        public async ValueTask DisposeAsync()
        {
            try
            {
                _logger?.LogInformation("Stopping MongoDB container...");
                await _mongoDbContainer.DisposeAsync();
                _logger?.LogInformation("MongoDB container stopped.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to stop MongoDB container.");
                // 不拋出異常，以免掩蓋測試失敗的原因
            }
        }
    }
}