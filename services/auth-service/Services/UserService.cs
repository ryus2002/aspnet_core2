using AuthService.DTOs;
using AuthService.Models;
using AuthService.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Services
{
    /// <summary>
    /// 用戶服務實現
    /// </summary>
    public class UserService : IUserService
    {
        private readonly AuthDbContext _context;

        /// <summary>
        /// 建構函數
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public UserService(AuthDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<User?> GetById(string id)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <inheritdoc />
        public async Task<User?> GetByUsername(string username)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        /// <inheritdoc />
        public async Task<User?> GetByEmail(string email)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByRefreshToken(string refreshToken)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken && t.ExpiresAt > DateTime.UtcNow));
        }

        /// <inheritdoc />
        public async Task<User> Register(RegisterRequest request)
        {
            // 檢查用戶名是否已存在
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new ApplicationException($"用戶名 '{request.Username}' 已被使用");
            }

            // 檢查電子郵件是否已存在
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new ApplicationException($"電子郵件 '{request.Email}' 已被使用");
            }

            // 生成鹽值
            var salt = GenerateSalt();
            // 創建密碼哈希
            var passwordHash = HashPassword(request.Password, salt);

            // 創建新用戶
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = request.Username,
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = passwordHash,
                Salt = salt,
                LastLoginIp = "127.0.0.1", // 添加必填屬性的預設值
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RefreshTokens = new List<RefreshToken>()
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // 分配預設角色
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User" && r.IsSystem);
            if (defaultRole != null)
            {
                // 修改 UserRole 的初始化代碼
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = defaultRole.Id,
                    CreatedAt = DateTime.UtcNow,
                    // 添加必要的導航屬性
                    User = user,
                    Role = defaultRole
                };

                await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }

            return user;
        }
        /// <inheritdoc />
        public async Task<bool> ValidatePassword(User user, string password)
        {
            var hash = HashPassword(password, user.Salt);
            // 添加 await 以避免警告
            await Task.CompletedTask;
            return hash == user.PasswordHash;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetUserRoles(string userId)
        {
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (!roleIds.Any())
        {
                return Enumerable.Empty<string>();
            }

            var roleNames = await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync();

            return roleNames;
        }

        /// <inheritdoc />
        public async Task<User> UpdateUserAsync(User user)
            {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
            }

        /// <summary>
        /// 生成隨機鹽值
        /// </summary>
        /// <returns>Base64編碼的鹽值</returns>
        private string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
    }
            return Convert.ToBase64String(saltBytes);
}

        /// <summary>
        /// 使用鹽值哈希密碼
        /// </summary>
        /// <param name="password">密碼</param>
        /// <param name="salt">鹽值</param>
        /// <returns>Base64編碼的哈希值</returns>
        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password + salt);
                var hashBytes = sha256.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}