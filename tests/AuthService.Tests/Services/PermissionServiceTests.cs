using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Models;
using AuthService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace AuthService.Tests.Services
{
    public class PermissionServiceTests
    {
        private readonly Mock<AuthDbContext> _mockContext;
        private readonly Mock<ILogger<PermissionService>> _mockLogger;
        private readonly IPermissionService _permissionService;

        public PermissionServiceTests()
        {
            _mockContext = new Mock<AuthDbContext>();
            _mockLogger = new Mock<ILogger<PermissionService>>();
            _permissionService = new PermissionService(_mockContext.Object, _mockLogger.Object);
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

            var userRoles = new List<UserRole>
            {
                new UserRole { UserId = userId, RoleId = roleId }
            };

            var rolePermissions = new List<RolePermission>
            {
                new RolePermission { RoleId = roleId, PermissionId = permissionId }
            };

            var permissions = new List<Permission>
            {
                new Permission { Id = permissionId, Resource = resource, Action = action }
            };

            _mockContext.Setup(c => c.UserRoles).ReturnsDbSet(userRoles);
            _mockContext.Setup(c => c.RolePermissions).ReturnsDbSet(rolePermissions);
            _mockContext.Setup(c => c.Permissions).ReturnsDbSet(permissions);

            // Act
            var result = await _permissionService.UserHasPermission(userId, resource, action);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UserHasPermission_WithInvalidPermission_ShouldReturnFalse()
        {
            // Arrange
            var userId = "user1";
            var roleId = "role1";
            var permissionId = "perm1";
            var resource = "product";
            var action = "read";

            var userRoles = new List<UserRole>
            {
                new UserRole { UserId = userId, RoleId = roleId }
            };

            var rolePermissions = new List<RolePermission>
            {
                new RolePermission { RoleId = roleId, PermissionId = permissionId }
            };

            var permissions = new List<Permission>
            {
                new Permission { Id = permissionId, Resource = resource, Action = "write" } // 不同的操作
            };

            _mockContext.Setup(c => c.UserRoles).ReturnsDbSet(userRoles);
            _mockContext.Setup(c => c.RolePermissions).ReturnsDbSet(rolePermissions);
            _mockContext.Setup(c => c.Permissions).ReturnsDbSet(permissions);

            // Act
            var result = await _permissionService.UserHasPermission(userId, resource, action);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserHasPermission_WithNoRoles_ShouldReturnFalse()
        {
            // Arrange
            var userId = "user1";
            var resource = "product";
            var action = "read";

            var userRoles = new List<UserRole>(); // 空角色列表

            _mockContext.Setup(c => c.UserRoles).ReturnsDbSet(userRoles);

            // Act
            var result = await _permissionService.UserHasPermission(userId, resource, action);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserHasPermissionByName_WithValidPermission_ShouldReturnTrue()
        {
            // Arrange
            var userId = "user1";
            var roleId = "role1";
            var permissionId = "perm1";
            var permissionName = "product:read";

            var userRoles = new List<UserRole>
            {
                new UserRole { UserId = userId, RoleId = roleId }
            };

            var rolePermissions = new List<RolePermission>
            {
                new RolePermission { RoleId = roleId, PermissionId = permissionId }
            };

            var permissions = new List<Permission>
            {
                new Permission { Id = permissionId, Name = permissionName }
            };

            _mockContext.Setup(c => c.UserRoles).ReturnsDbSet(userRoles);
            _mockContext.Setup(c => c.RolePermissions).ReturnsDbSet(rolePermissions);
            _mockContext.Setup(c => c.Permissions).ReturnsDbSet(permissions);

            // Act
            var result = await _permissionService.UserHasPermissionByName(userId, permissionName);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserPermissions_ShouldReturnUserPermissions()
        {
            // Arrange
            var userId = "user1";
            var roleId1 = "role1";
            var roleId2 = "role2";
            var permissionId1 = "perm1";
            var permissionId2 = "perm2";

            var userRoles = new List<UserRole>
            {
                new UserRole { UserId = userId, RoleId = roleId1 },
                new UserRole { UserId = userId, RoleId = roleId2 }
            };

            var rolePermissions = new List<RolePermission>
            {
                new RolePermission { RoleId = roleId1, PermissionId = permissionId1 },
                new RolePermission { RoleId = roleId2, PermissionId = permissionId2 }
            };

            var permissions = new List<Permission>
            {
                new Permission { Id = permissionId1, Name = "product:read", Resource = "product", Action = "read" },
                new Permission { Id = permissionId2, Name = "product:write", Resource = "product", Action = "write" }
            };

            _mockContext.Setup(c => c.UserRoles).ReturnsDbSet(userRoles);
            _mockContext.Setup(c => c.RolePermissions).ReturnsDbSet(rolePermissions);
            _mockContext.Setup(c => c.Permissions).ReturnsDbSet(permissions);

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