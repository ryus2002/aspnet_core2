using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Models;
using AuthService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace AuthService.Tests.Services
{
    // 再次擴展 UserServiceTests 類別，添加更多測試方法
    public partial class UserServiceTests
    {
        [Fact]
        public async Task GetUserRoles_WithExistingUser_ShouldReturnRoles()
        {
            // Arrange
            var userId = "user1";
            
            var roles = new List<Role>
            {
                new Role { Id = "role1", Name = "User" },
                new Role { Id = "role2", Name = "Admin" }
            };
            
            var userRoles = new List<UserRole>
            {
                new UserRole { UserId = userId, RoleId = "role1" },
                new UserRole { UserId = userId, RoleId = "role2" }
            };
            
            _mockContext.Setup(c => c.Roles).ReturnsDbSet(roles);
            _mockContext.Setup(c => c.UserRoles).ReturnsDbSet(userRoles);

            // Act
            var result = await _userService.GetUserRoles(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain("User");
            result.Should().Contain("Admin");
        }

        [Fact]
        public async Task GetUserRoles_WithNonExistingUser_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = "nonexistentuser";
            
            var roles = new List<Role>
            {
                new Role { Id = "role1", Name = "User" },
                new Role { Id = "role2", Name = "Admin" }
            };
            
            var userRoles = new List<UserRole>();
            
            _mockContext.Setup(c => c.Roles).ReturnsDbSet(roles);
            _mockContext.Setup(c => c.UserRoles).ReturnsDbSet(userRoles);

            // Act
            var result = await _userService.GetUserRoles(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUserAndReturnUpdatedUser()
        {
            // Arrange
            var user = new User
            {
                Id = "user1",
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                UpdatedAt = DateTime.UtcNow.AddDays(-1) // 設置為昨天
            };

            var users = new List<User> { user };
            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            // 更新用戶的全名
            user.FullName = "Updated User Name";

            // Act
            var result = await _userService.UpdateUserAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("Updated User Name");
            result.UpdatedAt.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1)); // 確保更新時間已經更新

            _mockContext.Verify(c => c.Users.Update(It.IsAny<User>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}