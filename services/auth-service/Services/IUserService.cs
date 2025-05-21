using AuthService.DTOs;
using AuthService.Models;

namespace AuthService.Services
{
    /// <summary>
    /// 用戶服務介面
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 根據ID獲取用戶
        /// </summary>
        /// <param name="id">用戶ID</param>
        /// <returns>用戶，如不存在則返回null</returns>
        Task<User?> GetById(string id);
        
        /// <summary>
        /// 根據用戶名獲取用戶
        /// </summary>
        /// <param name="username">用戶名</param>
        /// <returns>用戶，如不存在則返回null</returns>
        Task<User?> GetByUsername(string username);
        
        /// <summary>
        /// 根據電子郵件獲取用戶
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>用戶，如不存在則返回null</returns>
        Task<User?> GetByEmail(string email);
        
        /// <summary>
        /// 根據刷新令牌獲取用戶
        /// </summary>
        /// <param name="refreshToken">刷新令牌</param>
        /// <returns>用戶，如不存在則返回null</returns>
        Task<User?> GetUserByRefreshToken(string refreshToken);
        
        /// <summary>
        /// 註冊新用戶
        /// </summary>
        /// <param name="request">註冊請求</param>
        /// <returns>創建的用戶</returns>
        Task<User> Register(RegisterRequest request);
        
        /// <summary>
        /// 驗證用戶密碼
        /// </summary>
        /// <param name="user">用戶</param>
        /// <param name="password">密碼</param>
        /// <returns>是否有效</returns>
        Task<bool> ValidatePassword(User user, string password);
        
        /// <summary>
        /// 獲取用戶的角色
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>角色名稱列表</returns>
        Task<IEnumerable<string>> GetUserRoles(string userId);
        
        /// <summary>
        /// 更新用戶
        /// </summary>
        /// <param name="user">用戶</param>
        /// <returns>更新後的用戶</returns>
        Task<User> UpdateUserAsync(User user);
    }
}