namespace ProductService.Exceptions
{
    /// <summary>
    /// 表示錯誤請求的異常
    /// </summary>
    public class BadRequestException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="BadRequestException"/> 類的新實例
        /// </summary>
        public BadRequestException() : base() { }

        /// <summary>
        /// 使用指定的錯誤消息初始化 <see cref="BadRequestException"/> 類的新實例
        /// </summary>
        /// <param name="message">描述錯誤的消息</param>
        public BadRequestException(string message) : base(message) { }

        /// <summary>
        /// 使用指定的錯誤消息和對作為此異常原因的內部異常的引用來初始化 <see cref="BadRequestException"/> 類的新實例
        /// </summary>
        /// <param name="message">描述錯誤的消息</param>
        /// <param name="innerException">導致當前異常的異常</param>
        public BadRequestException(string message, Exception innerException) : base(message, innerException) { }
    }
}