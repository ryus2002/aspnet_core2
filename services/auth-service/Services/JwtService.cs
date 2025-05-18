using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthService.Models;
using AuthService.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    /// <summary>
    /// JWT服務接口，定義JWT相關操作
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// 為用戶生成JWT訪問令牌
        /// </summary>
        /// <param name="user">用戶對象</param>
        /// <param name="roles">用戶角色列表</param>
        /// <returns>JWT令牌和過期時間</returns>
        Task<(string token, DateTime expires)> GenerateJwtToken(User user, List<string> roles);
        
        /// <summary>
        /// 生成刷新令牌
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="ipAddress">IP地址</param>
        /// <returns>刷新令牌對象</returns>
        Task<RefreshToken> GenerateRefreshToken(string userId, string ipAddress);
        
        /// <summary>
        /// 從令牌中獲取聲明主體
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>聲明主體</returns>
        Task<ClaimsPrincipal> GetPrincipalFromToken(string token);
    }
    
    /// <summary>
    /// JWT服務實現類
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        
        /// <summary>
        /// 構造函數，注入JWT設置
        /// </summary>
        /// <param name="jwtSettings">JWT設置</param>
        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }
        
        /// <summary>
        /// 為用戶生成JWT訪問令牌
        /// </summary>
        /// <param name="user">用戶對象</param>
        /// <param name="roles">用戶角色列表</param>
        /// <returns>JWT令牌和過期時間</returns>
        public async Task<(string token, DateTime expires)> GenerateJwtToken(User user, List<string> roles)
        {
            // 設置JWT令牌的過期時間
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
            
            // 創建JWT令牌處理器
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // 獲取密鑰的字節數組
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            
            // 創建聲明列表
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Username)
            };
            
            // 添加角色聲明
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            // 創建JWT令牌描述符
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };
            
            // 創建JWT令牌
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            // 將JWT令牌序列化為字符串
            return (tokenHandler.WriteToken(token), expires);
        }
        
        /// <summary>
        /// 生成刷新令牌
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="ipAddress">IP地址</param>
        /// <returns>刷新令牌對象</returns>
        public async Task<RefreshToken> GenerateRefreshToken(string userId, string ipAddress)
        {
            // 創建刷新令牌
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = GenerateRandomToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
            
            return refreshToken;
        }
        
        /// <summary>
        /// 從令牌中獲取聲明主體
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>聲明主體</returns>
        public async Task<ClaimsPrincipal> GetPrincipalFromToken(string token)
        {
            try
            {
                // 創建JWT令牌處理器
                var tokenHandler = new JwtSecurityTokenHandler();
                
                // 獲取密鑰的字節數組
                var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
                
                // 設置令牌驗證參數
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = false // 不驗證過期時間，因為刷新令牌時可能已經過期
                };
                
                // 驗證令牌
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                
                // 檢查令牌類型
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("無效的令牌");
                }
                
                return principal;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// 生成隨機令牌
        /// </summary>
        /// <returns>隨機令牌字符串</returns>
        private string GenerateRandomToken()
        {
            // 使用隨機數生成器創建一個隨機字節數組
            var randomBytes = new byte[40];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            
            // 將字節數組轉換為Base64字符串
            return Convert.ToBase64String(randomBytes);
        }
    }
}