namespace AuthService.Settings
{
    /// <summary>
    /// JWT配置類，用於存儲JWT相關設置
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// JWT密鑰，用於簽名令牌
        /// </summary>
        public required string Secret { get; set; }
        
        /// <summary>
        /// 令牌發行者
        /// </summary>
        public required string Issuer { get; set; }
        
        /// <summary>
        /// 令牌接收者
        /// </summary>
        public required string Audience { get; set; }
        
        /// <summary>
        /// 訪問令牌過期時間（分鐘）
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; }
        
        /// <summary>
        /// 刷新令牌過期時間（天）
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; }
    }
}