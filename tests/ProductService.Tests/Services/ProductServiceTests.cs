using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductDbContext> _mockDbContext;
        private readonly Mock<IInventoryService> _mockInventoryService;
        private readonly Mock<ILogger<ProductService.Services.ProductService>> _mockLogger;
        private readonly IProductService _productService;

        public ProductServiceTests()
        {
            _mockDbContext = new Mock<IProductDbContext>();
            _mockInventoryService = new Mock<IInventoryService>();
            _mockLogger = new Mock<ILogger<ProductService.Services.ProductService>>();

            _productService = new ProductService.Services.ProductService(
                _mockDbContext.Object,
                _mockInventoryService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CreateProductAsync_WithValidRequest_ShouldCreateProduct()
        {
            // Arrange
            var categoryId = "category1";
            var request = new CreateProductRequest
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 100.00m,
                DiscountPrice = 90.00m,
                Currency = "TWD",
                CategoryId = categoryId,
                Status = "active",
                Tags = new List<string> { "test", "new" },
                StockQuantity = 10,
                LowStockThreshold = 3,
                Attributes = new Dictionary<string, object>
                {
                    { "color", "red" },
                    { "size", "medium" }
                }
            };

            // 設置模擬的分類查詢
            var mockCategoryCursor = new Mock<IAsyncCursor<Category>>();
            mockCategoryCursor.Setup(c => c.Current).Returns(new List<Category> { new Category { Id = categoryId } });
            mockCategoryCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            var mockCategoryCollection = new Mock<IMongoCollection<Category>>();
            mockCategoryCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Category>>(),
                It.IsAny<FindOptions<Category>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCategoryCursor.Object);

            _mockDbContext.Setup(c => c.Categories).Returns(mockCategoryCollection.Object);

            // 設置模擬的商品插入
            var mockProductCollection = new Mock<IMongoCollection<Product>>();
            mockProductCollection.Setup(c => c.InsertOneAsync(
                It.IsAny<Product>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockDbContext.Setup(c => c.Products).Returns(mockProductCollection.Object);

            // 設置庫存服務模擬
            _mockInventoryService.Setup(s => s.CreateInventoryChangeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(new InventoryChange());

            // Act
            var result = await _productService.CreateProductAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Description.Should().Be(request.Description);
            result.Price.Regular.Should().Be(request.Price);
            result.Price.Discount.Should().Be(request.DiscountPrice);
            result.Price.Currency.Should().Be(request.Currency);
            result.CategoryId.Should().Be(request.CategoryId);
            result.Status.Should().Be(request.Status);
            result.Tags.Should().BeEquivalentTo(request.Tags);
            result.Stock.Quantity.Should().Be(request.StockQuantity);
            result.Stock.Available.Should().Be(request.StockQuantity);
            result.Stock.LowStockThreshold.Should().Be(request.LowStockThreshold);
            result.Attributes.Should().BeEquivalentTo(request.Attributes);

            // 驗證庫存變動記錄被創建
            _mockInventoryService.Verify(s => s.CreateInventoryChangeAsync(
                It.IsAny<string>(),
                null,
                "increment",
                request.StockQuantity,
                "initial",
                null,
                null), Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_WithInvalidCategoryId_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new CreateProductRequest
            {
                Name = "Test Product",
                CategoryId = "nonexistent",
                StockQuantity = 10
            };

            // 設置模擬的分類查詢，返回空結果
            var mockCategoryCursor = new Mock<IAsyncCursor<Category>>();
            mockCategoryCursor.Setup(c => c.Current).Returns(new List<Category>());
            mockCategoryCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var mockCategoryCollection = new Mock<IMongoCollection<Category>>();
            mockCategoryCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Category>>(),
                It.IsAny<FindOptions<Category>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCategoryCursor.Object);

            _mockDbContext.Setup(c => c.Categories).Returns(mockCategoryCollection.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _productService.CreateProductAsync(request));
        }

        [Fact]
        public async Task GetProductByIdAsync_WithExistingProduct_ShouldReturnProduct()
        {
            // Arrange
            var productId = "product1";
            var product = new Product
            {
                Id = productId,
                Name = "Test Product"
            };

            // 設置模擬的商品查詢
            var mockCursor = new Mock<IAsyncCursor<Product>>();
            mockCursor.Setup(c => c.Current).Returns(new List<Product> { product });
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            var mockCollection = new Mock<IMongoCollection<Product>>();
            mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<FindOptions<Product>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mockDbContext.Setup(c => c.Products).Returns(mockCollection.Object);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Name.Should().Be(product.Name);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithNonExistingProduct_ShouldReturnNull()
        {
            // Arrange
            var productId = "nonexistent";

            // 設置模擬的商品查詢，返回空結果
            var mockCursor = new Mock<IAsyncCursor<Product>>();
            mockCursor.Setup(c => c.Current).Returns(new List<Product>());
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var mockCollection = new Mock<IMongoCollection<Product>>();
            mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<FindOptions<Product>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mockDbContext.Setup(c => c.Products).Returns(mockCollection.Object);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().BeNull();
        }
    }
}