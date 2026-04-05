using Serilog;
using Serilog.Events;
using System;

namespace AvaloniaApplication2.Infrastructure
{
    /// <summary>
    /// 日志配置类 - 配置和管理应用程序的日志系统
    /// </summary>
    public static class LoggingConfig
    {
        private static ILogger? _logger;

        /// <summary>
        /// 获取全局日志实例
        /// </summary>
        public static ILogger Logger => _logger ??= ConfigureLogger();

        /// <summary>
        /// 配置日志系统
        /// </summary>
        public static ILogger ConfigureLogger()
        {
            var logDirectory = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "Logs"
            );

            if (!System.IO.Directory.Exists(logDirectory))
            {
                System.IO.Directory.CreateDirectory(logDirectory);
            }

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ThreadId}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.File(
                    path: System.IO.Path.Combine(logDirectory, "app-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ThreadId}] {Message:lj}{NewLine}{Exception}",
                    shared: true
                )
                .CreateLogger();

            return logger;
        }

        /// <summary>
        /// 关闭并刷新日志
        /// </summary>
        public static void CloseAndFlush()
        {
            (_logger as Serilog.Core.Logger)?.Dispose();
        }
    }
}
