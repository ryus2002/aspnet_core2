using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Data;
using AuthService.Models;
using AuthService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthService.Tests.Services
{
    public partial class PermissionServiceTests
    {
        private readonly AuthDbContext _context;
        private readonly Mock<ILogger<PermissionService>> _mockLogger;
        private readonly IPermissionService _permissionService;

        public PermissionServiceTests()
        {
            // 使用 InMemory 數據庫創建 DbContext
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: $"AuthDb_{Guid.NewGuid()}")
                .Options;
            
            _context = new AuthDbContext(options);
            _mockLogger = new Mock<ILogger<PermissionService>>();
            _permissionService = new PermissionService(_context, _mockLogger.Object);
        }
        
        [Fact]
        public async Task UserHasPermission_WithValidPermission_ShouldReturnTrue()
        {
            // Arrange
            var userId = "user1";
            var roleId = "role1";
            var permissionId = "perm1";
            var resource = "product";
            var action = "read";

            // 創建用戶和角色
            var user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Salt = "salt",
                FullName = "Test User",
                LastLoginIp = "127.0.0.1",
                IsActive = true,
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var role = new Role
            {
                Id = roleId,
                Name = "TestRole",
                Description = "Test Role Description",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 設置測試數據
            var userRole = new UserRole { 
                UserId = userId, 
                RoleId = roleId,
                User = user,
                Role = role
            };
            
            var rolePermission = new RolePermission { RoleId = roleId, PermissionId = permissionId };
            var permission = new Permission { 
                Id = permissionId, 
                Name = "product:read", // 添加必填屬性
                Resource = resource, 
                Action = action,
                Description = "Read product permission" // 添加必填屬性
            };

            await _context.Users.AddAsync(user);
            await _context.Roles.AddAsync(role);
            await _context.UserRoles.AddAsync(userRole);
            await _context.RolePermissions.AddAsync(rolePermission);
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _permissionService.UserHasPermission(userId, resource, action);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UserHasPermission_WithInvalidPermission_ShouldReturnFalse()
        {
            // Arrange
            var userId = "user2";
            var roleId = "role2";
            var permissionId = "perm2";
            var resource = "product";
            var action = "read";

            // 創建用戶和角色
            var user = new User
            {
                Id = userId,
                Username = "testuser2",
                Email = "test2@example.com",
                PasswordHash = "hash",
                Salt = "salt",
                FullName = "Test User 2",
                LastLoginIp = "127.0.0.1",
                IsActive = true,
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var role = new Role
            {
                Id = roleId,
                Name = "TestRole2",
                Description = "Test Role 2 Description",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 設置測試數據
            var userRole = new UserRole { 
                UserId = userId, 
                RoleId = roleId,
                User = user,
                Role = role
            };
            
            var rolePermission = new RolePermission { RoleId = roleId, PermissionId = permissionId };
            var permission = new Permission { 
                Id = permissionId, 
                Name = "product:write", // 添加必填屬性
                Resource = resource, 
                Action = "write", // 不同的操作
                Description = "Write product permission" // 添加必填屬性
            };

            await _context.Users.AddAsync(user);
            await _context.Roles.AddAsync(role);
            await _context.UserRoles.AddAsync(userRole);
            await _context.RolePermissions.AddAsync(rolePermission);
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionService.UserHasPermission(userId, resource, action);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserHasPermission_WithNoRoles_ShouldReturnFalse()
        {
            // Arrange
            var userId = "user3";
            var resource = "product";
            var action = "read";
            // 不添加任何用戶角色
            // Act
            var result = await _permissionService.UserHasPermission(userId, resource, action);
            // Assert
            result.Should().BeFalse();
        }
            
        [Fact]
        public async Task UserHasPermissionByName_WithValidPermission_ShouldReturnTrue()
        {
            // Arrange
            var userId = "user4";
            var roleId = "role4";
            var permissionId = "perm4";
            var permissionName = "product:read";
            
            // 創建用戶和角色
            var user = new User
            {
                Id = userId,
                Username = "testuser4",
                Email = "test4@example.com",
                PasswordHash = "hash",
                Salt = "salt",
                FullName = "Test User 4",
                LastLoginIp = "127.0.0.1",
                IsActive = true,
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var role = new Role
            {
                Id = roleId,
                Name = "TestRole4",
                Description = "Test Role 4 Description",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 設置測試數據
            var userRole = new UserRole { 
                UserId = userId, 
                RoleId = roleId,
                User = user,
                Role = role
            };
            
            var rolePermission = new RolePermission { RoleId = roleId, PermissionId = permissionId };
            var permission = new Permission { 
                Id = permissionId, 
                Name = permissionName,
                Resource = "product", // 添加必填屬性
                Action = "read", // 添加必填屬性
                Description = "Read product permission" // 添加必填屬性
            };

            await _context.Users.AddAsync(user);
            await _context.Roles.AddAsync(role);
            await _context.UserRoles.AddAsync(userRole);
            await _context.RolePermissions.AddAsync(rolePermission);
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _permissionService.UserHasPermissionByName(userId, permissionName);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserPermissions_ShouldReturnUserPermissions()
        {
            // Arrange
            var userId = "user5";
            var roleId1 = "role5a";
            var roleId2 = "role5b";
            var permissionId1 = "perm5a";
            var permissionId2 = "perm5b";

            // 創建用戶和角色
            var user = new User
            {
                Id = userId,
                Username = "testuser5",
                Email = "test5@example.com",
                PasswordHash = "hash",
                Salt = "salt",
                FullName = "Test User 5",
                LastLoginIp = "127.0.0.1",
                IsActive = true,
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var role1 = new Role
            {
                Id = roleId1,
                Name = "TestRole5a",
                Description = "Test Role 5a Description",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var role2 = new Role
            {
                Id = roleId2,
                Name = "TestRole5b",
                Description = "Test Role 5b Description",
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 設置測試數據
            var userRoles = new List<UserRole> 
            { 
                new UserRole { 
                    UserId = userId, 
                    RoleId = roleId1,
                    User = user,
                    Role = role1
                },
                new UserRole { 
                    UserId = userId, 
                    RoleId = roleId2,
                    User = user,
                    Role = role2
                }
            };
            
            var rolePermissions = new List<RolePermission> 
            { 
                new RolePermission { RoleId = roleId1, PermissionId = permissionId1 },
                new RolePermission { RoleId = roleId2, PermissionId = permissionId2 }
            };
            
            var permissions = new List<Permission> 
            { 
                new Permission { 
                    Id = permissionId1, 
                    Name = "product:read", 
                    Resource = "product", 
                    Action = "read",
                    Description = "Read product permission" // 添加必填屬性
                },
                new Permission { 
                    Id = permissionId2, 
                    Name = "product:write", 
                    Resource = "product", 
                    Action = "write",
                    Description = "Write product permission" // 添加必填屬性
                }
            };

            await _context.Users.AddAsync(user);
            await _context.Roles.AddRangeAsync(new[] { role1, role2 });
            await _context.UserRoles.AddRangeAsync(userRoles);
            await _context.RolePermissions.AddRangeAsync(rolePermissions);
            await _context.Permissions.AddRangeAsync(permissions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionService.GetUserPermissions(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Name == "product:read");
            result.Should().Contain(p => p.Name == "product:write");
        }
    }
}