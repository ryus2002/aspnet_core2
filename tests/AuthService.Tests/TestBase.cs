using System;
using AuthService.Models;

namespace AuthService.Tests
{
    public abstract class TestBase
    {
        // 輔助方法：創建測試用戶
        protected User CreateTestUser(string id, string username, string email, string fullName = null)
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
        protected Role CreateTestRole(string id, string name, string description = null, bool isSystem = true)
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
        protected UserRole CreateUserRole(string userId, string roleId)
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
        protected Permission CreateTestPermission(string id, string name, string resource = null, string action = null)
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
        protected RolePermission CreateRolePermission(string roleId, string permissionId)
        {
            return new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            };
        }

        // 輔助方法：創建刷新令牌
        protected RefreshToken CreateRefreshToken(string token, string userId, DateTime? expiryDate = null)
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
    }
}