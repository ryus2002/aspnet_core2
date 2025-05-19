using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Services
{
    /// <summary>
    /// 權限服務接口，定義權限相關的業務邏輯操作
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// 檢查用戶是否擁有指定權限
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="resource">資源名稱</param>
        /// <param name="action">操作類型</param>
        /// <returns>是否擁有權限</returns>
        Task<bool> UserHasPermission(string userId, string resource, string action);
        
        /// <summary>
        /// 檢查用戶是否擁有指定權限
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="permissionName">權限名稱</param>
        /// <returns>是否擁有權限</returns>
        Task<bool> UserHasPermissionByName(string userId, string permissionName);
        
        /// <summary>
        /// 獲取用戶的所有權限
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>權限列表</returns>
        Task<List<Permission>> GetUserPermissions(string userId);
        
        /// <summary>
        /// 為角色分配權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="permissionId">權限ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> AssignPermissionToRole(string roleId, string permissionId);
        
        /// <summary>
        /// 從角色移除權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="permissionId">權限ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> RemovePermissionFromRole(string roleId, string permissionId);
        
        /// <summary>
        /// 創建新權限
        /// </summary>
        /// <param name="permission">權限對象</param>
        /// <returns>創建的權限</returns>
        Task<Permission> CreatePermission(Permission permission);
        
        /// <summary>
        /// 獲取所有權限
        /// </summary>
        /// <returns>權限列表</returns>
        Task<List<Permission>> GetAllPermissions();
        
        /// <summary>
        /// 根據ID獲取權限
        /// </summary>
        /// <param name="id">權限ID</param>
        /// <returns>權限對象</returns>
        Task<Permission> GetPermissionById(string id);
        
        /// <summary>
        /// 根據名稱獲取權限
        /// </summary>
        /// <param name="name">權限名稱</param>
        /// <returns>權限對象</returns>
        Task<Permission> GetPermissionByName(string name);
        
        /// <summary>
        /// 更新權限
        /// </summary>
        /// <param name="permission">權限對象</param>
        /// <returns>更新後的權限</returns>
        Task<Permission> UpdatePermission(Permission permission);
        
        /// <summary>
        /// 刪除權限
        /// </summary>
        /// <param name="id">權限ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> DeletePermission(string id);
    }
    
    /// <summary>
    /// 權限服務實現類
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<PermissionService> _logger;
        
        /// <summary>
        /// 構造函數，注入數據庫上下文和日誌記錄器
        /// </summary>
        /// <param name="context">數據庫上下文</param>
        /// <param name="logger">日誌記錄器</param>
        public PermissionService(AuthDbContext context, ILogger<PermissionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// 檢查用戶是否擁有指定權限
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="resource">資源名稱</param>
        /// <param name="action">操作類型</param>
        /// <returns>是否擁有權限</returns>
        public async Task<bool> UserHasPermission(string userId, string resource, string action)
        {
            try
            {
                // 查詢用戶的所有角色
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();
                
                // 如果用戶沒有角色，則沒有權限
                if (userRoles.Count == 0)
                {
                    return false;
                }
                
                // 查詢這些角色是否擁有指定的權限
                var hasPermission = await _context.RolePermissions
                    .Where(rp => userRoles.Contains(rp.RoleId))
                    .Join(_context.Permissions,
                        rp => rp.PermissionId,
                        p => p.Id,
                        (rp, p) => p)
                    .AnyAsync(p => p.Resource == resource && p.Action == action);
                
                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查用戶權限時發生錯誤: {UserId}, {Resource}, {Action}", userId, resource, action);
                return false;
            }
        }
        
        /// <summary>
        /// 檢查用戶是否擁有指定權限
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="permissionName">權限名稱</param>
        /// <returns>是否擁有權限</returns>
        public async Task<bool> UserHasPermissionByName(string userId, string permissionName)
        {
            try
            {
                // 查詢用戶的所有角色
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();
                
                // 如果用戶沒有角色，則沒有權限
                if (userRoles.Count == 0)
                {
                    return false;
                }
                
                // 查詢這些角色是否擁有指定的權限
                var hasPermission = await _context.RolePermissions
                    .Where(rp => userRoles.Contains(rp.RoleId))
                    .Join(_context.Permissions,
                        rp => rp.PermissionId,
                        p => p.Id,
                        (rp, p) => p)
                    .AnyAsync(p => p.Name == permissionName);
                
                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查用戶權限時發生錯誤: {UserId}, {PermissionName}", userId, permissionName);
                return false;
            }
        }
        
        /// <summary>
        /// 獲取用戶的所有權限
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>權限列表</returns>
        public async Task<List<Permission>> GetUserPermissions(string userId)
        {
            try
            {
                // 查詢用戶的所有角色
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();
                
                // 查詢這些角色擁有的所有權限
                var permissions = await _context.RolePermissions
                    .Where(rp => userRoles.Contains(rp.RoleId))
                    .Join(_context.Permissions,
                        rp => rp.PermissionId,
                        p => p.Id,
                        (rp, p) => p)
                    .Distinct()
                    .ToListAsync();
                
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取用戶權限時發生錯誤: {UserId}", userId);
                return new List<Permission>();
            }
        }
        
        /// <summary>
        /// 為角色分配權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="permissionId">權限ID</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> AssignPermissionToRole(string roleId, string permissionId)
        {
            try
            {
                // 檢查角色是否存在
                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("嘗試為不存在的角色分配權限: {RoleId}", roleId);
                    return false;
                }
                
                // 檢查權限是否存在
                var permission = await _context.Permissions.FindAsync(permissionId);
                if (permission == null)
                {
                    _logger.LogWarning("嘗試分配不存在的權限: {PermissionId}", permissionId);
                    return false;
                }
                
                // 檢查是否已經分配了該權限
                var existingRolePermission = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
                
                if (existingRolePermission != null)
                {
                    _logger.LogInformation("角色已經擁有該權限: {RoleId}, {PermissionId}", roleId, permissionId);
                    return true;
                }
                
                // 創建角色權限關聯
                var rolePermission = new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _context.RolePermissions.AddAsync(rolePermission);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功為角色分配權限: {RoleId}, {PermissionId}", roleId, permissionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "為角色分配權限時發生錯誤: {RoleId}, {PermissionId}", roleId, permissionId);
                return false;
            }
        }
        
        /// <summary>
        /// 從角色移除權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="permissionId">權限ID</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> RemovePermissionFromRole(string roleId, string permissionId)
        {
            try
            {
                // 查找角色權限關聯
                var rolePermission = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
                
                if (rolePermission == null)
                {
                    _logger.LogWarning("嘗試移除不存在的角色權限關聯: {RoleId}, {PermissionId}", roleId, permissionId);
                    return false;
                }
                
                // 移除角色權限關聯
                _context.RolePermissions.Remove(rolePermission);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功從角色移除權限: {RoleId}, {PermissionId}", roleId, permissionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "從角色移除權限時發生錯誤: {RoleId}, {PermissionId}", roleId, permissionId);
                return false;
            }
        }
        
        /// <summary>
        /// 創建新權限
        /// </summary>
        /// <param name="permission">權限對象</param>
        /// <returns>創建的權限</returns>
        public async Task<Permission> CreatePermission(Permission permission)
        {
            try
            {
                // 檢查權限名稱是否已存在
                var existingPermission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Name == permission.Name);
                
                if (existingPermission != null)
                {
                    _logger.LogWarning("嘗試創建已存在的權限: {PermissionName}", permission.Name);
                    throw new ApplicationException($"權限名稱 '{permission.Name}' 已存在");
                }
                
                // 設置創建時間和更新時間
                permission.CreatedAt = DateTime.UtcNow;
                permission.UpdatedAt = DateTime.UtcNow;
                
                // 添加權限
                await _context.Permissions.AddAsync(permission);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功創建權限: {PermissionName}", permission.Name);
                return permission;
            }
            catch (ApplicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建權限時發生錯誤: {PermissionName}", permission.Name);
                throw new ApplicationException("創建權限時發生錯誤", ex);
            }
        }
        
        /// <summary>
        /// 獲取所有權限
        /// </summary>
        /// <returns>權限列表</returns>
        public async Task<List<Permission>> GetAllPermissions()
        {
            try
            {
                return await _context.Permissions.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取所有權限時發生錯誤");
                return new List<Permission>();
            }
        }
        
        /// <summary>
        /// 根據ID獲取權限
        /// </summary>
        /// <param name="id">權限ID</param>
        /// <returns>權限對象</returns>
        public async Task<Permission> GetPermissionById(string id)
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
        
        /// <summary>
        /// 根據名稱獲取權限
        /// </summary>
        /// <param name="name">權限名稱</param>
        /// <returns>權限對象</returns>
        public async Task<Permission> GetPermissionByName(string name)
        {
            try
            {
                return await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Name == name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據名稱獲取權限時發生錯誤: {PermissionName}", name);
                return null;
            }
        }
        
        /// <summary>
        /// 更新權限
        /// </summary>
        /// <param name="permission">權限對象</param>
        /// <returns>更新後的權限</returns>
        public async Task<Permission> UpdatePermission(Permission permission)
        {
            try
            {
                // 檢查權限是否存在
                var existingPermission = await _context.Permissions.FindAsync(permission.Id);
                if (existingPermission == null)
                {
                    _logger.LogWarning("嘗試更新不存在的權限: {PermissionId}", permission.Id);
                    throw new ApplicationException($"權限ID '{permission.Id}' 不存在");
                }
                
                // 檢查權限名稱是否已被其他權限使用
                var nameConflict = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Name == permission.Name && p.Id != permission.Id);
                
                if (nameConflict != null)
                {
                    _logger.LogWarning("嘗試將權限名稱更新為已存在的名稱: {PermissionName}", permission.Name);
                    throw new ApplicationException($"權限名稱 '{permission.Name}' 已被其他權限使用");
                }
                
                // 更新權限屬性
                existingPermission.Name = permission.Name;
                existingPermission.Description = permission.Description;
                existingPermission.Resource = permission.Resource;
                existingPermission.Action = permission.Action;
                existingPermission.UpdatedAt = DateTime.UtcNow;
                
                // 保存更改
                _context.Permissions.Update(existingPermission);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功更新權限: {PermissionId}", permission.Id);
                return existingPermission;
            }
            catch (ApplicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新權限時發生錯誤: {PermissionId}", permission.Id);
                throw new ApplicationException("更新權限時發生錯誤", ex);
            }
        }
        
        /// <summary>
        /// 刪除權限
        /// </summary>
        /// <param name="id">權限ID</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> DeletePermission(string id)
        {
            try
            {
                // 檢查權限是否存在
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null)
                {
                    _logger.LogWarning("嘗試刪除不存在的權限: {PermissionId}", id);
                    return false;
                }
                
                // 刪除與該權限相關的所有角色權限關聯
                var rolePermissions = await _context.RolePermissions
                    .Where(rp => rp.PermissionId == id)
                    .ToListAsync();
                
                _context.RolePermissions.RemoveRange(rolePermissions);
                
                // 刪除權限
                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功刪除權限: {PermissionId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除權限時發生錯誤: {PermissionId}", id);
                return false;
            }
        }
    }
}