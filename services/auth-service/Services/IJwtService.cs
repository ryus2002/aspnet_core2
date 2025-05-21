using AuthService.Models;
using System.Security.Claims;

namespace AuthService.Services
{
    /// <summary>
    /// JWT服務介面
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// 生成JWT令牌
        /// </summary>
        /// <param name="user">用戶</param>
        /// <param name="roles">用戶角色</param>
        /// <returns>令牌和過期時間</returns>
        Task<(string token, DateTime expires)> GenerateJwtToken(User user, IEnumerable<string> roles);
        
        /// <summary>
        /// 生成刷新令牌
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="ipAddress">IP地址</param>
        /// <returns>刷新令牌</returns>
        Task<RefreshToken> GenerateRefreshToken(string userId, string ipAddress);
        
        /// <summary>
        /// 從令牌獲取主體
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>主體</returns>
        Task<ClaimsPrincipal?> GetPrincipalFromToken(string token);
    }
}