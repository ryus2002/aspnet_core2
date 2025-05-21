using System;
using System.Collections.Generic;
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
    // 擴展 PermissionServiceTests 類別，添加更多測試方法
    public partial class PermissionServiceTests
    {
        [Fact]
        public async Task AssignPermissionToRole_WithValidInputs_ShouldReturnTrue()
        {
            // Arrange
            var roleId = "role1";
            var permissionId = "perm1";

            var roles = new List<Role>
            {
                new Role { Id = roleId, Name = "Admin" }
            };

            var permissions = new List<Permission>
            {
                new Permission { Id = permissionId, Name = "product:read" }
            };

            var rolePermissions = new List<RolePermission>();

            _mockContext.Setup(c => c.Roles).ReturnsDbSet(roles);
            _mockContext.Setup(c => c.Permissions).ReturnsDbSet(permissions);
            _mockContext.Setup(c => c.RolePermissions).ReturnsDbSet(rolePermissions);

            _mockContext.Setup(c => c.RolePermissions.AddAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>()))
                .Callback((RolePermission rp, CancellationToken token) => rolePermissions.Add(rp));

            // Act
            var result = await _permissionService.AssignPermissionToRole(roleId, permissionId);

            // Assert
            result.Should().BeTrue();
            rolePermissions.Should().ContainSingle();
            rolePermissions[0].RoleId.Should().Be(roleId);
            rolePermissions[0].PermissionId.Should().Be(permissionId);

            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AssignPermissionToRole_WithNonExistingRole_ShouldReturnFalse()
        {
            // Arrange
            var roleId = "nonexistingrole";
            var permissionId = "perm1";

            var roles = new List<Role>(); // 空角色列表

            var permissions = new List<Permission>
            {
                new Permission { Id = permissionId, Name = "product:read" }
            };

            _mockContext.Setup(c => c.Roles).ReturnsDbSet(roles);
            _mockContext.Setup(c => c.Permissions).ReturnsDbSet(permissions);

            // Act
            var result = await _permissionService.AssignPermissionToRole(roleId, permissionId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AssignPermissionToRole_WithNonExistingPermission_ShouldReturnFalse()
        {
            // Arrange
            var roleId = "role1";
            var permissionId = "nonexistingperm";

            var roles = new List<Role>
            {
                new Role { Id = roleId, Name = "Admin" }
            };

            var permissions = new List<Permission>(); // 空權限列表

            _mockContext.Setup(c => c.Roles).ReturnsDbSet(roles);
            _mockContext.Setup(c => c.Permissions).ReturnsDbSet(permissions);

            // Act
            var result = await _permissionService.AssignPermissionToRole(roleId, permissionId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RemovePermissionFromRole_WithExistingRolePermission_ShouldReturnTrue()
        {
            // Arrange
            var roleId = "role1";
            var permissionId = "perm1";

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            };

            var rolePermissions = new List<RolePermission> { rolePermission };

            _mockContext.Setup(c => c.RolePermissions).ReturnsDbSet(rolePermissions);

            _mockContext.Setup(c => c.RolePermissions.Remove(It.IsAny<RolePermission>()))
                .Callback((RolePermission rp) => rolePermissions.Remove(rp));

            // Act
            var result = await _permissionService.RemovePermissionFromRole(roleId, permissionId);

            // Assert
            result.Should().BeTrue();
            rolePermissions.Should().BeEmpty();

            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RemovePermissionFromRole_WithNonExistingRolePermission_ShouldReturnFalse()
        {
            // Arrange
            var roleId = "role1";
            var permissionId = "perm1";

            var rolePermissions = new List<RolePermission>(); // 空角色權限列表

            _mockContext.Setup(c => c.RolePermissions).ReturnsDbSet(rolePermissions);

            // Act
            var result = await _permissionService.RemovePermissionFromRole(roleId, permissionId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreatePermission_WithValidPermission_ShouldReturnCreatedPermission()
        {
            // Arrange
            var permissions = new List<Permission>();

            _mockContext.Setup(c => c.Permissions).ReturnsDbSet(permissions);

            _mockContext.Setup(c => c.Permissions.AddAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
                .Callback((Permission p, CancellationToken token) => permissions.Add(p));

            var permission = new Permission
            {
                Name = "product:create",
                Description = "Create products",
                Resource = "product",
                Action = "create"
            };

            // Act
            var result = await _permissionService.CreatePermission(permission);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(permission.Name);
            result.Description.Should().Be(permission.Description);
            result.Resource.Should().Be(permission.Resource);
            result.Action.Should().Be(permission.Action);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            permissions.Should().ContainSingle();
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}