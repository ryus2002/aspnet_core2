using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthService.Models;
using AuthService.Services;
using AuthService.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AuthService.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IJwtService _jwtService;

        public JwtServiceTests()
        {
            // 設置測試用的JWT設置
            _jwtSettings = new JwtSettings
            {
                Secret = "ThisIsAVerySecureTestSecretKeyWithAtLeast32Chars",
                Issuer = "test-issuer",
                Audience = "test-audience",
                AccessTokenExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7
            };

            var options = new Mock<IOptions<JwtSettings>>();
            options.Setup(o => o.Value).Returns(_jwtSettings);

            _jwtService = new JwtService(options.Object);
        }

        [Fact]
        public async Task GenerateJwtToken_ShouldReturnValidToken()
        {
            // Arrange
            var user = new User
            {
                Id = "user123",
                Username = "testuser",
                Email = "test@example.com"
            };
            var roles = new List<string> { "User" };

            // Act
            var (token, expires) = await _jwtService.GenerateJwtToken(user, roles);

            // Assert
            token.Should().NotBeNullOrEmpty();
            expires.Should().BeAfter(DateTime.UtcNow);
            expires.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes), TimeSpan.FromSeconds(5));

            // 驗證令牌內容
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
            jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
            
            // 驗證聲明
            jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
            jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
            jwtToken.Claims.Should().Contain(c => c.Type == "username" && c.Value == user.Username);
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == roles[0]);
        }

        [Fact]
        public async Task GenerateRefreshToken_ShouldReturnValidRefreshToken()
        {
            // Arrange
            var userId = "user123";
            var ipAddress = "127.0.0.1";

            // Act
            var refreshToken = await _jwtService.GenerateRefreshToken(userId, ipAddress);

            // Assert
            refreshToken.Should().NotBeNull();
            refreshToken.UserId.Should().Be(userId);
            refreshToken.Token.Should().NotBeNullOrEmpty();
            refreshToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            refreshToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays), TimeSpan.FromSeconds(5));
            refreshToken.CreatedByIp.Should().Be(ipAddress);
        }

        [Fact]
        public async Task GetPrincipalFromToken_WithValidToken_ShouldReturnPrincipal()
        {
            // Arrange
            var user = new User
            {
                Id = "user123",
                Username = "testuser",
                Email = "test@example.com"
            };
            var roles = new List<string> { "User" };
            var (token, _) = await _jwtService.GenerateJwtToken(user, roles);

            // Act
            var principal = await _jwtService.GetPrincipalFromToken(token);

            // Assert
            principal.Should().NotBeNull();
            principal.Identity.Should().NotBeNull();
            principal.Identity!.IsAuthenticated.Should().BeTrue();
            principal.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
            principal.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
            principal.Claims.Should().Contain(c => c.Type == "username" && c.Value == user.Username);
            principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == roles[0]);
        }

        [Fact]
        public async Task GetPrincipalFromToken_WithInvalidToken_ShouldReturnNull()
        {
            // Arrange
            var invalidToken = "invalid.token.string";

            // Act
            var principal = await _jwtService.GetPrincipalFromToken(invalidToken);

            // Assert
            principal.Should().BeNull();
        }
    }
}