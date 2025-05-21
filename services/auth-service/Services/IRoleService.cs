using AuthService.Models;

namespace AuthService.Services
{
    /// <summary>
    /// 角色服務介面
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// 創建新角色
        /// </summary>
        /// <param name="name">角色名稱</param>
        /// <param name="description">角色描述</param>
        /// <returns>創建的角色</returns>
        Task<Role> CreateRoleAsync(string name, string description);
        
        /// <summary>
        /// 獲取所有角色
        /// </summary>
        /// <returns>角色列表</returns>
        Task<IEnumerable<Role>> GetAllRolesAsync();
        
        /// <summary>
        /// 根據ID獲取角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色，如不存在則返回null</returns>
        Task<Role?> GetRoleByIdAsync(string id);
        
        /// <summary>
        /// 根據名稱獲取角色
        /// </summary>
        /// <param name="name">角色名稱</param>
        /// <returns>角色，如不存在則返回null</returns>
        Task<Role?> GetRoleByNameAsync(string name);
        
        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="name">新角色名稱</param>
        /// <param name="description">新角色描述</param>
        /// <returns>更新後的角色</returns>
        Task<Role> UpdateRoleAsync(string id, string name, string description);
        
        /// <summary>
        /// 刪除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteRoleAsync(string id);
        
        /// <summary>
        /// 為用戶分配角色
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>是否成功</returns>
        Task<bool> AssignRoleToUserAsync(string userId, string roleId);
        
        /// <summary>
        /// 移除用戶的角色
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>是否成功</returns>
        Task<bool> RemoveRoleFromUserAsync(string userId, string roleId);

        // 以下是控制器中使用的方法，需要添加到介面中

        /// <summary>
        /// 獲取所有角色
        /// </summary>
        /// <returns>角色列表</returns>
        Task<IEnumerable<Role>> GetAllRoles();

        /// <summary>
        /// 根據ID獲取角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色，如不存在則返回null</returns>
        Task<Role?> GetRoleById(string id);

        /// <summary>
        /// 創建新角色
        /// </summary>
        /// <param name="role">角色資訊</param>
        /// <returns>創建的角色</returns>
        Task<Role> CreateRole(Role role);

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="role">更新後的角色資訊</param>
        /// <returns>更新後的角色</returns>
        Task<Role> UpdateRole(string id, Role role);

        /// <summary>
        /// 刪除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteRole(string id);

        /// <summary>
        /// 為用戶分配角色
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>是否成功</returns>
        Task<bool> AssignRoleToUser(string userId, string roleId);

        /// <summary>
        /// 移除用戶的角色
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>是否成功</returns>
        Task<bool> RemoveRoleFromUser(string userId, string roleId);

        /// <summary>
        /// 獲取角色的權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>權限列表</returns>
        Task<IEnumerable<Permission>> GetRolePermissions(string roleId);

        /// <summary>
        /// 獲取用戶的角色
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>角色列表</returns>
        Task<IEnumerable<Role>> GetUserRoles(string userId);
    }
}