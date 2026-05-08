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
                DisableAvaloniaDataAnnotationValidation();

                ServiceContainer.ConfigureServices();

                var settingsService = ServiceContainer.GetRequiredService<SettingsService>();
                var pluginManager = ServiceContainer.GetRequiredService<PluginManager>();

                ApplySavedTheme(settingsService.Settings.Theme);
                ApplySavedAccentColor(settingsService.Settings.AccentColorIndex);

                var mainWindowViewModel = ServiceContainer.GetRequiredService<MainWindowViewModel>();

                var mainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };

                // 恢复窗口位置
                if (settingsService.Settings.WindowX.HasValue && settingsService.Settings.WindowY.HasValue)
                {
                    mainWindow.Position = new PixelPoint(
                        (int)settingsService.Settings.WindowX.Value,
                        (int)settingsService.Settings.WindowY.Value);
                }

                // 恢复侧边栏状态
                mainWindowViewModel.IsSidebarExpanded = settingsService.Settings.SidebarExpanded;
                mainWindowViewModel.SidebarWidth = settingsService.Settings.SidebarWidth.ToString();

                desktop.MainWindow = mainWindow;

                // 退出时保存窗口位置和侧边栏状态
                desktop.Exit += OnApplicationExit;

                // 启动日志记录
                var notify = NotificationService.Instance;
                notify.ShowInfo("应用程序启动");
                notify.ShowInfo($"插件目录: {pluginManager.PluginsDirectory}");

                _ = pluginManager.LoadPluginsAsync();

                Log.Information("应用程序启动完成");
                notify.ShowSuccess("应用程序启动完成");
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async void OnApplicationExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Log.Information("应用程序正在退出...");

            var hotReloadManager = ServiceContainer.GetService<PluginHotReloadManager>();
            hotReloadManager?.StopAllWatching();

            var pluginManager = ServiceContainer.GetService<PluginManager>();
            pluginManager?.StopFolderWatcher();

            // 保存窗口位置和侧边栏状态
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow is { } window)
            {
                var settingsService = ServiceContainer.GetService<SettingsService>();
                if (settingsService != null)
                {
                    await settingsService.UpdateWindowPositionAsync(
                        window.Position.X,
                        window.Position.Y);
                    await settingsService.UpdateSidebarLayoutAsync(
                        (window.DataContext as MainWindowViewModel)?.IsSidebarExpanded ?? false,
                        double.TryParse((window.DataContext as MainWindowViewModel)?.SidebarWidth, out var w) ? w : 64);
                }
            }

            ServiceContainer.Shutdown();
        }

        private void ApplySavedTheme(string theme)
        {
            var themeVariant = theme.ToLower() switch
            {
                "light" => Avalonia.Styling.ThemeVariant.Light,
                "dark" => Avalonia.Styling.ThemeVariant.Dark,
                "system" => Avalonia.Styling.ThemeVariant.Default,
                _ => Avalonia.Styling.ThemeVariant.Light
            };

            RequestedThemeVariant = themeVariant;
        }

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
