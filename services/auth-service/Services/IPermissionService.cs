using AuthService.Models;

namespace AuthService.Services
{
    /// <summary>
    /// 權限服務介面
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// 檢查用戶是否擁有指定的權限
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="resource">資源名稱</param>
        /// <param name="action">操作名稱</param>
        /// <returns>是否有權限</returns>
        Task<bool> UserHasPermission(string userId, string resource, string action);
        
        /// <summary>
        /// 根據權限名稱檢查用戶是否擁有指定的權限
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="permissionName">權限名稱</param>
        /// <returns>是否有權限</returns>
        Task<bool> UserHasPermissionByName(string userId, string permissionName);
        
        /// <summary>
        /// 獲取用戶的所有權限
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>權限列表</returns>
        Task<IEnumerable<Permission>> GetUserPermissions(string userId);
        
        /// <summary>
        /// 為角色分配權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="permissionId">權限ID</param>
        /// <returns>是否成功</returns>
        Task<bool> AssignPermissionToRole(string roleId, string permissionId);
        
        /// <summary>
        /// 從角色中移除權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="permissionId">權限ID</param>
        /// <returns>是否成功</returns>
        Task<bool> RemovePermissionFromRole(string roleId, string permissionId);
        
        /// <summary>
        /// 創建新權限
        /// </summary>
        /// <param name="permission">權限實體</param>
        /// <returns>創建的權限</returns>
        Task<Permission> CreatePermission(Permission permission);

        /// <summary>
        /// 獲取所有權限
        /// </summary>
        /// <returns>權限列表</returns>
        Task<IEnumerable<Permission>> GetAllPermissions();

        /// <summary>
        /// 根據ID獲取權限
        /// </summary>
        /// <param name="id">權限ID</param>
        /// <returns>權限，如不存在則返回null</returns>
        Task<Permission?> GetPermissionById(string id);

        /// <summary>
        /// 更新權限
        /// </summary>
        /// <param name="id">權限ID</param>
        /// <param name="permission">更新後的權限資訊</param>
        /// <returns>更新後的權限</returns>
        Task<Permission> UpdatePermission(string id, Permission permission);

        /// <summary>
        /// 刪除權限
        /// </summary>
        /// <param name="id">權限ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeletePermission(string id);
    }
}