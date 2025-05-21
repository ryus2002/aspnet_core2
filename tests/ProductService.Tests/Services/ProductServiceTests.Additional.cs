using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using ProductService.DTOs;
using ProductService.Models;
using Xunit;

namespace ProductService.Tests.Services
{
    // 擴展 ProductServiceTests 類別，添加更多測試方法
    public partial class ProductServiceTests
    {
        [Fact]
        public async Task DeleteProductAsync_WithExistingProduct_ShouldReturnTrue()
        {
            // Arrange
            var productId = "product1";

            // 設置模擬的刪除結果
            var mockResult = new Mock<DeleteResult>();
            mockResult.Setup(r => r.DeletedCount).Returns(1);

            var mockCollection = new Mock<IMongoCollection<Product>>();
            mockCollection.Setup(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResult.Object);

            _mockDbContext.Setup(c => c.Products).Returns(mockCollection.Object);

            // Act
            var result = await _productService.DeleteProductAsync(productId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteProductAsync_WithNonExistingProduct_ShouldReturnFalse()
        {
            // Arrange
            var productId = "nonexistent";

            // 設置模擬的刪除結果
            var mockResult = new Mock<DeleteResult>();
            mockResult.Setup(r => r.DeletedCount).Returns(0);

            var mockCollection = new Mock<IMongoCollection<Product>>();
            mockCollection.Setup(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResult.Object);

            _mockDbContext.Setup(c => c.Products).Returns(mockCollection.Object);

            // Act
            var result = await _productService.DeleteProductAsync(productId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateProductAsync_WithValidRequest_ShouldReturnUpdatedProduct()
        {
            // Arrange
            var productId = "product1";
            var existingProduct = new Product
            {
                Id = productId,
                Name = "Original Name",
                Description = "Original Description",
                Price = new PriceInfo
                {
                    Regular = 100.00m,
                    Discount = 90.00m,
                    Currency = "TWD"
                },
                CategoryId = "category1",
                Status = "active"
            };

            var updateRequest = new UpdateProductRequest
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Price = 120.00m,
                DiscountPrice = 100.00m
            };

            // 設置模擬的商品查詢
            var mockFindCursor = new Mock<IAsyncCursor<Product>>();
            mockFindCursor.Setup(c => c.Current).Returns(new List<Product> { existingProduct });
            mockFindCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            // 設置模擬的更新結果
            var mockUpdateResult = new Mock<UpdateResult>();
            mockUpdateResult.Setup(r => r.ModifiedCount).Returns(1);

            var mockCollection = new Mock<IMongoCollection<Product>>();
            
            // 設置查詢模擬
            mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<FindOptions<Product>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockFindCursor.Object);
            
            // 設置更新模擬
            mockCollection.Setup(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<UpdateDefinition<Product>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockUpdateResult.Object);

            _mockDbContext.Setup(c => c.Products).Returns(mockCollection.Object);

            // 更新後的產品
            var updatedProduct = new Product
            {
                Id = productId,
                Name = updateRequest.Name,
                Description = updateRequest.Description,
                Price = new PriceInfo
                {
                    Regular = updateRequest.Price.Value,
                    Discount = updateRequest.DiscountPrice.Value,
                    Currency = "TWD"
                },
                CategoryId = "category1",
                Status = "active"
            };

            // 設置第二次查詢（獲取更新後的產品）
            var mockUpdatedCursor = new Mock<IAsyncCursor<Product>>();
            mockUpdatedCursor.Setup(c => c.Current).Returns(new List<Product> { updatedProduct });
            mockUpdatedCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            // 設置第二次查詢的模擬
            mockCollection.SetupSequence(c => c.FindAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<FindOptions<Product>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockFindCursor.Object)  // 第一次查詢
                .ReturnsAsync(mockUpdatedCursor.Object);  // 第二次查詢

            // Act
            var result = await _productService.UpdateProductAsync(productId, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Name.Should().Be(updateRequest.Name);
            result.Description.Should().Be(updateRequest.Description);
            result.Price.Regular.Should().Be(updateRequest.Price.Value);
            result.Price.Discount.Should().Be(updateRequest.DiscountPrice.Value);
        }

        [Fact]
        public async Task UpdateProductStockAsync_ShouldUpdateStockAndCreateInventoryChange()
        {
            // Arrange
            var productId = "product1";
            var existingProduct = new Product
            {
                Id = productId,
                Name = "Test Product",
                Stock = new StockInfo
                {
                    Quantity = 10,
                    Reserved = 2,
                    Available = 8,
                    LowStockThreshold = 3
                }
            };

            var updateRequest = new UpdateStockRequest
            {
                Quantity = 20,
                Reason = "restock"
            };

            // 設置模擬的商品查詢
            var mockFindCursor = new Mock<IAsyncCursor<Product>>();
            mockFindCursor.Setup(c => c.Current).Returns(new List<Product> { existingProduct });
            mockFindCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            // 設置模擬的更新結果
            var mockUpdateResult = new Mock<UpdateResult>();
            mockUpdateResult.Setup(r => r.ModifiedCount).Returns(1);

            var mockCollection = new Mock<IMongoCollection<Product>>();
            
            // 設置查詢模擬
            mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<FindOptions<Product>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockFindCursor.Object);
            
            // 設置更新模擬
            mockCollection.Setup(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<UpdateDefinition<Product>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockUpdateResult.Object);

            _mockDbContext.Setup(c => c.Products).Returns(mockCollection.Object);

            // 更新後的產品
            var updatedProduct = new Product
            {
                Id = productId,
                Name = "Test Product",
                Stock = new StockInfo
                {
                    Quantity = updateRequest.Quantity,
                    Reserved = 2,
                    Available = updateRequest.Quantity - 2,
                    LowStockThreshold = 3
                }
            };

            // 設置第二次查詢（獲取更新後的產品）
            var mockUpdatedCursor = new Mock<IAsyncCursor<Product>>();
            mockUpdatedCursor.Setup(c => c.Current).Returns(new List<Product> { updatedProduct });
            mockUpdatedCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            // 設置第二次查詢的模擬
            mockCollection.SetupSequence(c => c.FindAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<FindOptions<Product>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockFindCursor.Object)  // 第一次查詢
                .ReturnsAsync(mockUpdatedCursor.Object);  // 第二次查詢

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
            var result = await _productService.UpdateProductStockAsync(productId, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Stock.Quantity.Should().Be(updateRequest.Quantity);
            result.Stock.Available.Should().Be(updateRequest.Quantity - 2);

            // 驗證庫存變動記錄被創建
            _mockInventoryService.Verify(s => s.CreateInventoryChangeAsync(
                productId,
                null,
                "increment",
                10,  // 20 - 10 = 10 (新數量 - 舊數量)
                "restock",
                null,
                null), Times.Once);
        }
    }
}