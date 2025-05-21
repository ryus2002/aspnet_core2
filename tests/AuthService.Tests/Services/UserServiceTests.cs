using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AuthService.Tests.Services
{
    public partial class UserServiceTests
    {
        private readonly AuthDbContext _context;
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            // 使用 InMemory 數據庫創建 DbContext
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: $"AuthDb_{Guid.NewGuid()}")
                .Options;
            
            _context = new AuthDbContext(options);
            _userService = new UserService(_context);
        }

        [Fact]
        public async Task Register_WithValidRequest_ShouldCreateUser()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                Email = "new@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FullName = "New User"
            };

            // 創建一個系統角色，供 Register 方法使用
            var role = new Role { 
                Id = "2", 
                Name = "User", 
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Description = "Regular user role" // 添加必填屬性
            };
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.Register(registerRequest);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be(registerRequest.Username);
            result.Email.Should().Be(registerRequest.Email);
            result.FullName.Should().Be(registerRequest.FullName);
            
            // 驗證用戶已保存到數據庫
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == registerRequest.Username);
            savedUser.Should().NotBeNull();
            savedUser!.Email.Should().Be(registerRequest.Email);
            
            // 驗證密碼已哈希
            savedUser.PasswordHash.Should().NotBeNullOrEmpty();
            savedUser.Salt.Should().NotBeNullOrEmpty();
            
            // 驗證用戶角色已分配
            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == savedUser.Id);
            userRole.Should().NotBeNull();
            userRole!.RoleId.Should().Be(role.Id);
        }

        [Fact]
        public async Task Register_WithExistingUsername_ShouldThrowException()
        {
            // Arrange
            var existingUser = new User
            {
                Username = "existinguser",
                Email = "existing@example.com",
                PasswordHash = "hash",
                Salt = "salt",
                FullName = "Existing User", // 添加必填屬性
                LastLoginIp = "127.0.0.1" // 添加必填屬性
            };
            await _context.Users.AddAsync(existingUser);
            await _context.SaveChangesAsync();

            var registerRequest = new RegisterRequest
            {
                Username = "existinguser", // 相同的用戶名
                Email = "new@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FullName = "New User"
            };

            // Act & Assert
            await _userService.Invoking(s => s.Register(registerRequest))
                .Should().ThrowAsync<ApplicationException>()
                .WithMessage($"用戶名 '{registerRequest.Username}' 已被使用");
        }

        [Fact]
        public async Task Register_WithExistingEmail_ShouldThrowException()
        {
            // Arrange
            var existingUser = new User
            {
                Username = "existinguser",
                Email = "existing@example.com",
                PasswordHash = "hash",
                Salt = "salt",
                FullName = "Existing User", // 添加必填屬性
                LastLoginIp = "127.0.0.1" // 添加必填屬性
            };
            await _context.Users.AddAsync(existingUser);
            await _context.SaveChangesAsync();

            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                Email = "existing@example.com", // 相同的電子郵件
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FullName = "New User"
            };

            // Act & Assert
            await _userService.Invoking(s => s.Register(registerRequest))
                .Should().ThrowAsync<ApplicationException>()
                .WithMessage($"電子郵件 '{registerRequest.Email}' 已被使用");
        }
    }
}