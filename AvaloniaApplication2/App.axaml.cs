using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaloniaApplication2.Services;
using AvaloniaApplication2.ViewModels;
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

                // 初始化服务
                var settingsService = new SettingsService();
                var notificationService = NotificationService.Instance;
                var pluginManager = new PluginManager(settingsService, notificationService);

                // 应用保存的主题设置
                ApplySavedTheme(settingsService.Settings.Theme);

                // 创建主窗口 ViewModel
                var mainWindowViewModel = new MainWindowViewModel(pluginManager, settingsService);

                // 创建主窗口
                var mainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };

                desktop.MainWindow = mainWindow;

                // 异步加载插件
                _ = pluginManager.LoadPluginsAsync();
            }

            base.OnFrameworkInitializationCompleted();
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
                _ => Avalonia.Styling.ThemeVariant.Dark // 默认深色主题
            };

            RequestedThemeVariant = themeVariant;
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