using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    /// <summary>
    /// 權限模型類，定義系統中的操作權限
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// 權限唯一標識符
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 權限名稱，如 "CreateUser", "ReadProduct" 等
        /// </summary>
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }
        
        /// <summary>
        /// 權限描述
        /// </summary>
        [StringLength(255)]
        public required string Description { get; set; }
        
        /// <summary>
        /// 資源名稱，如 "User", "Product" 等
        /// </summary>
        [Required]
        [StringLength(100)]
        public required string Resource { get; set; }
        
        /// <summary>
        /// 操作類型，如 "Create", "Read", "Update", "Delete" 等
        /// </summary>
        [Required]
        [StringLength(50)]
        public required string Action { get; set; }
        
        /// <summary>
        /// 權限創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 權限更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 權限與角色的關聯集合
        /// </summary>
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}