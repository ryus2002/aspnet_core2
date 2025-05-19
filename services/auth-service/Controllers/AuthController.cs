using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthService.Controllers
{
    /// <summary>
    /// 認證控制器，處理用戶註冊、登入等認證相關操作
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// 構造函數，注入所需服務
        /// </summary>
        /// <param name="userService">用戶服務</param>
        /// <param name="jwtService">JWT服務</param>
        /// <param name="logger">日誌記錄器</param>
        public AuthController(IUserService userService, IJwtService jwtService, ILogger<AuthController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 用戶註冊
        /// </summary>
        /// <param name="request">註冊請求</param>
        /// <returns>註冊結果</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("收到用戶註冊請求: {Username}", request.Username);

                // 調用用戶服務進行註冊
                var user = await _userService.Register(request);
                
                // 獲取用戶角色
                var roles = await _userService.GetUserRoles(user.Id);
                
                // 生成JWT令牌
                var (accessToken, expires) = await _jwtService.GenerateJwtToken(user, roles);
                
                // 生成刷新令牌
                var refreshToken = await _jwtService.GenerateRefreshToken(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                
                // 將刷新令牌添加到用戶的刷新令牌集合中
                user.RefreshTokens.Add(refreshToken);
                await _userService.UpdateUserAsync(user);
                
                // 創建認證響應
                var response = new AuthResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = roles,
                    AccessToken = accessToken,
                    AccessTokenExpires = expires,
                    RefreshToken = refreshToken.Token,
                    EmailVerified = user.EmailVerified
                };
                
                _logger.LogInformation("用戶註冊成功: {Username}", user.Username);
                
                return CreatedAtAction(nameof(Register), response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("用戶註冊失敗: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用戶註冊過程中發生錯誤");
                return StatusCode(StatusCodes.Status500InternalServerError, "註冊過程中發生錯誤，請稍後再試");
            }
        }
        
        /// <summary>
        /// 用戶登入
        /// </summary>
        /// <param name="request">登入請求</param>
        /// <returns>登入結果</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("收到用戶登入請求: {Username}", request.Username);
                
                // 根據用戶名或電子郵件查找用戶
                User user = null;
                
                // 檢查輸入是否為電子郵件格式
                if (request.Username.Contains("@"))
                {
                    user = await _userService.GetByEmail(request.Username);
    }
                else
                {
                    user = await _userService.GetByUsername(request.Username);
}
                
                // 如果用戶不存在，返回未授權
                if (user == null)
                {
                    _logger.LogWarning("登入失敗：用戶不存在 {Username}", request.Username);
                    return Unauthorized("用戶名或密碼不正確");
                }
                
                // 驗證密碼
                if (!await _userService.ValidatePassword(user, request.Password))
                {
                    _logger.LogWarning("登入失敗：密碼不正確 {Username}", request.Username);
                    return Unauthorized("用戶名或密碼不正確");
                }
                
                // 檢查用戶是否被禁用
                if (!user.IsActive)
                {
                    _logger.LogWarning("登入失敗：用戶被禁用 {Username}", request.Username);
                    return Unauthorized("您的帳戶已被禁用，請聯繫管理員");
                }
                
                // 獲取用戶角色
                var roles = await _userService.GetUserRoles(user.Id);
                
                // 生成JWT令牌
                var (accessToken, expires) = await _jwtService.GenerateJwtToken(user, roles);
                
                // 生成刷新令牌
                var refreshToken = await _jwtService.GenerateRefreshToken(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                
                // 將刷新令牌添加到用戶的刷新令牌集合中
                user.RefreshTokens.Add(refreshToken);
                
                // 更新用戶的最後登入信息
                user.LastLogin = DateTime.UtcNow;
                user.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                // 保存用戶更新
                await _userService.UpdateUserAsync(user);
                
                // 創建認證響應
                var response = new AuthResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = roles,
                    AccessToken = accessToken,
                    AccessTokenExpires = expires,
                    RefreshToken = refreshToken.Token,
                    EmailVerified = user.EmailVerified
                };
                
                _logger.LogInformation("用戶登入成功: {Username}", user.Username);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用戶登入過程中發生錯誤");
                return StatusCode(StatusCodes.Status500InternalServerError, "登入過程中發生錯誤，請稍後再試");
            }
        }
        
        /// <summary>
        /// 刷新令牌
        /// </summary>
        /// <param name="request">刷新令牌請求</param>
        /// <returns>新的訪問令牌和刷新令牌</returns>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                _logger.LogInformation("收到刷新令牌請求");
                
                // 驗證刷新令牌是否存在
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest("刷新令牌不能為空");
                }
                
                // 查找擁有此刷新令牌的用戶
                var user = await _userService.GetUserByRefreshToken(request.RefreshToken);
                
                if (user == null)
                {
                    _logger.LogWarning("刷新令牌失敗：找不到有效的刷新令牌");
                    return Unauthorized("無效的刷新令牌");
                }
                
                // 找到對應的刷新令牌
                var refreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);
                
                // 檢查刷新令牌是否有效
                if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("刷新令牌失敗：刷新令牌已過期或被撤銷");
                    return Unauthorized("刷新令牌已過期或被撤銷");
                }
                
                // 獲取用戶角色
                var roles = await _userService.GetUserRoles(user.Id);
                
                // 生成新的JWT令牌
                var (accessToken, expires) = await _jwtService.GenerateJwtToken(user, roles);
                
                // 生成新的刷新令牌
                var newRefreshToken = await _jwtService.GenerateRefreshToken(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                
                // 撤銷舊的刷新令牌
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.ReplacedByToken = newRefreshToken.Token;
                refreshToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                // 添加新的刷新令牌
                user.RefreshTokens.Add(newRefreshToken);
                
                // 保存用戶更新
                await _userService.UpdateUserAsync(user);
                
                // 創建認證響應
                var response = new AuthResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = roles,
                    AccessToken = accessToken,
                    AccessTokenExpires = expires,
                    RefreshToken = newRefreshToken.Token,
                    EmailVerified = user.EmailVerified
                };
                
                _logger.LogInformation("刷新令牌成功: {Username}", user.Username);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新令牌過程中發生錯誤");
                return StatusCode(StatusCodes.Status500InternalServerError, "刷新令牌過程中發生錯誤，請稍後再試");
            }
        }
        
        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="request">刷新令牌請求</param>
        /// <returns>登出結果</returns>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            try
            {
                _logger.LogInformation("收到用戶登出請求");
                
                // 如果沒有提供刷新令牌，直接返回成功
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return Ok(new { message = "登出成功" });
                }
                
                // 查找擁有此刷新令牌的用戶
                var user = await _userService.GetUserByRefreshToken(request.RefreshToken);
                
                // 如果找不到用戶，也視為登出成功
                if (user == null)
                {
                    return Ok(new { message = "登出成功" });
                }
                
                // 找到對應的刷新令牌
                var refreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);
                
                // 如果找到了刷新令牌，則撤銷它
                if (refreshToken != null && !refreshToken.IsRevoked)
                {
                    refreshToken.RevokedAt = DateTime.UtcNow;
                    refreshToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    
                    // 保存用戶更新
                    await _userService.UpdateUserAsync(user);
                }
                
                _logger.LogInformation("用戶登出成功");
                
                return Ok(new { message = "登出成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用戶登出過程中發生錯誤");
                return StatusCode(StatusCodes.Status500InternalServerError, "登出過程中發生錯誤，請稍後再試");
            }
        }
    }
}