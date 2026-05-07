using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaApplication2.DependencyInjection;
using AvaloniaApplication2.Services;
using AvaloniaApplication2.ViewModels;
using Serilog;
using System;
using System.Linq;

namespace AvaloniaApplication2
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // 禁用重复的数据验证
                DisableAvaloniaDataAnnotationValidation();

                // 初始化依赖注入容器
                ServiceContainer.ConfigureServices();

                // 从容器获取服务
                var settingsService = ServiceContainer.GetRequiredService<SettingsService>();
                var pluginManager = ServiceContainer.GetRequiredService<PluginManager>();

                // 应用保存的主题设置
                ApplySavedTheme(settingsService.Settings.Theme);

                // 应用已保存的主题色
                ApplySavedAccentColor(settingsService.Settings.AccentColorIndex);

                // 创建主窗口 ViewModel（从容器获取）
                var mainWindowViewModel = ServiceContainer.GetRequiredService<MainWindowViewModel>();

                // 创建主窗口
                var mainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };

                desktop.MainWindow = mainWindow;

                // 注册退出事件
                desktop.Exit += OnApplicationExit;

                // 异步加载插件
                _ = pluginManager.LoadPluginsAsync();

                Log.Information("应用程序启动完成");
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// 应用程序退出处理
        /// </summary>
        private void OnApplicationExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Log.Information("应用程序正在退出...");
            
            // 停止所有插件热重载监视
            var hotReloadManager = ServiceContainer.GetService<PluginHotReloadManager>();
            hotReloadManager?.StopAllWatching();

            // 停止 Plugins 文件夹监视
            var pluginManager = ServiceContainer.GetService<PluginManager>();
            pluginManager?.StopFolderWatcher();

            ServiceContainer.Shutdown();
        }

        /// <summary>
        /// 应用保存的主题设置
        /// </summary>
        private void ApplySavedTheme(string theme)
        {
            var themeVariant = theme.ToLower() switch
            {
                "light" => Avalonia.Styling.ThemeVariant.Light,
                "dark" => Avalonia.Styling.ThemeVariant.Dark,
                "system" => Avalonia.Styling.ThemeVariant.Default,
                _ => Avalonia.Styling.ThemeVariant.Light // 默认浅色主题
            };

            RequestedThemeVariant = themeVariant;
        }

        /// <summary>
        /// 启动时应用已保存的主题色
        /// </summary>
        private static void ApplySavedAccentColor(int accentColorIndex)
        {
            var accentColors = new[]
            {
                Color.FromRgb(0, 103, 192),
                Color.FromRgb(16, 137, 62),
                Color.FromRgb(136, 23, 152),
                Color.FromRgb(210, 75, 0),
                Color.FromRgb(197, 0, 47),
            };

            if (accentColorIndex < 0 || accentColorIndex >= accentColors.Length) return;

            var color = accentColors[accentColorIndex];
            var brush = new SolidColorBrush(color);

            var lightColor = Color.FromRgb(
                (byte)Math.Min(255, color.R + 160),
                (byte)Math.Min(255, color.G + 160),
                (byte)Math.Min(255, color.B + 160));
            var lightBrush = new SolidColorBrush(lightColor);

            var topBarTintColor = Color.FromArgb(25, color.R, color.G, color.B);
            var sidebarTintColor = Color.FromArgb(18, color.R, color.G, color.B);

            if (Current?.Resources is { } res)
            {
                res["AccentBrush"] = brush;
                res["AccentColor"] = color;
                res["NavActiveBrush"] = lightBrush;
                res["NavActiveColor"] = color;
                res["TopBarAccentBorderBrush"] = brush;
                res["TopBarBackgroundBrush"] = new SolidColorBrush(topBarTintColor);
                res["SidebarBackgroundBrush"] = new SolidColorBrush(sidebarTintColor);
            }
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}