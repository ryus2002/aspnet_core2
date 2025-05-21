using System;

namespace AuthService.Models
{
    /// <summary>
    /// 角色與權限的多對多關聯模型
    /// </summary>
    public class RolePermission
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public required string RoleId { get; set; }
        
        /// <summary>
        /// 權限ID
        /// </summary>
        public required string PermissionId { get; set; }
        
        /// <summary>
        /// 關聯創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 角色導航屬性
        /// </summary>
        public virtual Role Role { get; set; } = null!;
        
        /// <summary>
        /// 權限導航屬性
        /// </summary>
        public virtual Permission Permission { get; set; } = null!;
    }
}