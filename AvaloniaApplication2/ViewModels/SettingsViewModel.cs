using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System.Linq;

namespace AvaloniaApplication2.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;

        [ObservableProperty]
        private int selectedThemeIndex = 1; // 0=Dark, 1=Light, 2=System

        [ObservableProperty]
        private bool autoLoadPlugins;

        [ObservableProperty]
        private string pluginsDirectory;

        [ObservableProperty]
        private string appVersion = "1.0.0";

        [ObservableProperty]
        private string statusMessage = "";

        [ObservableProperty]
        private int selectedSettingsTab;

        // 启动页面
        [ObservableProperty]
        private int defaultStartupPageIndex;

        // 语言 (0=Auto, 1=简体中文, 2=English)
        [ObservableProperty]
        private int languageIndex = 1;

        // 主题色 (0=默认蓝, 1=翠绿, 2=紫色, 3=橙色, 4=红色)
        [ObservableProperty]
        private int accentColorIndex;

        // 背景不透明度
        [ObservableProperty]
        private double backgroundOpacity = 85;

        // 自定义背景图片
        [ObservableProperty]
        private bool customBackgroundImage;

        // 背景图片路径
        [ObservableProperty]
        private string backgroundImagePath = "";

        // 缓存目录
        [ObservableProperty]
        private string cacheDirectory = System.IO.Path.Combine(System.AppContext.BaseDirectory, "appdata");

        // 预设主题色
        private static readonly Color[] AccentColors = new[]
        {
            Color.FromRgb(0, 103, 192),    // 默认蓝
            Color.FromRgb(16, 137, 62),     // 翠绿
            Color.FromRgb(136, 23, 152),    // 紫色
            Color.FromRgb(210, 75, 0),      // 橙色
            Color.FromRgb(197, 0, 47),      // 红色
        };

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;

            // 加载当前设置
            SelectedThemeIndex = settingsService.Settings.Theme.ToLower() switch
            {
                "dark" => 0,
                "system" => 2,
                _ => 1 // light (默认浅色)
            };
            AutoLoadPlugins = settingsService.Settings.AutoLoadPlugins;
            PluginsDirectory = settingsService.Settings.PluginsDirectory;
            LanguageIndex = settingsService.Settings.LanguageIndex;
            AccentColorIndex = settingsService.Settings.AccentColorIndex;
            BackgroundOpacity = settingsService.Settings.BackgroundOpacity;
            CustomBackgroundImage = settingsService.Settings.CustomBackgroundImage;
            BackgroundImagePath = settingsService.Settings.BackgroundImagePath;
            DefaultStartupPageIndex = settingsService.Settings.DefaultStartupPageIndex;

            // 应用已保存的主题色和背景透明度
            if (AccentColorIndex >= 0 && AccentColorIndex < AccentColors.Length)
            {
                ApplyAccentColor();
            }
            ApplyBackgroundOpacity();
        }

        [RelayCommand]
        private void NavigateSettingsTab(string tabIndex)
        {
            if (int.TryParse(tabIndex, out var index))
                SelectedSettingsTab = index;
        }

        // ===== 主题 =====

        [RelayCommand]
        private async Task SaveThemeAsync()
        {
            string themeValue = SelectedThemeIndex switch
            {
                0 => "Dark",
                2 => "System",
                _ => "Light"
            };

            await _settingsService.UpdateThemeAsync(themeValue);
            ApplyTheme(themeValue);
            StatusMessage = $"主题已切换为: {themeValue}";
        }

        private void ApplyTheme(string theme)
        {
            if (App.Current == null) return;

            var themeVariant = theme.ToLower() switch
            {
                "light" => Avalonia.Styling.ThemeVariant.Light,
                "dark" => Avalonia.Styling.ThemeVariant.Dark,
                "system" => Avalonia.Styling.ThemeVariant.Default,
                _ => Avalonia.Styling.ThemeVariant.Default
            };

            App.Current.RequestedThemeVariant = themeVariant;
        }

        partial void OnSelectedThemeIndexChanged(int value)
        {
            _ = SaveThemeAsync();
        }

        // ===== 主题色 =====

        [RelayCommand]
        private void ApplyAccentColor()
        {
            if (App.Current == null || AccentColorIndex < 0 || AccentColorIndex >= AccentColors.Length)
                return;

            var color = AccentColors[AccentColorIndex];
            var brush = new SolidColorBrush(color);

            // 生成浅色变体（用于背景色，如 hover/active 状态）
            var lightColor = Color.FromRgb(
                (byte)Math.Min(255, color.R + 160),
                (byte)Math.Min(255, color.G + 160),
                (byte)Math.Min(255, color.B + 160));
            var lightBrush = new SolidColorBrush(lightColor);

            // 顶部栏主题色底色（非常淡，约10%透明度）
            var topBarTintColor = Color.FromArgb(25, color.R, color.G, color.B);
            var topBarTintBrush = new SolidColorBrush(topBarTintColor);

            // 侧边栏主题色底色
            var sidebarTintColor = Color.FromArgb(18, color.R, color.G, color.B);
            var sidebarTintBrush = new SolidColorBrush(sidebarTintColor);

            // 更新全局资源
            App.Current.Resources["AccentBrush"] = brush;
            App.Current.Resources["AccentColor"] = color;
            App.Current.Resources["NavActiveBrush"] = lightBrush;
            App.Current.Resources["NavActiveColor"] = color;
            App.Current.Resources["TopBarAccentBorderBrush"] = brush;
            App.Current.Resources["TopBarBackgroundBrush"] = topBarTintBrush;
            App.Current.Resources["SidebarBackgroundBrush"] = sidebarTintBrush;

            _ = _settingsService.UpdateAccentColorAsync(AccentColorIndex);
            StatusMessage = "主题色已更新";
        }

        partial void OnAccentColorIndexChanged(int value)
        {
            ApplyAccentColor();
        }

        // ===== 语言 =====

        partial void OnLanguageIndexChanged(int value)
        {
            _ = _settingsService.UpdateLanguageAsync(value);
            StatusMessage = value switch
            {
                0 => "语言已设置为: Auto",
                1 => "语言已设置为: 简体中文",
                2 => "语言已设置为: English",
                _ => ""
            };
        }

        // ===== 背景不透明度 =====

        partial void OnBackgroundOpacityChanged(double value)
        {
            _ = _settingsService.UpdateBackgroundOpacityAsync(value);
            ApplyBackgroundOpacity();
        }

        private void ApplyBackgroundOpacity()
        {
            if (App.Current == null) return;

            if (CustomBackgroundImage)
            {
                ApplyBackgroundImage();
                return;
            }

            // 移除应用层覆盖以获取当前主题的原始背景色
            App.Current.Resources.Remove("WindowBackgroundBrush");
            var bgBrush = App.Current.Resources["WindowBackgroundBrush"] as SolidColorBrush;
            if (bgBrush == null) return;

            var opacity = BackgroundOpacity / 100.0;
            if (opacity >= 1.0) return; // 完全不需要调整

            // 基于当前主题背景色创建带透明度的版本
            var adjustedColor = Color.FromArgb(
                (byte)(255 * opacity),
                bgBrush.Color.R, bgBrush.Color.G, bgBrush.Color.B);
            App.Current.Resources["WindowBackgroundBrush"] = new SolidColorBrush(adjustedColor);
        }

        // ===== 自定义背景图片 =====

        partial void OnCustomBackgroundImageChanged(bool value)
        {
            _ = _settingsService.UpdateCustomBackgroundImageAsync(value);
            if (!value && App.Current != null)
            {
                // 移除应用层覆盖，让 ThemeDictionary 的背景生效
                App.Current.Resources.Remove("WindowBackgroundBrush");
                ApplyBackgroundOpacity();
            }
        }

        [RelayCommand]
        private async Task SelectBackgroundImageAsync()
        {
            try
            {
                var mainWindow = App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
                if (mainWindow?.MainWindow is Window window)
                {
                    var fileTypes = new FilePickerFileType[]
                    {
                        new("图片文件")
                        {
                            Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.webp" }
                        }
                    };
                    var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "选择背景图片",
                        AllowMultiple = false,
                        FileTypeFilter = fileTypes
                    });

                    if (files != null && files.Count > 0)
                    {
                        BackgroundImagePath = files[0].Path.LocalPath;
                        await _settingsService.UpdateBackgroundImagePathAsync(BackgroundImagePath);
                        ApplyBackgroundImage();
                        StatusMessage = "背景图片已设置";
                    }
                }
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"选择图片失败: {ex.Message}";
            }
        }

        private void ApplyBackgroundImage()
        {
            if (App.Current == null || string.IsNullOrEmpty(BackgroundImagePath))
                return;

            if (CustomBackgroundImage && System.IO.File.Exists(BackgroundImagePath))
            {
                try
                {
                    var brush = new ImageBrush(new Avalonia.Media.Imaging.Bitmap(BackgroundImagePath))
                    {
                        Stretch = Stretch.UniformToFill,
                        Opacity = BackgroundOpacity / 100.0
                    };
                    App.Current.Resources["WindowBackgroundBrush"] = brush;
                }
                catch
                {
                    // 图片加载失败
                }
            }
        }

        // ===== 自动加载、插件目录 =====

        [RelayCommand]
        private async Task SaveAutoLoadAsync()
        {
            await _settingsService.UpdateAutoLoadPluginsAsync(AutoLoadPlugins);
            StatusMessage = "设置已保存";
        }

        [RelayCommand]
        private async Task SavePluginsDirectoryAsync()
        {
            await _settingsService.UpdatePluginsDirectoryAsync(PluginsDirectory);
            StatusMessage = "插件目录已更新";
        }

        // ===== 缓存 =====

        [RelayCommand]
        private void ClearCache()
        {
            try
            {
                if (System.IO.Directory.Exists(CacheDirectory))
                {
                    System.IO.Directory.Delete(CacheDirectory, true);
                    System.IO.Directory.CreateDirectory(CacheDirectory);
                    StatusMessage = "缓存已清除";
                }
                else
                {
                    StatusMessage = "缓存目录不存在";
                }
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"清除缓存失败: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task SelectCacheFolderAsync()
        {
            try
            {
                var mainWindow = App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
                if (mainWindow?.MainWindow is Window window)
                {
                    var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                    {
                        Title = "选择缓存数据目录",
                        AllowMultiple = false
                    });

                    if (folders != null && folders.Count > 0)
                    {
                        CacheDirectory = folders[0].Path.LocalPath;
                        StatusMessage = $"缓存目录已选择: {folders[0].Path.LocalPath}";
                    }
                }
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"选择文件夹失败: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task SelectFolderAsync()
        {
            try
            {
                var mainWindow = App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
                if (mainWindow?.MainWindow is Window window)
                {
                    var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                    {
                        Title = "选择插件目录",
                        AllowMultiple = false
                    });

                    if (folders != null && folders.Count > 0)
                    {
                        PluginsDirectory = folders[0].Path.LocalPath;
                        StatusMessage = $"已选择: {folders[0].Path.LocalPath}";
                    }
                }
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"选择文件夹失败: {ex.Message}";
            }
        }
    }
}
