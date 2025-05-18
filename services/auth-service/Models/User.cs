using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    /// <summary>
    /// 用戶模型類，存儲用戶的基本信息
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用戶唯一標識符
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 用戶名，用於登入
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        
        /// <summary>
        /// 用戶電子郵件地址，用於通知和密碼重置
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        /// <summary>
        /// 用戶密碼的雜湊值，不存儲明文密碼
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }
        
        /// <summary>
        /// 密碼加密的鹽值
        /// </summary>
        public string Salt { get; set; }
        
        /// <summary>
        /// 用戶的全名
        /// </summary>
        [StringLength(100)]
        public string FullName { get; set; }
        
        /// <summary>
        /// 用戶是否已激活賬戶
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// 用戶電子郵件是否已驗證
        /// </summary>
        public bool EmailVerified { get; set; } = false;
        
        /// <summary>
        /// 用戶創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 用戶最後更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 用戶最後登入時間
        /// </summary>
        public DateTime? LastLogin { get; set; }
        
        /// <summary>
        /// 用戶最後登入IP
        /// </summary>
        public string LastLoginIp { get; set; }
        
        /// <summary>
        /// 用戶刷新令牌集合
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        
        /// <summary>
        /// 用戶角色關聯集合
        /// </summary>
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}