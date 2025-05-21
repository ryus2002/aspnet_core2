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
    public partial class PermissionServiceTests
    {
        [Fact]
        public async Task AssignPermissionToRole_WithValidInputs_ShouldReturnTrue()
        {
            // Arrange
            var roleId = "role1";
            var permissionId = "perm1";

            var role = new Role { 
                Id = roleId, 
                Name = "Admin",
                Description = "Administrator role" // 添加必填屬性
            };
            var permission = new Permission { 
                Id = permissionId, 
                Name = "product:read",
                Resource = "product", // 添加必填屬性
                Action = "read", // 添加必填屬性
                Description = "Read product permission" // 添加必填屬性
            };

            await _context.Roles.AddAsync(role);
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();
            // Act
            var result = await _permissionService.AssignPermissionToRole(roleId, permissionId);

            // Assert
            result.Should().BeTrue();
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
            rolePermission.Should().NotBeNull();
        }

        [Fact]
        public async Task AssignPermissionToRole_WithNonExistingRole_ShouldReturnFalse()
        {
            // Arrange
            var roleId = "nonexistent";
            var permissionId = "perm2";

            var permission = new Permission { 
                Id = permissionId, 
                Name = "product:read",
                Resource = "product", // 添加必填屬性
                Action = "read", // 添加必填屬性
                Description = "Read product permission" // 添加必填屬性
            };
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();
            // Act
            var result = await _permissionService.AssignPermissionToRole(roleId, permissionId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AssignPermissionToRole_WithNonExistingPermission_ShouldReturnFalse()
        {
            // Arrange
            var roleId = "role3";
            var permissionId = "nonexistent";

            var role = new Role { 
                Id = roleId, 
                Name = "User",
                Description = "Regular user role" // 添加必填屬性
            };
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
            // Act
            var result = await _permissionService.AssignPermissionToRole(roleId, permissionId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RemovePermissionFromRole_WithExistingRolePermission_ShouldReturnTrue()
        {
            // Arrange
            var roleId = "role4";
            var permissionId = "perm4";

            var rolePermission = new RolePermission { RoleId = roleId, PermissionId = permissionId };
            await _context.RolePermissions.AddAsync(rolePermission);
            await _context.SaveChangesAsync();
            // Act
            var result = await _permissionService.RemovePermissionFromRole(roleId, permissionId);

            // Assert
            result.Should().BeTrue();
            var removed = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
            removed.Should().BeNull();
        }

        [Fact]
        public async Task RemovePermissionFromRole_WithNonExistingRolePermission_ShouldReturnFalse()
        {
            // Arrange
            var roleId = "role5";
            var permissionId = "perm5";

            // 不添加任何角色權限
            // Act
            var result = await _permissionService.RemovePermissionFromRole(roleId, permissionId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreatePermission_WithValidPermission_ShouldReturnCreatedPermission()
        {
            // Arrange
            var permission = new Permission
            {
                Name = "product:delete",
                Resource = "product",
                Action = "delete",
                Description = "Delete products"
            };

            // Act
            var result = await _permissionService.CreatePermission(permission);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeNullOrEmpty();
            result.Name.Should().Be(permission.Name);
            result.Resource.Should().Be(permission.Resource);
            result.Action.Should().Be(permission.Action);
            result.Description.Should().Be(permission.Description);

            var savedPermission = await _context.Permissions.FindAsync(result.Id);
            savedPermission.Should().NotBeNull();
        }
    }
}