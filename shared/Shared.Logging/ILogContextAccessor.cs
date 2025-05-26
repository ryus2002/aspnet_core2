using System.Collections.Concurrent;

namespace Shared.Logging
{
    /// <summary>
    /// 日誌上下文訪問器接口
    /// </summary>
    public interface ILogContextAccessor
    {
        /// <summary>
        /// 添加或更新上下文屬性
        /// </summary>
        /// <param name="key">屬性鍵</param>
        /// <param name="value">屬性值</param>
        void SetProperty(string key, object value);

        /// <summary>
        /// 獲取上下文屬性
        /// </summary>
        /// <param name="key">屬性鍵</param>
        /// <returns>屬性值，如不存在則返回 null</returns>
        object? GetProperty(string key);

        /// <summary>
        /// 移除上下文屬性
        /// </summary>
        /// <param name="key">屬性鍵</param>
        void RemoveProperty(string key);

        /// <summary>
        /// 清除所有上下文屬性
        /// </summary>
        void ClearProperties();

        /// <summary>
        /// 獲取所有上下文屬性
        /// </summary>
        /// <returns>屬性字典</returns>
        IDictionary<string, object> GetAllProperties();
    }

    /// <summary>
    /// 日誌上下文訪問器實現
    /// </summary>
    public class LogContextAccessor : ILogContextAccessor
    {
        private readonly ConcurrentDictionary<string, object> _properties = new();

        /// <summary>
        /// 添加或更新上下文屬性
        /// </summary>
        /// <param name="key">屬性鍵</param>
        /// <param name="value">屬性值</param>
        public void SetProperty(string key, object value)
        {
            _properties.AddOrUpdate(key, value, (_, _) => value);
        }

        /// <summary>
        /// 獲取上下文屬性
        /// </summary>
        /// <param name="key">屬性鍵</param>
        /// <returns>屬性值，如不存在則返回 null</returns>
        public object? GetProperty(string key)
        {
            _properties.TryGetValue(key, out var value);
            return value;
        }

        /// <summary>
        /// 移除上下文屬性
        /// </summary>
        /// <param name="key">屬性鍵</param>
        public void RemoveProperty(string key)
        {
            _properties.TryRemove(key, out _);
        }

        /// <summary>
        /// 清除所有上下文屬性
        /// </summary>
        public void ClearProperties()
        {
            _properties.Clear();
        }

        /// <summary>
        /// 獲取所有上下文屬性
        /// </summary>
        /// <returns>屬性字典</returns>
        public IDictionary<string, object> GetAllProperties()
        {
            return new Dictionary<string, object>(_properties);
        }
    }
}