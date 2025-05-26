using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProductService.Models;
using ProductService.Models.Requests;
using Xunit;
using Xunit.Abstractions;

namespace ProductService.IntegrationTests.Services
{
    public class ProductServiceIntegrationTests : Infrastructure.IntegrationTestBase
    {
        private readonly ITestOutputHelper _output;

        public ProductServiceIntegrationTests(ITestOutputHelper output) : base(output)
        {
            _output = output;
                _logger.LogInformation("Starting ProductServiceIntegrationTests");
            }
        [Fact]
        public async Task GetProductByIdAsync_WithExistingProduct_ShouldReturnProduct()
        {
            // Arrange
            _logger.LogInformation("Testing GetProductByIdAsync with existing product");
            var productService = GetProductService();
            
            // 使用有效的 ObjectId
            var productId = MongoDbFixture.IPhoneProductId;
            _logger.LogInformation($"Fetching product with ID: {productId}");
            // Act
            var product = await productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(product);
            Assert.Equal(productId, product.Id);
            Assert.Equal("iPhone 15", product.Name);
        }
        [Fact]
        public async Task GetProductByIdAsync_WithNonExistingProduct_ShouldReturnNull()
        {
            // Arrange
            _logger.LogInformation("Testing GetProductByIdAsync with non-existing product");
            var productService = GetProductService();
                    
            // 使用有效的 ObjectId 格式，但数据库中不存在
            var nonExistentId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            _logger.LogInformation($"Fetching product with ID: {nonExistentId}");
            // Act
            var product = await productService.GetProductByIdAsync(nonExistentId);
            // Assert
            Assert.Null(product);
        }
        [Fact]
        public async Task GetProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange
            _logger.LogInformation("Testing GetProductsAsync");
            var productService = GetProductService();
            // Act
            var products = await productService.GetProductsAsync();
            // Assert
            Assert.NotNull(products);
            Assert.Equal(3, products.Count); // 我们在测试数据中添加了3个产品
    }

        [Fact]
        public async Task CreateProductAsync_WithValidRequest_ShouldCreateProduct()
        {
            // Arrange
            _logger.LogInformation("Testing CreateProductAsync with valid request");
            var productService = GetProductService();
            var inventoryService = GetInventoryService();
            
            var request = new CreateProductRequest
            {
                Name = "新測試產品",
                Description = "這是一個測試產品",
                Price = new PriceInfo
                {
                    Regular = 1000,
                    Discount = 900,
                    Currency = "TWD"
                },
                CategoryId = MongoDbFixture.ElectronicsCategoryId,
                Stock = new StockInfo
        {
                    Quantity = 50,
                    Reserved = 0,
                    Available = 50,
                    LowStockThreshold = 10
                },
                Status = "active"
            };
            
            _logger.LogInformation("Creating new product");

            // 创建一个 Mock 的 InventoryService 来替代实际的服务
            // 或者修复 InventoryService 中的空引用问题
            // Act
            var product = await productService.CreateProductAsync(request);
            // Assert
            Assert.NotNull(product);
            Assert.NotNull(product.Id);
            Assert.Equal(request.Name, product.Name);
            Assert.Equal(request.CategoryId, product.CategoryId);
            Assert.Equal(request.Stock.Quantity, product.Stock.Quantity);
        }
                
        [Fact]
        public async Task CreateProductAsync_WithInvalidCategoryId_ShouldThrowArgumentException()
                {
            // Arrange
            _logger.LogInformation("Testing CreateProductAsync with invalid category ID");
            var productService = GetProductService();
            
            // 使用有效的 ObjectId 格式，但不存在的分类ID
            var nonExistentCategoryId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            
            var request = new CreateProductRequest
            {
                Name = "無效分類產品",
                Description = "這個產品的分類不存在",
                Price = new PriceInfo
                {
                    Regular = 1000,
                    Discount = 900,
                    Currency = "TWD"
                },
                CategoryId = nonExistentCategoryId,
                Stock = new StockInfo
                    {
                    Quantity = 50,
                    Reserved = 0,
                    Available = 50,
                    LowStockThreshold = 10
                },
                Status = "active"
            };
            _logger.LogInformation("Expecting ArgumentException");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => productService.CreateProductAsync(request));
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldUpdateProduct()
        {
            // Arrange
            _logger.LogInformation("Testing UpdateProductAsync");
            var productService = GetProductService();
            var productId = MongoDbFixture.IPhoneProductId;
            
            var updateRequest = new UpdateProductRequest
            {
                Name = "Updated iPhone 15",
                Description = "更新的 iPhone 15 描述",
                Price = new PriceInfo
                {
                    Regular = 36000,
                    Discount = 34000,
                    Currency = "TWD"
                },
                Status = "active"
            };
            
            _logger.LogInformation($"Updating product with ID: {productId}");

            // Act
            var result = await productService.UpdateProductAsync(productId, updateRequest);
            var updatedProduct = await productService.GetProductByIdAsync(productId);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedProduct);
            Assert.Equal(updateRequest.Name, updatedProduct.Name);
            Assert.Equal(updateRequest.Description, updatedProduct.Description);
            Assert.Equal(updateRequest.Price.Regular, updatedProduct.Price.Regular);
    }

        [Fact]
        public async Task DeleteProductAsync_WithExistingProduct_ShouldReturnTrue()
        {
            // Arrange
            _logger.LogInformation("Testing DeleteProductAsync with existing product");
            var productService = GetProductService();
            var productId = MongoDbFixture.SofaProductId;
            
            _logger.LogInformation($"Deleting product with ID: {productId}");

            // Act
            var result = await productService.DeleteProductAsync(productId);
            var product = await productService.GetProductByIdAsync(productId);

            // Assert
            Assert.True(result);
            Assert.Null(product);
}

        [Fact]
        public async Task DeleteProductAsync_WithNonExistingProduct_ShouldReturnFalse()
        {
            // Arrange
            _logger.LogInformation("Testing DeleteProductAsync with non-existing product");
            var productService = GetProductService();
            
            // 使用有效的 ObjectId 格式，但不存在的产品ID
            var nonExistentId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            
            _logger.LogInformation($"Deleting product with ID: {nonExistentId}");

            // Act
            var result = await productService.DeleteProductAsync(nonExistentId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateProductStockAsync_ShouldUpdateStockAndCreateInventoryChange()
        {
            // Arrange
            _logger.LogInformation("Testing UpdateProductStockAsync");
            var productService = GetProductService();
            var productId = MongoDbFixture.IPhoneProductId;
            
            _logger.LogInformation($"Fetching original product with ID: {productId}");
            var originalProduct = await productService.GetProductByIdAsync(productId);
            Assert.NotNull(originalProduct);
            
            var originalQuantity = originalProduct.Stock.Quantity;
            var incrementQuantity = 20;
            var expectedQuantity = originalQuantity + incrementQuantity;
            
            var request = new UpdateStockRequest
            {
                Type = "increment",
                Quantity = incrementQuantity,
                Reason = "测试库存更新",
                ReferenceId = "test123",
                UserId = "user1"
            };
            
            _logger.LogInformation($"Updating stock for product with ID: {productId}");

            // 创建一个 Mock 的 InventoryService 来替代实际的服务
            // 或者修复 InventoryService 中的空引用问题

            // Act
            var result = await productService.UpdateProductStockAsync(productId, request);
            var updatedProduct = await productService.GetProductByIdAsync(productId);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedProduct);
            Assert.Equal(expectedQuantity, updatedProduct.Stock.Quantity);
        }
    }
}