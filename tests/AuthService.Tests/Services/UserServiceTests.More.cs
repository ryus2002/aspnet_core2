using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AuthService.Tests.Services
{
    public partial class UserServiceTests
    {
        // 輔助方法：創建測試用戶
        private User CreateTestUser(string id, string username, string email, string fullName = null)
        {
            return new User
            {
                Id = id,
                Username = username,
                Email = email,
                PasswordHash = "hash",
                Salt = "salt",
                FullName = fullName ?? $"{username} FullName",
                LastLoginIp = "127.0.0.1",
                IsActive = true,
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // 輔助方法：創建測試角色
        private Role CreateTestRole(string id, string name, string description = null, bool isSystem = true)
        {
            return new Role
            {
                Id = id,
                Name = name,
                Description = description ?? $"{name} Description",
                IsSystem = isSystem,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // 輔助方法：創建用戶角色關聯
        private UserRole CreateUserRole(string userId, string roleId)
        {
            var user = CreateTestUser(userId, $"user_{userId}", $"user_{userId}@example.com");
            var role = CreateTestRole(roleId, $"role_{roleId}");
            
            return new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow,
                User = user,
                Role = role
            };
        }

        // 輔助方法：創建測試權限
        private Permission CreateTestPermission(string id, string name, string resource = null, string action = null)
        {
            return new Permission
            {
                Id = id,
                Name = name,
                Description = $"{name} Description",
                Resource = resource ?? "TestResource",
                Action = action ?? "TestAction",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // 輔助方法：創建角色權限關聯
        private RolePermission CreateRolePermission(string roleId, string permissionId)
        {
            return new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            };
        }

        // 輔助方法：創建刷新令牌
        private RefreshToken CreateRefreshToken(string token, string userId, DateTime? expiryDate = null)
        {
            return new RefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiresAt = expiryDate ?? DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1" // 添加必要的非空屬性
            };
        }

        [Fact]
        public async Task GetUserRoles_WithExistingUser_ShouldReturnRoles()
        {
            // Arrange
            var userId = "user1";
            var roleId1 = "role1";
            var roleId2 = "role2";
            var user = CreateTestUser(userId, "testuser", "test@example.com");
            var roles = new List<Role>
            {
                CreateTestRole(roleId1, "User"),
                CreateTestRole(roleId2, "Admin")
            };
            
            // 先保存用戶和角色
            await _context.Users.AddAsync(user);
            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();
            
            // 創建用戶角色關聯
            var userRoles = new List<UserRole>
            {
                new UserRole { 
                    UserId = userId, 
                    RoleId = roleId1, 
                    User = user, 
                    Role = roles[0] 
                },
                new UserRole { 
                    UserId = userId, 
                    RoleId = roleId2, 
                    User = user, 
                    Role = roles[1] 
                }
            };

            await _context.UserRoles.AddRangeAsync(userRoles);
            await _context.SaveChangesAsync();

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
            var userId = "nonexistent";
            // 不添加任何用戶
            
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
            var user = CreateTestUser("user1", "oldusername", "old@example.com", "Old User");
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // 重要：清除 DbContext 的變更追踪器，避免實體追踪衝突
            _context.ChangeTracker.Clear();
            // 創建一個更新後的用戶對象
            var updatedUser = CreateTestUser("user1", "newusername", "new@example.com", "New User");
            updatedUser.CreatedAt = user.CreatedAt;
            updatedUser.UpdatedAt = user.UpdatedAt; // 這個會在 UpdateUserAsync 方法中被更新

            // Act
            var result = await _userService.UpdateUserAsync(updatedUser);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(updatedUser.Id);
            result.Username.Should().Be(updatedUser.Username);
            result.Email.Should().Be(updatedUser.Email);
            result.FullName.Should().Be(updatedUser.FullName);
            result.UpdatedAt.Should().BeAfter(user.UpdatedAt); // 檢查更新時間是否已更新

            // 驗證數據庫中的用戶已更新
            var savedUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
            savedUser.Should().NotBeNull();
            savedUser!.Username.Should().Be(updatedUser.Username);
            savedUser.Email.Should().Be(updatedUser.Email);
            savedUser.FullName.Should().Be(updatedUser.FullName);
            savedUser.UpdatedAt.Should().BeAfter(user.UpdatedAt);
        }
    }
}