using System;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    /// <summary>
    /// 刷新令牌模型，用於JWT認證系統中刷新訪問令牌
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// 刷新令牌唯一標識符
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 所屬用戶ID
        /// </summary>
        public required string UserId { get; set; }
        
        /// <summary>
        /// 刷新令牌值
        /// </summary>
        [Required]
        public required string Token { get; set; }
        
        /// <summary>
        /// 令牌過期時間
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// 令牌創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 令牌創建IP地址
        /// </summary>
        public required string CreatedByIp { get; set; }
        
        /// <summary>
        /// 令牌是否已被撤銷
        /// </summary>
        public bool IsRevoked { get; set; } = false;
        
        /// <summary>
        /// 令牌撤銷時間
        /// </summary>
        public DateTime? RevokedAt { get; set; }
        
        /// <summary>
        /// 令牌撤銷IP地址
        /// </summary>
        public string RevokedByIp { get; set; } = string.Empty;
        
        /// <summary>
        /// 替換此令牌的新令牌
        /// </summary>
        public string ReplacedByToken { get; set; } = string.Empty;
        
        /// <summary>
        /// 令牌是否處於活躍狀態
        /// </summary>
        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
        
        /// <summary>
        /// 用戶導航屬性
        /// </summary>
        public virtual User User { get; set; } = null!;
    }
}