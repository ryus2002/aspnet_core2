using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthService.DTOs;
using AuthService.Services;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public AuthController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// 用戶登入
        /// </summary>
        /// <remarks>
        /// 使用用戶名/電子郵件和密碼進行登入，成功後返回訪問令牌和刷新令牌
        /// </remarks>
        /// <param name="request">登入請求參數</param>
        /// <returns>包含訪問令牌、刷新令牌和用戶資料的響應</returns>
        /// <response code="200">登入成功</response>
        /// <response code="400">請求參數無效</response>
        /// <response code="401">認證失敗</response>
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "用戶登入",
            Description = "使用用戶名/電子郵件和密碼進行登入，成功後返回訪問令牌和刷新令牌",
            OperationId = "Login",
            Tags = new[] { "認證" }
        )]
        [SwaggerResponse(200, "登入成功", typeof(AuthResponse))]
        [SwaggerResponse(400, "請求參數無效")]
        [SwaggerResponse(401, "認證失敗")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 實現登入邏輯
            return Ok(new AuthResponse());
        }

        /// <summary>
        /// 用戶登出
        /// </summary>
        /// <remarks>
        /// 使當前令牌失效
        /// </remarks>
        /// <returns>登出成功的消息</returns>
        /// <response code="200">登出成功</response>
        /// <response code="401">未認證</response>
        [Authorize]
        [HttpPost("logout")]
        [SwaggerOperation(
            Summary = "用戶登出",
            Description = "使當前令牌失效",
            OperationId = "Logout",
            Tags = new[] { "認證" }
        )]
        [SwaggerResponse(200, "登出成功")]
        [SwaggerResponse(401, "未認證")]
        public async Task<IActionResult> Logout()
        {
            // 實現登出邏輯
            return Ok(new { message = "登出成功" });
        }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        /// <remarks>
        /// 使用刷新令牌獲取新的訪問令牌
        /// </remarks>
        /// <param name="request">刷新令牌請求參數</param>
        /// <returns>包含新訪問令牌和刷新令牌的響應</returns>
        /// <response code="200">令牌刷新成功</response>
        /// <response code="400">請求參數無效</response>
        /// <response code="401">刷新令牌無效或已過期</response>
        [HttpPost("refresh-token")]
        [SwaggerOperation(
            Summary = "刷新令牌",
            Description = "使用刷新令牌獲取新的訪問令牌",
            OperationId = "RefreshToken",
            Tags = new[] { "認證" }
        )]
        [SwaggerResponse(200, "令牌刷新成功", typeof(AuthResponse))]
        [SwaggerResponse(400, "請求參數無效")]
        [SwaggerResponse(401, "刷新令牌無效或已過期")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            // 實現刷新令牌邏輯
            return Ok(new AuthResponse());
        }

        /// <summary>
        /// 用戶註冊
        /// </summary>
        /// <remarks>
        /// 註冊新用戶
        /// </remarks>
        /// <param name="request">註冊請求參數</param>
        /// <returns>註冊成功的用戶資料</returns>
        /// <response code="201">用戶創建成功</response>
        /// <response code="400">請求參數無效</response>
        /// <response code="409">用戶名或電子郵件已存在</response>
        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "用戶註冊",
            Description = "註冊新用戶",
            OperationId = "Register",
            Tags = new[] { "認證" }
        )]
        [SwaggerResponse(201, "用戶創建成功")]
        [SwaggerResponse(400, "請求參數無效")]
        [SwaggerResponse(409, "用戶名或電子郵件已存在")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // 實現註冊邏輯
            return StatusCode(201, new { /* 用戶資料 */ });
        }
    }
}