using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace AuthService.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<AuthDbContext> _mockContext;
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            _mockContext = new Mock<AuthDbContext>();
            _userService = new UserService(_mockContext.Object);
        }

        [Fact]
        public async Task Register_WithValidRequest_ShouldCreateUser()
        {
            // Arrange
            var users = new List<User>();
            var roles = new List<Role>
            {
                new Role { Id = "role1", Name = "User", Description = "普通用戶", IsSystem = true }
            };
            var userRoles = new List<UserRole>();

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);
            _mockContext.Setup(c => c.Roles).ReturnsDbSet(roles);
            _mockContext.Setup(c => c.UserRoles).ReturnsDbSet(userRoles);

            _mockContext.Setup(c => c.Users.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback((User user, CancellationToken token) => users.Add(user));

            _mockContext.Setup(c => c.UserRoles.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()))
                .Callback((UserRole userRole, CancellationToken token) => userRoles.Add(userRole));

            var request = new RegisterRequest
            {
                Username = "newuser",
                Email = "newuser@example.com",
                FullName = "New User",
                Password = "Password123!"
            };

            // Act
            var result = await _userService.Register(request);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be(request.Username);
            result.Email.Should().Be(request.Email);
            result.FullName.Should().Be(request.FullName);
            result.PasswordHash.Should().NotBeNullOrEmpty();
            result.Salt.Should().NotBeNullOrEmpty();

            users.Should().ContainSingle();
            userRoles.Should().ContainSingle();
            userRoles[0].UserId.Should().Be(result.Id);
            userRoles[0].RoleId.Should().Be(roles[0].Id);

            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Register_WithExistingUsername_ShouldThrowException()
        {
            // Arrange
            var existingUser = new User
            {
                Id = "user1",
                Username = "existinguser",
                Email = "existing@example.com"
            };

            var users = new List<User> { existingUser };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            var request = new RegisterRequest
            {
                Username = "existinguser", // 使用已存在的用戶名
                Email = "newuser@example.com",
                FullName = "New User",
                Password = "Password123!"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _userService.Register(request));
        }

        [Fact]
        public async Task Register_WithExistingEmail_ShouldThrowException()
        {
            // Arrange
            var existingUser = new User
            {
                Id = "user1",
                Username = "existinguser",
                Email = "existing@example.com"
            };

            var users = new List<User> { existingUser };

            _mockContext.Setup(c => c.Users).ReturnsDbSet(users);

            var request = new RegisterRequest
            {
                Username = "newuser",
                Email = "existing@example.com", // 使用已存在的電子郵件
                FullName = "New User",
                Password = "Password123!"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _userService.Register(request));
        }
    }
}