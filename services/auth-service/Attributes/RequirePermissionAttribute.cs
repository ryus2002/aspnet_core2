using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AuthService.Attributes
{
    /// <summary>
    /// 權限要求屬性，用於標記需要特定權限的控制器或方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute
    {
        /// <summary>
        /// 資源名稱
        /// </summary>
        public string Resource { get; }
        
        /// <summary>
        /// 操作類型
        /// </summary>
        public string Action { get; }
        
        /// <summary>
        /// 權限名稱
        /// </summary>
        public string PermissionName { get; }
        
        /// <summary>
        /// 構造函數，使用資源和操作定義權限
        /// </summary>
        /// <param name="resource">資源名稱</param>
        /// <param name="action">操作類型</param>
        public RequirePermissionAttribute(string resource, string action)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            PermissionName = null;
        }
        
        /// <summary>
        /// 構造函數，使用權限名稱定義權限
        /// </summary>
        /// <param name="permissionName">權限名稱</param>
        public RequirePermissionAttribute(string permissionName)
        {
            PermissionName = permissionName ?? throw new ArgumentNullException(nameof(permissionName));
            Resource = null;
            Action = null;
        }
    }
}