namespace ProductService.Settings
{
    /// <summary>
    /// MongoDB設置
    /// </summary>
    public class MongoDbSettings
    {
        /// <summary>
        /// 連接字符串
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// 數據庫名稱
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;
    }
}