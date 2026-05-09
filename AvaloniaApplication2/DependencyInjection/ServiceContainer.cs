using AvaloniaApplication2.Infrastructure;
using AvaloniaApplication2.Services;
using AvaloniaApplication2.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

namespace AvaloniaApplication2.DependencyInjection
{
    /// <summary>
    /// 服务容器配置 - 管理依赖注入
    /// </summary>
    public static class ServiceContainer
    {
        private static ServiceProvider? _serviceProvider;

        /// <summary>
        /// 获取服务提供者
        /// </summary>
        public static IServiceProvider ServiceProvider 
        { 
            get 
            { 
                if (_serviceProvider == null)
                {
                    ConfigureServices();
                }
                return _serviceProvider!; 
            } 
        }

        /// <summary>
        /// 配置所有服务
        /// </summary>
        public static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // 注册日志
            services.AddSingleton(LoggingConfig.Logger);

            // 注册核心服务
            services.AddSingleton<SettingsService>();
            services.AddSingleton<NotificationService>();
            services.AddSingleton<PluginManager>();
            services.AddSingleton<PluginHotReloadManager>();
            services.AddSingleton<SettingsSyncService>();
            services.AddSingleton<ClipboardService>();
            services.AddSingleton<PluginSecurityService>();
            services.AddSingleton<GlobalExceptionHandler>();

            // 注册 ViewModel（Transient，每次请求创建新实例）
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<PluginManagerViewModel>();
            services.AddTransient<SettingsViewModel>();

            _serviceProvider = services.BuildServiceProvider();

            // 初始化全局异常处理
            var exceptionHandler = ServiceProvider.GetRequiredService<GlobalExceptionHandler>();
            exceptionHandler.RegisterGlobalHandlers();

            LoggingConfig.Logger.Information("依赖注入容器已配置完成");
        }

        /// <summary>
        /// 获取指定类型的服务
        /// </summary>
        public static T GetRequiredService<T>() where T : notnull
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// 尝试获取服务
        /// </summary>
        public static T? GetService<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }

        /// <summary>
        /// 关闭并清理服务容器
        /// </summary>
        public static void Shutdown()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            
            LoggingConfig.CloseAndFlush();
        }
    }
}
