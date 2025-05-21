using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Models;
using AuthService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace AuthService.Tests.Services
{
    // 擴展 UserServiceTests 類別，添加更多測試方法
    public partial class UserServiceTests
    {
        [Fact]
        public async Task GetByUsername_WithExistingUser_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                Id = "user1",
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash", // 添加必填屬性
                Salt = "salt", // 添加必填屬性
                FullName = "Test User", // 添加必填屬性
                LastLoginIp = "127.0.0.1", // 添加必填屬性
                RefreshTokens = new List<RefreshToken>()
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetByUsername("testuser");

            // Assert
            result.Should().NotBeNull();
            result?.Id.Should().Be(user.Id);
            result?.Username.Should().Be(user.Username);
            result?.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetByUsername_WithNonExistingUser_ShouldReturnNull()
        {
            // Arrange
            // 不添加任何用戶
            // Act
            var result = await _userService.GetByUsername("nonexistentuser");
            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetById_WithExistingUser_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                Id = "user1",
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash", // 添加必填屬性
                Salt = "salt", // 添加必填屬性
                FullName = "Test User", // 添加必填屬性
                LastLoginIp = "127.0.0.1", // 添加必填屬性
                RefreshTokens = new List<RefreshToken>()
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetById("user1");

            // Assert
            result.Should().NotBeNull();
            result?.Id.Should().Be(user.Id);
            result?.Username.Should().Be(user.Username);
            result?.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetById_WithNonExistingUser_ShouldReturnNull()
        {
            // Arrange
            // 不添加任何用戶
            // Act
            var result = await _userService.GetById("nonexistentid");
            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByEmail_WithExistingUser_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                Id = "user1",
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash", // 添加必填屬性
                Salt = "salt", // 添加必填屬性
                FullName = "Test User", // 添加必填屬性
                LastLoginIp = "127.0.0.1", // 添加必填屬性
                RefreshTokens = new List<RefreshToken>()
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetByEmail("test@example.com");

            // Assert
            result.Should().NotBeNull();
            result?.Id.Should().Be(user.Id);
            result?.Username.Should().Be(user.Username);
            result?.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetUserByRefreshToken_WithExistingToken_ShouldReturnUser()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                Id = "token1",
                Token = "valid-refresh-token",
                UserId = "user1",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1",
                // 添加必填屬性，即使它們是空的或默認值
                ReplacedByToken = string.Empty,
                RevokedByIp = string.Empty,
                IsRevoked = false
            };

            var user = new User
            {
                Id = "user1",
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Salt = "salt",
                FullName = "Test User",
                LastLoginIp = "127.0.0.1",
                RefreshTokens = new List<RefreshToken>()
            };

            // 重要：先添加用戶，然後再添加刷新令牌
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // 然後添加刷新令牌
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByRefreshToken("valid-refresh-token");

            // Assert
            result.Should().NotBeNull();
            result?.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task ValidatePassword_WithCorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            // 使用一個已知的密碼和鹽值創建哈希
            var password = "Password123!";
            var salt = "dGVzdHNhbHQ="; // base64 encoded "testsalt"
            
            // 模擬 UserService 中的哈希方法
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password + salt);
            var hashBytes = System.Security.Cryptography.SHA256.Create().ComputeHash(passwordBytes);
            var passwordHash = Convert.ToBase64String(hashBytes);

            var user = new User
            {
                Id = "user1",
                Username = "testuser", // 添加必填屬性
                Email = "test@example.com", // 添加必填屬性
                PasswordHash = passwordHash,
                Salt = salt,
                FullName = "Test User", // 添加必填屬性
                LastLoginIp = "127.0.0.1" // 添加必填屬性
            };

            // Act
            var result = await _userService.ValidatePassword(user, password);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidatePassword_WithIncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            // 使用一個已知的密碼和鹽值創建哈希
            var correctPassword = "Password123!";
            var wrongPassword = "WrongPassword!";
            var salt = "dGVzdHNhbHQ="; // base64 encoded "testsalt"
            
            // 模擬 UserService 中的哈希方法
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(correctPassword + salt);
            var hashBytes = System.Security.Cryptography.SHA256.Create().ComputeHash(passwordBytes);
            var passwordHash = Convert.ToBase64String(hashBytes);

            var user = new User
            {
                Id = "user1",
                Username = "testuser", // 添加必填屬性
                Email = "test@example.com", // 添加必填屬性
                PasswordHash = passwordHash,
                Salt = salt,
                FullName = "Test User", // 添加必填屬性
                LastLoginIp = "127.0.0.1" // 添加必填屬性
            };

            // Act
            var result = await _userService.ValidatePassword(user, wrongPassword);

            // Assert
            result.Should().BeFalse();
        }
    }
}