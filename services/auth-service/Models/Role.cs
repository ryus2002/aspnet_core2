using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    /// <summary>
    /// 角色模型類，定義系統中的用戶角色
    /// </summary>
    public class Role
    {
        /// <summary>
        /// 角色唯一標識符
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 角色名稱，如 "Admin", "User" 等
        /// </summary>
        [Required]
        [StringLength(50)]
        public required string Name { get; set; }
        
        /// <summary>
        /// 角色描述
        /// </summary>
        [StringLength(200)]
        public required string Description { get; set; }
        
        /// <summary>
        /// 是否為系統預設角色
        /// </summary>
        public bool IsSystem { get; set; } = false;
        
        /// <summary>
        /// 角色創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 角色更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 角色與用戶的關聯集合
        /// </summary>
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        
        /// <summary>
        /// 角色與權限的關聯集合
        /// </summary>
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}