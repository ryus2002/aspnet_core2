namespace ProductService.Exceptions
{
    /// <summary>
    /// 表示請求的資源未找到的異常
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="NotFoundException"/> 類的新實例
        /// </summary>
        public NotFoundException() : base() { }

        /// <summary>
        /// 使用指定的錯誤消息初始化 <see cref="NotFoundException"/> 類的新實例
        /// </summary>
        /// <param name="message">描述錯誤的消息</param>
        public NotFoundException(string message) : base(message) { }

        /// <summary>
        /// 使用指定的錯誤消息和對作為此異常原因的內部異常的引用來初始化 <see cref="NotFoundException"/> 類的新實例
        /// </summary>
        /// <param name="message">描述錯誤的消息</param>
        /// <param name="innerException">導致當前異常的異常</param>
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}