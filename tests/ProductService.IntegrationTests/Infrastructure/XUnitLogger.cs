using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace ProductService.IntegrationTests.Infrastructure
{
    public class XUnitLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _categoryName;

        public XUnitLogger(ITestOutputHelper testOutputHelper, string categoryName)
        {
            _testOutputHelper = testOutputHelper;
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NoopDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            try
            {
                _testOutputHelper.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {_categoryName}: {formatter(state, exception)}");
                
                if (exception != null)
                {
                    _testOutputHelper.WriteLine($"Exception: {exception}");
                }
            }
            catch (Exception)
            {
                // 忽略寫入測試輸出時的錯誤
            }
        }

        private class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new NoopDisposable();
            public void Dispose() { }
        }
    }

    public class XUnitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public XUnitLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XUnitLogger(_testOutputHelper, categoryName);
        }

        public void Dispose() { }
    }

    public static class XUnitLoggerExtensions
    {
        public static ILoggingBuilder AddXUnit(this ILoggingBuilder builder, ITestOutputHelper testOutputHelper)
        {
            builder.AddProvider(new XUnitLoggerProvider(testOutputHelper));
            return builder;
        }
    }
}