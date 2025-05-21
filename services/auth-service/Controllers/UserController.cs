using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthService.Attributes;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthService.Controllers
{
    /// <summary>
    /// 用戶控制器，處理用戶相關操作
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<UserController> _logger;
        
        /// <summary>
        /// 構造函數，注入所需服務
        /// </summary>
        /// <param name="userService">用戶服務</param>
        /// <param name="permissionService">權限服務</param>
        /// <param name="logger">日誌記錄器</param>
        public UserController(
            IUserService userService,
            IPermissionService permissionService,
            ILogger<UserController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// 獲取所有用戶
        /// </summary>
        /// <returns>用戶列表</returns>
        [HttpGet]
        [RequirePermission("User", "Read")]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                _logger.LogInformation("獲取所有用戶");
                
                // 添加 await 以避免警告
                await Task.CompletedTask;
                
                // 這裡實現獲取所有用戶的邏輯
                // 暫時返回空列表
                return Ok(new List<UserDto>());
                }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取所有用戶時發生錯誤");
                return StatusCode(StatusCodes.Status500InternalServerError, "獲取用戶時發生錯誤");
            }
        }
        
        /// <summary>
        /// 根據ID獲取用戶
        /// </summary>
        /// <param name="id">用戶ID</param>
        /// <returns>用戶信息</returns>
        [HttpGet("{id}")]
        [RequirePermission("User", "Read")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                _logger.LogInformation("獲取用戶: {UserId}", id);
                
                var user = await _userService.GetById(id);
                if (user == null)
                {
                    return NotFound($"用戶ID '{id}' 不存在");
                }
                
                // 將用戶對象轉換為DTO
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    IsActive = user.IsActive,
                    EmailVerified = user.EmailVerified,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    LastLogin = user.LastLogin
                };
                
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取用戶時發生錯誤: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "獲取用戶時發生錯誤");
            }
        }
        
        /// <summary>
        /// 更新用戶信息
        /// </summary>
        /// <param name="id">用戶ID</param>
        /// <param name="updateRequest">更新請求</param>
        /// <returns>更新後的用戶信息</returns>
        [HttpPut("{id}")]
        [RequirePermission("User", "Update")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest updateRequest)
        {
            try
            {
                _logger.LogInformation("更新用戶: {UserId}", id);
                
                var user = await _userService.GetById(id);
                if (user == null)
                {
                    return NotFound($"用戶ID '{id}' 不存在");
            }
                
                // 更新用戶信息
                user.FullName = updateRequest.FullName ?? user.FullName;
                user.IsActive = updateRequest.IsActive ?? user.IsActive;
                
                // 保存更改
                var updatedUser = await _userService.UpdateUserAsync(user);
                
                // 將更新後的用戶對象轉換為DTO
                var userDto = new UserDto
            {
                    Id = updatedUser.Id,
                    Username = updatedUser.Username,
                    Email = updatedUser.Email,
                    FullName = updatedUser.FullName,
                    IsActive = updatedUser.IsActive,
                    EmailVerified = updatedUser.EmailVerified,
                    CreatedAt = updatedUser.CreatedAt,
                    UpdatedAt = updatedUser.UpdatedAt,
                    LastLogin = updatedUser.LastLogin
                };
                
                return Ok(userDto);
        }
            catch (Exception ex)
    {
                _logger.LogError(ex, "更新用戶時發生錯誤: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "更新用戶時發生錯誤");
            }
        }
        
        /// <summary>
        /// 刪除用戶
        /// </summary>
        /// <param name="id">用戶ID</param>
        /// <returns>操作結果</returns>
        [HttpDelete("{id}")]
        [RequirePermission("User", "Delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                _logger.LogInformation("刪除用戶: {UserId}", id);
                
                // 添加 await 以避免警告
                await Task.CompletedTask;
                
                // 這裡實現刪除用戶的邏輯
                // 暫時只返回成功
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除用戶時發生錯誤: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "刪除用戶時發生錯誤");
            }
        }
    }
    
    /// <summary>
    /// 用戶數據傳輸對象
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// 用戶名
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// 電子郵件
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// 全名
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// 電子郵件是否已驗證
        /// </summary>
        public bool EmailVerified { get; set; }
        
        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// 最後登入時間
        /// </summary>
        public DateTime? LastLogin { get; set; }
    }
    
    /// <summary>
    /// 更新用戶請求
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// 全名
        /// </summary>
        public string FullName { get; set; }
        
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool? IsActive { get; set; }
}
}