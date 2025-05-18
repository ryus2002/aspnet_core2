using System;

namespace AuthService.Models
{
    /// <summary>
    /// 用戶與角色的多對多關聯模型
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// 角色ID
        /// </summary>
        public string RoleId { get; set; }
        
        /// <summary>
        /// 關聯創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 用戶導航屬性
        /// </summary>
        public virtual User User { get; set; }
        
        /// <summary>
        /// 角色導航屬性
        /// </summary>
        public virtual Role Role { get; set; }
    }
}