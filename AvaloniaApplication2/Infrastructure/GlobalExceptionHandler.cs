using AvaloniaApplication2.Services;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaApplication2.Infrastructure
{
    /// <summary>
    /// 全局异常处理器 - 统一处理应用程序中的异常
    /// </summary>
    public class GlobalExceptionHandler
    {
        private readonly NotificationService _notificationService;
        private readonly ILogger _logger;

        public GlobalExceptionHandler(NotificationService notificationService)
        {
            _notificationService = notificationService;
            _logger = LoggingConfig.Logger.ForContext<GlobalExceptionHandler>();
        }

        /// <summary>
        /// 处理未捕获的异常
        /// </summary>
        public void HandleException(Exception ex, string context = "")
        {
            _logger.Error(ex, "未处理的异常: {Context}", context);

            // 用户友好的错误提示
            var userMessage = GetUserFriendlyMessage(ex);
            _notificationService.ShowError(userMessage);
        }

        /// <summary>
        /// 处理任务中的异常
        /// </summary>
        public void HandleTaskException(Exception ex, string taskName = "")
        {
            _logger.Warning(ex, "任务异常: {TaskName}", taskName);
            
            // 任务异常通常不显示给用户，除非是关键任务
            if (IsCriticalTask(taskName))
            {
                var userMessage = GetUserFriendlyMessage(ex);
                _notificationService.ShowWarning($"后台任务失败: {userMessage}");
            }
        }

        /// <summary>
        /// 获取用户友好的错误消息
        /// </summary>
        private string GetUserFriendlyMessage(Exception ex)
        {
            return ex switch
            {
                System.IO.FileNotFoundException => "文件未找到，请检查路径是否正确",
                System.UnauthorizedAccessException => "权限不足，请以管理员身份运行",
                System.IO.IOException ioEx when ioEx.Message.Contains("being used") => 
                    "文件正在被其他程序使用，请关闭后重试",
                System.Net.WebException => "网络连接失败，请检查网络设置",
                TimeoutException => "操作超时，请稍后重试",
                InvalidOperationException invalidOp => $"操作无效: {invalidOp.Message}",
                _ => "发生未知错误，请查看日志文件获取详细信息"
            };
        }

        /// <summary>
        /// 判断是否是关键任务
        /// </summary>
        private bool IsCriticalTask(string taskName)
        {
            var criticalTasks = new[] 
            { 
                "LoadPlugins", 
                "SaveSettings", 
                "Initialize" 
            };

            return criticalTasks.Any(taskName.Contains);
        }

        /// <summary>
        /// 注册全局异常处理器
        /// </summary>
        public void RegisterGlobalHandlers()
        {
            // 处理未捕获的异常
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    HandleException(exception, "AppDomain.UnhandledException");
                }
            };

            // 处理未观察到的任务异常
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                HandleTaskException(e.Exception, "UnobservedTaskException");
                e.SetObserved(); // 标记为已处理，防止应用崩溃
            };

            _logger.Information("全局异常处理器已注册");
        }
    }
}
