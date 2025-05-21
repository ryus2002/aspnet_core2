using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    /// <summary>
    /// 用戶註冊請求DTO
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// 用戶名，用於登入
        /// </summary>
        [Required(ErrorMessage = "用戶名為必填項")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "用戶名長度必須在3到50個字符之間")]
        public required string Username { get; set; }
        
        /// <summary>
        /// 用戶電子郵件
        /// </summary>
        [Required(ErrorMessage = "電子郵件為必填項")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        public required string Email { get; set; }
        
        /// <summary>
        /// 用戶密碼
        /// </summary>
        [Required(ErrorMessage = "密碼為必填項")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在6到100個字符之間")]
        public required string Password { get; set; }
        
        /// <summary>
        /// 確認密碼
        /// </summary>
        [Required(ErrorMessage = "確認密碼為必填項")]
        [Compare("Password", ErrorMessage = "密碼和確認密碼不匹配")]
        public required string ConfirmPassword { get; set; }
        
        /// <summary>
        /// 用戶全名
        /// </summary>
        [StringLength(100, ErrorMessage = "全名長度不能超過100個字符")]
        public required string FullName { get; set; }
    }
    
    /// <summary>
    /// 用戶登入請求DTO
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// 用戶名或電子郵件
        /// </summary>
        [Required(ErrorMessage = "用戶名/電子郵件為必填項")]
        public required string Username { get; set; }
        
        /// <summary>
        /// 用戶密碼
        /// </summary>
        [Required(ErrorMessage = "密碼為必填項")]
        public required string Password { get; set; }
    }
    
    /// <summary>
    /// 刷新令牌請求DTO
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// 刷新令牌
        /// </summary>
        [Required(ErrorMessage = "刷新令牌為必填項")]
        public required string RefreshToken { get; set; }
    }
    
    /// <summary>
    /// 重置密碼請求DTO
    /// </summary>
    public class ResetPasswordRequest
    {
        /// <summary>
        /// 用戶電子郵件
        /// </summary>
        [Required(ErrorMessage = "電子郵件為必填項")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        public required string Email { get; set; }
    }
    
    /// <summary>
    /// 設置新密碼請求DTO
    /// </summary>
    public class SetNewPasswordRequest
    {
        /// <summary>
        /// 重置令牌
        /// </summary>
        [Required(ErrorMessage = "重置令牌為必填項")]
        public required string ResetToken { get; set; }
        
        /// <summary>
        /// 新密碼
        /// </summary>
        [Required(ErrorMessage = "新密碼為必填項")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在6到100個字符之間")]
        public required string NewPassword { get; set; }
        
        /// <summary>
        /// 確認新密碼
        /// </summary>
        [Required(ErrorMessage = "確認新密碼為必填項")]
        [Compare("NewPassword", ErrorMessage = "新密碼和確認新密碼不匹配")]
        public required string ConfirmNewPassword { get; set; }
    }
    
    /// <summary>
    /// 認證響應DTO
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        public required string Id { get; set; }
        
        /// <summary>
        /// 用戶名
        /// </summary>
        public required string Username { get; set; }
        
        /// <summary>
        /// 用戶電子郵件
        /// </summary>
        public required string Email { get; set; }
        
        /// <summary>
        /// 用戶全名
        /// </summary>
        public required string FullName { get; set; }
        
        /// <summary>
        /// 用戶角色列表
        /// </summary>
        public required List<string> Roles { get; set; } = new List<string>();
        
        /// <summary>
        /// JWT訪問令牌
        /// </summary>
        public required string AccessToken { get; set; }
        
        /// <summary>
        /// 訪問令牌過期時間
        /// </summary>
        public DateTime AccessTokenExpires { get; set; }
        
        /// <summary>
        /// 刷新令牌
        /// </summary>
        public required string RefreshToken { get; set; }
        
        /// <summary>
        /// 用戶是否已驗證電子郵件
        /// </summary>
        public bool EmailVerified { get; set; }
    }
}