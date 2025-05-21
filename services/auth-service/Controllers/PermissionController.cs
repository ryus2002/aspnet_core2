using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthService.Attributes;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthService.Controllers
{
    /// <summary>
    /// 權限控制器，處理權限相關操作
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionController> _logger;
        
        /// <summary>
        /// 構造函數，注入所需服務
        /// </summary>
        /// <param name="permissionService">權限服務</param>
        /// <param name="logger">日誌記錄器</param>
        public PermissionController(
            IPermissionService permissionService,
            ILogger<PermissionController> logger)
        {
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// 獲取所有權限
        /// </summary>
        /// <returns>權限列表</returns>
        [HttpGet]
        [RequirePermission("Permission", "Read")]
        [ProducesResponseType(typeof(List<Permission>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllPermissions()
        {
            try
            {
                _logger.LogInformation("獲取所有權限");
                
                var permissions = await _permissionService.GetAllPermissions();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取所有權限時發生錯誤");
                return StatusCode(StatusCodes.Status500InternalServerError, "獲取權限時發生錯誤");
            }
        }
        
        /// <summary>
        /// 根據ID獲取權限
        /// </summary>
        /// <param name="id">權限ID</param>
        /// <returns>權限信息</returns>
        [HttpGet("{id}")]
        [RequirePermission("Permission", "Read")]
        [ProducesResponseType(typeof(Permission), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPermissionById(string id)
        {
            try
            {
                _logger.LogInformation("獲取權限: {PermissionId}", id);
                
                var permission = await _permissionService.GetPermissionById(id);
                if (permission == null)
                {
                    return NotFound($"權限ID '{id}' 不存在");
                }
                
                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取權限時發生錯誤: {PermissionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "獲取權限時發生錯誤");
            }
        }
        
        /// <summary>
        /// 創建新權限
        /// </summary>
        /// <param name="permission">權限對象</param>
        /// <returns>創建的權限</returns>
        [HttpPost]
        [RequirePermission("Permission", "Create")]
        [ProducesResponseType(typeof(Permission), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreatePermission([FromBody] Permission permission)
        {
            try
            {
                _logger.LogInformation("創建權限: {PermissionName}", permission.Name);
                
                var createdPermission = await _permissionService.CreatePermission(permission);
                return CreatedAtAction(nameof(GetPermissionById), new { id = createdPermission.Id }, createdPermission);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("創建權限失敗: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建權限時發生錯誤: {PermissionName}", permission.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, "創建權限時發生錯誤");
            }
        }
        
        /// <summary>
        /// 更新權限
        /// </summary>
        /// <param name="id">權限ID</param>
        /// <param name="permission">權限對象</param>
        /// <returns>更新後的權限</returns>
        [HttpPut("{id}")]
        [RequirePermission("Permission", "Update")]
        [ProducesResponseType(typeof(Permission), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePermission(string id, [FromBody] Permission permission)
        {
            try
            {
                _logger.LogInformation("更新權限: {PermissionId}", id);
                
                // 確保ID匹配
                if (id != permission.Id)
                {
                    return BadRequest("請求路徑中的ID與權限對象中的ID不匹配");
                }
                
                var updatedPermission = await _permissionService.UpdatePermission(id, permission);
                return Ok(updatedPermission);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("更新權限失敗: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新權限時發生錯誤: {PermissionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "更新權限時發生錯誤");
            }
        }
        
        /// <summary>
        /// 刪除權限
        /// </summary>
        /// <param name="id">權限ID</param>
        /// <returns>操作結果</returns>
        [HttpDelete("{id}")]
        [RequirePermission("Permission", "Delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePermission(string id)
        {
            try
            {
                _logger.LogInformation("刪除權限: {PermissionId}", id);
                
                var result = await _permissionService.DeletePermission(id);
                if (!result)
                {
                    return NotFound($"權限ID '{id}' 不存在");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除權限時發生錯誤: {PermissionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "刪除權限時發生錯誤");
            }
        }
        
        /// <summary>
        /// 為角色分配權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="permissionId">權限ID</param>
        /// <returns>操作結果</returns>
        [HttpPost("roles/{roleId}/permissions/{permissionId}")]
        [RequirePermission("Permission", "Assign")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignPermissionToRole(string roleId, string permissionId)
        {
            try
            {
                _logger.LogInformation("為角色分配權限: {RoleId}, {PermissionId}", roleId, permissionId);
                
                var result = await _permissionService.AssignPermissionToRole(roleId, permissionId);
                if (!result)
                {
                    return NotFound("角色或權限不存在");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "為角色分配權限時發生錯誤: {RoleId}, {PermissionId}", roleId, permissionId);
                return StatusCode(StatusCodes.Status500InternalServerError, "為角色分配權限時發生錯誤");
            }
        }
        
        /// <summary>
        /// 從角色移除權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="permissionId">權限ID</param>
        /// <returns>操作結果</returns>
        [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
        [RequirePermission("Permission", "Remove")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemovePermissionFromRole(string roleId, string permissionId)
        {
            try
            {
                _logger.LogInformation("從角色移除權限: {RoleId}, {PermissionId}", roleId, permissionId);
                
                var result = await _permissionService.RemovePermissionFromRole(roleId, permissionId);
                if (!result)
                {
                    return NotFound("角色權限關聯不存在");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "從角色移除權限時發生錯誤: {RoleId}, {PermissionId}", roleId, permissionId);
                return StatusCode(StatusCodes.Status500InternalServerError, "從角色移除權限時發生錯誤");
            }
        }
        
        /// <summary>
        /// 獲取用戶的所有權限
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>權限列表</returns>
        [HttpGet("users/{userId}/permissions")]
        [RequirePermission("Permission", "Read")]
        [ProducesResponseType(typeof(List<Permission>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserPermissions(string userId)
        {
            try
            {
                _logger.LogInformation("獲取用戶權限: {UserId}", userId);
                
                var permissions = await _permissionService.GetUserPermissions(userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取用戶權限時發生錯誤: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "獲取用戶權限時發生錯誤");
            }
        }
    }
}