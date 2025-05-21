using AuthService.Models;
using AuthService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Services
{
    /// <summary>
    /// 權限服務實現
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        /// <summary>
        /// 建構函數
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="logger">日誌記錄器</param>
        public PermissionService(AuthDbContext context, ILogger<PermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<bool> UserHasPermission(string userId, string resource, string action)
        {
            try
            {
                // 獲取用戶的角色
                var userRoleIds = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                if (!userRoleIds.Any())
                {
                    _logger.LogInformation("用戶 {UserId} 沒有任何角色", userId);
                    return false;
                }

                // 獲取角色的權限
                var permissionIds = await _context.RolePermissions
                    .Where(rp => userRoleIds.Contains(rp.RoleId))
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                if (!permissionIds.Any())
                {
                    _logger.LogInformation("用戶 {UserId} 的角色沒有任何權限", userId);
                    return false;
                }

                // 檢查是否有指定的資源和操作權限
                var hasPermission = await _context.Permissions
                    .AnyAsync(p => permissionIds.Contains(p.Id) && p.Resource == resource && p.Action == action);

                _logger.LogInformation("用戶 {UserId} 對資源 {Resource} 進行 {Action} 操作的權限檢查結果: {HasPermission}",
                    userId, resource, action, hasPermission);

                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查用戶 {UserId} 權限時發生錯誤", userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UserHasPermissionByName(string userId, string permissionName)
        {
            try
            {
                // 獲取用戶的角色
                var userRoleIds = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                if (!userRoleIds.Any())
                {
                    return false;
                }

                // 獲取角色的權限
                var permissionIds = await _context.RolePermissions
                    .Where(rp => userRoleIds.Contains(rp.RoleId))
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                if (!permissionIds.Any())
                {
                    return false;
                }

                // 檢查是否有指定名稱的權限
                var hasPermission = await _context.Permissions
                    .AnyAsync(p => permissionIds.Contains(p.Id) && p.Name == permissionName);

                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據名稱檢查用戶 {UserId} 權限時發生錯誤", userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Permission>> GetUserPermissions(string userId)
        {
            try
            {
                // 獲取用戶的角色
                var userRoleIds = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                if (!userRoleIds.Any())
                {
                    return Enumerable.Empty<Permission>();
                }

                // 獲取角色的權限ID
                var permissionIds = await _context.RolePermissions
                    .Where(rp => userRoleIds.Contains(rp.RoleId))
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                if (!permissionIds.Any())
                {
                    return Enumerable.Empty<Permission>();
                }

                // 獲取權限詳情
                var permissions = await _context.Permissions
                    .Where(p => permissionIds.Contains(p.Id))
                    .ToListAsync();

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取用戶 {UserId} 權限時發生錯誤", userId);
                return Enumerable.Empty<Permission>();
            }
        }

        /// <inheritdoc />
        public async Task<bool> AssignPermissionToRole(string roleId, string permissionId)
        {
            try
            {
                // 檢查角色是否存在
                var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
                if (!roleExists)
                {
                    _logger.LogWarning("嘗試為不存在的角色 {RoleId} 分配權限", roleId);
                    return false;
                }

                // 檢查權限是否存在
                var permissionExists = await _context.Permissions.AnyAsync(p => p.Id == permissionId);
                if (!permissionExists)
                {
                    _logger.LogWarning("嘗試分配不存在的權限 {PermissionId}", permissionId);
                    return false;
                }

                // 檢查是否已經分配
                var exists = await _context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

                if (exists)
                {
                    _logger.LogInformation("角色 {RoleId} 已經擁有權限 {PermissionId}", roleId, permissionId);
                    return true;
                }

                // 添加角色權限關聯
                var rolePermission = new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.RolePermissions.AddAsync(rolePermission);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功為角色 {RoleId} 分配權限 {PermissionId}", roleId, permissionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "為角色 {RoleId} 分配權限 {PermissionId} 時發生錯誤", roleId, permissionId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> RemovePermissionFromRole(string roleId, string permissionId)
        {
            try
            {
                // 查找角色權限關聯
                var rolePermission = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

                if (rolePermission == null)
                {
                    _logger.LogWarning("嘗試移除不存在的角色權限關聯: 角色 {RoleId}, 權限 {PermissionId}", roleId, permissionId);
                    return false;
                }

                // 移除角色權限關聯
                _context.RolePermissions.Remove(rolePermission);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功從角色 {RoleId} 移除權限 {PermissionId}", roleId, permissionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "從角色 {RoleId} 移除權限 {PermissionId} 時發生錯誤", roleId, permissionId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<Permission> CreatePermission(Permission permission)
        {
            try
            {
                // 設置ID和時間戳
                if (string.IsNullOrEmpty(permission.Id))
                {
                    permission.Id = Guid.NewGuid().ToString();
                }

                permission.CreatedAt = DateTime.UtcNow;
                permission.UpdatedAt = DateTime.UtcNow;

                // 如果沒有設置Name，則使用Resource:Action格式
                if (string.IsNullOrEmpty(permission.Name) && !string.IsNullOrEmpty(permission.Resource) && !string.IsNullOrEmpty(permission.Action))
                {
                    permission.Name = $"{permission.Resource}:{permission.Action}";
                }

                await _context.Permissions.AddAsync(permission);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功創建權限 {PermissionName} (ID: {PermissionId})", permission.Name, permission.Id);
                return permission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建權限時發生錯誤");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Permission>> GetAllPermissions()
        {
            try
            {
                return await _context.Permissions.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取所有權限時發生錯誤");
                return Enumerable.Empty<Permission>();
            }
        }

        /// <inheritdoc />
        public async Task<Permission?> GetPermissionById(string id)
        {
            try
            {
                return await _context.Permissions.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據ID獲取權限時發生錯誤: {PermissionId}", id);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<Permission> UpdatePermission(string id, Permission permission)
        {
            try
            {
                var existingPermission = await _context.Permissions.FindAsync(id);
                if (existingPermission == null)
                {
                    throw new KeyNotFoundException($"找不到ID為 '{id}' 的權限");
                }

                // 更新權限屬性
                existingPermission.Name = permission.Name;
                existingPermission.Description = permission.Description;
                existingPermission.Resource = permission.Resource;
                existingPermission.Action = permission.Action;
                existingPermission.UpdatedAt = DateTime.UtcNow;

                _context.Permissions.Update(existingPermission);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功更新權限 {PermissionName} (ID: {PermissionId})", existingPermission.Name, existingPermission.Id);
                return existingPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新權限時發生錯誤: {PermissionId}", id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeletePermission(string id)
        {
            try
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null)
                {
                    _logger.LogWarning("嘗試刪除不存在的權限: {PermissionId}", id);
                    return false;
                }

                // 檢查是否有角色使用此權限
                var hasRoles = await _context.RolePermissions.AnyAsync(rp => rp.PermissionId == id);
                if (hasRoles)
                {
                    _logger.LogWarning("無法刪除已分配給角色的權限: {PermissionId}", id);
                    throw new InvalidOperationException("無法刪除已分配給角色的權限");
                }

                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功刪除權限 {PermissionName} (ID: {PermissionId})", permission.Name, permission.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除權限時發生錯誤: {PermissionId}", id);
                throw;
            }
        }
    }
}