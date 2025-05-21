using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AuthService.Attributes
{
    /// <summary>
    /// 要求特定權限的屬性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// 權限名稱
        /// </summary>
        public string PermissionName { get; }
        
        /// <summary>
        /// 資源名稱
        /// </summary>
        public string Resource { get; }
        
        /// <summary>
        /// 操作名稱
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// 建構函數
        /// </summary>
        /// <param name="resource">資源名稱</param>
        /// <param name="action">操作名稱</param>
        public RequirePermissionAttribute(string resource, string action)
            : base(typeof(RequirePermissionFilter))
        {
            Resource = resource;
            Action = action;
            PermissionName = $"{resource}:{action}";
            Arguments = new object[] { resource, action, PermissionName, false };
        }

        /// <summary>
        /// 建構函數
        /// </summary>
        /// <param name="permissionName">權限名稱（格式：resource:action）</param>
        public RequirePermissionAttribute(string permissionName)
            : base(typeof(RequirePermissionFilter))
        {
            PermissionName = permissionName;
            
            // 嘗試解析資源和操作
            var parts = permissionName.Split(':');
            if (parts.Length == 2)
            {
                Resource = parts[0];
                Action = parts[1];
            }
            else
            {
                Resource = string.Empty;
                Action = string.Empty;
            }
            
            Arguments = new object[] { Resource, Action, permissionName, true };
        }
    }

    /// <summary>
    /// 權限過濾器
    /// </summary>
    public class RequirePermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string _resource;
        private readonly string _action;
        private readonly string _permissionName;
        private readonly bool _usePermissionName;

        /// <summary>
        /// 建構函數
        /// </summary>
        /// <param name="resource">資源</param>
        /// <param name="action">操作</param>
        /// <param name="permissionName">權限名稱</param>
        /// <param name="usePermissionName">是否使用權限名稱</param>
        public RequirePermissionFilter(string resource, string action, string permissionName, bool usePermissionName)
        {
            _resource = resource;
            _action = action;
            _permissionName = permissionName;
            _usePermissionName = usePermissionName;
        }

        /// <summary>
        /// 授權處理
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns>任務</returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // 權限檢查邏輯將在中間件中實現
            // 這裡僅用於標記需要檢查的權限
            await Task.CompletedTask;
        }
    }
}