using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthService.DTOs;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{
    /// <summary>
    /// 用戶服務接口，定義用戶相關的業務邏輯操作
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 註冊新用戶
        /// </summary>
        /// <param name="request">註冊請求</param>
        /// <returns>註冊成功的用戶</returns>
        Task<User> Register(RegisterRequest request);
        
        /// <summary>
        /// 根據用戶名查詢用戶
        /// </summary>
        /// <param name="username">用戶名</param>
        /// <returns>用戶對象，如果不存在則返回null</returns>
        Task<User> GetByUsername(string username);
        
        /// <summary>
        /// 根據ID查詢用戶
        /// </summary>
        /// <param name="id">用戶ID</param>
        /// <returns>用戶對象，如果不存在則返回null</returns>
        Task<User> GetById(string id);
        
        /// <summary>
        /// 根據電子郵件查詢用戶
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>用戶對象，如果不存在則返回null</returns>
        Task<User> GetByEmail(string email);
        
        /// <summary>
        /// 驗證用戶密碼
        /// </summary>
        /// <param name="user">用戶對象</param>
        /// <param name="password">待驗證的密碼</param>
        /// <returns>密碼是否正確</returns>
        Task<bool> ValidatePassword(User user, string password);
        
        /// <summary>
        /// 獲取用戶的角色列表
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>角色名稱列表</returns>
        Task<List<string>> GetUserRoles(string userId);
    }
    
    /// <summary>
    /// 用戶服務實現類
    /// </summary>
    public class UserService : IUserService
    {
        private readonly AuthDbContext _context;
        
        /// <summary>
        /// 構造函數，注入數據庫上下文
        /// </summary>
        /// <param name="context">數據庫上下文</param>
        public UserService(AuthDbContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// 註冊新用戶
        /// </summary>
        /// <param name="request">註冊請求</param>
        /// <returns>註冊成功的用戶</returns>
        public async Task<User> Register(RegisterRequest request)
        {
            // 檢查用戶名是否已存在
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new ApplicationException("用戶名已被使用");
            }
            
            // 檢查電子郵件是否已存在
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new ApplicationException("電子郵件已被使用");
            }
            
            // 生成密碼鹽值
            var salt = GenerateSalt();
            
            // 創建新用戶
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = HashPassword(request.Password, salt),
                Salt = salt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            // 將用戶添加到數據庫
            await _context.Users.AddAsync(user);
            
            // 獲取默認用戶角色
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (defaultRole == null)
            {
                // 如果默認角色不存在，則創建
                defaultRole = new Role
                {
                    Name = "User",
                    Description = "普通用戶",
                    IsSystem = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.Roles.AddAsync(defaultRole);
                await _context.SaveChangesAsync();
            }
            
            // 為用戶分配默認角色
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = defaultRole.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _context.UserRoles.AddAsync(userRole);
            
            // 保存更改
            await _context.SaveChangesAsync();
            
            return user;
        }
        
        /// <summary>
        /// 根據用戶名查詢用戶
        /// </summary>
        /// <param name="username">用戶名</param>
        /// <returns>用戶對象，如果不存在則返回null</returns>
        public async Task<User> GetByUsername(string username)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Username == username);
        }
        
        /// <summary>
        /// 根據ID查詢用戶
        /// </summary>
        /// <param name="id">用戶ID</param>
        /// <returns>用戶對象，如果不存在則返回null</returns>
        public async Task<User> GetById(string id)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        
        /// <summary>
        /// 根據電子郵件查詢用戶
        /// </summary>
        /// <param name="email">電子郵件</param>
        /// <returns>用戶對象，如果不存在則返回null</returns>
        public async Task<User> GetByEmail(string email)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        
        /// <summary>
        /// 驗證用戶密碼
        /// </summary>
        /// <param name="user">用戶對象</param>
        /// <param name="password">待驗證的密碼</param>
        /// <returns>密碼是否正確</returns>
        public async Task<bool> ValidatePassword(User user, string password)
        {
            // 使用相同的鹽值對輸入的密碼進行哈希
            var hashedPassword = HashPassword(password, user.Salt);
            
            // 比較哈希值是否匹配
            return hashedPassword == user.PasswordHash;
        }
        
        /// <summary>
        /// 獲取用戶的角色列表
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>角色名稱列表</returns>
        public async Task<List<string>> GetUserRoles(string userId)
        {
            // 查詢用戶的所有角色
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name)
                .ToListAsync();
            
            return roles;
        }
        
        /// <summary>
        /// 生成隨機鹽值
        /// </summary>
        /// <returns>鹽值字符串</returns>
        private string GenerateSalt()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
        
        /// <summary>
        /// 使用鹽值對密碼進行哈希
        /// </summary>
        /// <param name="password">原始密碼</param>
        /// <param name="salt">鹽值</param>
        /// <returns>哈希後的密碼</returns>
        private string HashPassword(string password, string salt)
        {
            // 將密碼和鹽值組合
            var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
            
            // 使用SHA256算法計算哈希值
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}