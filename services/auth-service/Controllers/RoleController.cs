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
    /// 角色控制器，處理角色相關操作
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;
        
        /// <summary>
        /// 構造函數，注入所需服務
        /// </summary>
        /// <param name="roleService">角色服務</param>
        /// <param name="logger">日誌記錄器</param>
        public RoleController(
            IRoleService roleService,
            ILogger<RoleController> logger)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// 獲取所有角色
        /// </summary>
        /// <returns>角色列表</returns>
        [HttpGet]
        [RequirePermission("Role", "Read")]
        [ProducesResponseType(typeof(List<Role>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                _logger.LogInformation("獲取所有角色");
                
                var roles = await _roleService.GetAllRoles();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取所有角色時發生錯誤");
                return StatusCode(StatusCodes.Status500InternalServerError, "獲取角色時發生錯誤");
            }
        }
        
        /// <summary>
        /// 根據ID獲取角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色信息</returns>
        [HttpGet("{id}")]
        [RequirePermission("Role", "Read")]
        [ProducesResponseType(typeof(Role), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleById(string id)
        {
            try
            {
                _logger.LogInformation("獲取角色: {RoleId}", id);
                
                var role = await _roleService.GetRoleById(id);
                if (role == null)
                {
                    return NotFound($"角色ID '{id}' 不存在");
                }
                
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取角色時發生錯誤: {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "獲取角色時發生錯誤");
            }
        }
        
        /// <summary>
        /// 創建新角色
        /// </summary>
        /// <param name="role">角色對象</param>
        /// <returns>創建的角色</returns>
        [HttpPost]
        [RequirePermission("Role", "Create")]
        [ProducesResponseType(typeof(Role), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateRole([FromBody] Role role)
        {
            try
            {
                _logger.LogInformation("創建角色: {RoleName}", role.Name);
                
                var createdRole = await _roleService.CreateRole(role);
                return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.Id }, createdRole);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("創建角色失敗: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建角色時發生錯誤: {RoleName}", role.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, "創建角色時發生錯誤");
            }
        }
        
        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="role">角色對象</param>
        /// <returns>更新後的角色</returns>
        [HttpPut("{id}")]
        [RequirePermission("Role", "Update")]
        [ProducesResponseType(typeof(Role), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] Role role)
        {
            try
            {
                _logger.LogInformation("更新角色: {RoleId}", id);
                
                // 確保ID匹配
                if (id != role.Id)
                {
                    return BadRequest("請求路徑中的ID與角色對象中的ID不匹配");
                }
                
                var updatedRole = await _roleService.UpdateRole(role);
                return Ok(updatedRole);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("更新角色失敗: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新角色時發生錯誤: {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "更新角色時發生錯誤");
            }
        }
        
        /// <summary>
        /// 刪除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>操作結果</returns>
        [HttpDelete("{id}")]
        [RequirePermission("Role", "Delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            try
            {
                _logger.LogInformation("刪除角色: {RoleId}", id);
                
                var result = await _roleService.DeleteRole(id);
                if (!result)
                {
                    return NotFound($"角色ID '{id}' 不存在");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除角色時發生錯誤: {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "刪除角色時發生錯誤");
            }
        }
        
        /// <summary>
        /// 為用戶分配角色
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>操作結果</returns>
        [HttpPost("users/{userId}/roles/{roleId}")]
        [RequirePermission("Role", "Assign")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRoleToUser(string userId, string roleId)
        {
            try
            {
                _logger.LogInformation("為用戶分配角色: {UserId}, {RoleId}", userId, roleId);
                
                var result = await _roleService.AssignRoleToUser(userId, roleId);
                if (!result)
                {
                    return NotFound("用戶或角色不存在");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "為用戶分配角色時發生錯誤: {UserId}, {RoleId}", userId, roleId);
                return StatusCode(StatusCodes.Status500InternalServerError, "為用戶分配角色時發生錯誤");
            }
        }
        
        /// <summary>
        /// 從用戶移除角色
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>操作結果</returns>
        [HttpDelete("users/{userId}/roles/{roleId}")]
        [RequirePermission("Role", "Remove")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleId)
        {
            try
            {
                _logger.LogInformation("從用戶移除角色: {UserId}, {RoleId}", userId, roleId);
                
                var result = await _roleService.RemoveRoleFromUser(userId, roleId);
                if (!result)
                {
                    return NotFound("用戶角色關聯不存在");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "從用戶移除角色時發生錯誤: {UserId}, {RoleId}", userId, roleId);
                return StatusCode(StatusCodes.Status500InternalServerError, "從用戶移除角色時發生錯誤");
            }
        }
        
        /// <summary>
        /// 獲取角色的所有權限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>權限列表</returns>
        [HttpGet("{roleId}/permissions")]
        [RequirePermission("Role", "Read")]
        [ProducesResponseType(typeof(List<Permission>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRolePermissions(string roleId)
        {
            try
            {
                _logger.LogInformation("獲取角色權限: {RoleId}", roleId);
                
                var role = await _roleService.GetRoleById(roleId);
                if (role == null)
                {
                    return NotFound($"角色ID '{roleId}' 不存在");
                }
                
                var permissions = await _roleService.GetRolePermissions(roleId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取角色權限時發生錯誤: {RoleId}", roleId);
                return StatusCode(StatusCodes.Status500InternalServerError, "獲取角色權限時發生錯誤");
            }
        }
        
        /// <summary>
        /// 獲取用戶的所有角色
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>角色列表</returns>
        [HttpGet("users/{userId}/roles")]
        [RequirePermission("Role", "Read")]
        [ProducesResponseType(typeof(List<Role>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            try
            {
                _logger.LogInformation("獲取用戶角色: {UserId}", userId);
                
                var roles = await _roleService.GetUserRoles(userId);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取用戶角色時發生錯誤: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "獲取用戶角色時發生錯誤");
            }
        }
    }
}