using AvaloniaApplication2.Models;
using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
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
        private int selectedThemeIndex = 1;

        [ObservableProperty]
        private bool autoLoadPlugins;

        [ObservableProperty]
        private string pluginsDirectory;

        [ObservableProperty]
        private string appVersion = "5.0.1";

        [ObservableProperty]
        private string statusMessage = "";

        [ObservableProperty]
        private int selectedSettingsTab;

        [ObservableProperty]
        private int defaultStartupPageIndex;

        [ObservableProperty]
        private int languageIndex = 1;

        [ObservableProperty]
        private int accentColorIndex;

        [ObservableProperty]
        private double backgroundOpacity = 85;

        [ObservableProperty]
        private bool customBackgroundImage;

        [ObservableProperty]
        private string backgroundImagePath = "";

        [ObservableProperty]
        private string cacheDirectory = System.IO.Path.Combine(AppContext.BaseDirectory, "appdata");

        [ObservableProperty]
        private string pluginMarketUrl = "";

        // 用户数据文件列表
        public ObservableCollection<UserDataFileInfo> UserDataFiles { get; } = new();

        // 搜索历史
        [ObservableProperty] private bool enableSearchHistory = true;

        // 安全检测
        [ObservableProperty] private bool enableSecurityScan = true;

        // 设置同步
        [ObservableProperty] private bool syncEnabled;
        [ObservableProperty] private bool autoSync;
        [ObservableProperty] private string syncRepoOwner = "";
        [ObservableProperty] private string syncRepoName = "";
        [ObservableProperty] private string syncToken = "";
        [ObservableProperty] private string syncBranch = "main";
        [ObservableProperty] private string? lastSyncTime;

        private static readonly Color[] AccentColors = new[]
        {
            Color.FromRgb(0, 103, 192),
            Color.FromRgb(16, 137, 62),
            Color.FromRgb(136, 23, 152),
            Color.FromRgb(210, 75, 0),
            Color.FromRgb(197, 0, 47),
        };

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;

            SelectedThemeIndex = settingsService.Settings.Theme.ToLower() switch
            {
                "dark" => 0,
                "system" => 2,
                _ => 1
            };
            AutoLoadPlugins = settingsService.Settings.AutoLoadPlugins;
            PluginsDirectory = settingsService.Settings.PluginsDirectory;
            LanguageIndex = settingsService.Settings.LanguageIndex;
            AccentColorIndex = settingsService.Settings.AccentColorIndex;
            BackgroundOpacity = settingsService.Settings.BackgroundOpacity;
            CustomBackgroundImage = settingsService.Settings.CustomBackgroundImage;
            BackgroundImagePath = settingsService.Settings.BackgroundImagePath;
            DefaultStartupPageIndex = settingsService.Settings.DefaultStartupPageIndex;
            PluginMarketUrl = settingsService.Settings.PluginMarketUrl;
            SyncEnabled = settingsService.Settings.SyncEnabled;
            AutoSync = settingsService.Settings.AutoSync;
            SyncRepoOwner = settingsService.Settings.SyncRepoOwner;
            SyncRepoName = settingsService.Settings.SyncRepoName;
            SyncToken = settingsService.Settings.SyncToken;
            SyncBranch = string.IsNullOrEmpty(settingsService.Settings.SyncBranch) ? "main" : settingsService.Settings.SyncBranch;
            LastSyncTime = settingsService.Settings.LastSyncTime;
            EnableSearchHistory = settingsService.Settings.EnableSearchHistory;

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
            NotificationService.Instance.ShowInfo($"主题切换: {themeValue}");
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

            var lightColor = Color.FromRgb(
                (byte)Math.Min(255, color.R + 160),
                (byte)Math.Min(255, color.G + 160),
                (byte)Math.Min(255, color.B + 160));
            var lightBrush = new SolidColorBrush(lightColor);

            var topBarTintColor = Color.FromArgb(25, color.R, color.G, color.B);
            var topBarTintBrush = new SolidColorBrush(topBarTintColor);

            var sidebarTintColor = Color.FromArgb(18, color.R, color.G, color.B);
            var sidebarTintBrush = new SolidColorBrush(sidebarTintColor);

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

            App.Current.Resources.Remove("WindowBackgroundBrush");
            var bgBrush = App.Current.Resources["WindowBackgroundBrush"] as SolidColorBrush;
            if (bgBrush == null) return;

            var opacity = BackgroundOpacity / 100.0;
            if (opacity >= 1.0) return;

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
            catch (Exception ex)
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
                }
            }
        }

        // ===== 设置同步 =====

        [RelayCommand]
        private async Task SaveSyncSettingsAsync()
        {
            var s = _settingsService.Settings;
            s.SyncEnabled = SyncEnabled;
            s.AutoSync = AutoSync;
            s.SyncRepoOwner = SyncRepoOwner.Trim();
            s.SyncRepoName = SyncRepoName.Trim();
            s.SyncToken = SyncToken.Trim();
            s.SyncBranch = string.IsNullOrWhiteSpace(SyncBranch) ? "main" : SyncBranch.Trim();
            await _settingsService.SaveSettingsAsync();
            LastSyncTime = s.LastSyncTime;
            StatusMessage = "同步设置已保存";
        }

        [RelayCommand]
        private async Task SyncNowAsync()
        {
            await SaveSyncSettingsAsync();
            var syncService = DependencyInjection.ServiceContainer.GetService<SettingsSyncService>();
            if (syncService == null)
            {
                StatusMessage = "同步服务不可用";
                return;
            }
            StatusMessage = "正在同步...";
            LastSyncTime = await syncService.SyncAsync();
            StatusMessage = LastSyncTime;
        }

        // 市场URL变更自动保存
        partial void OnPluginMarketUrlChanged(string value)
        {
            _settingsService.Settings.PluginMarketUrl = value;
            _ = _settingsService.SaveSettingsAsync();
        }

        // 安全检测自动保存
        partial void OnEnableSecurityScanChanged(bool value)
        {
            var secService = DependencyInjection.ServiceContainer.GetService<PluginSecurityService>();
            if (secService != null) secService.Enabled = value;
        }

        // 搜索历史自动保存
        partial void OnEnableSearchHistoryChanged(bool value)
        {
            _settingsService.Settings.EnableSearchHistory = value;
            _ = _settingsService.SaveSettingsAsync();
        }

        // 同步字段变更自动保存
        partial void OnSyncEnabledChanged(bool value) => _ = SaveSyncSettingsAsync();
        partial void OnAutoSyncChanged(bool value) => _ = SaveSyncSettingsAsync();
        partial void OnSyncRepoOwnerChanged(string value) => _ = SaveSyncSettingsAsync();
        partial void OnSyncRepoNameChanged(string value) => _ = SaveSyncSettingsAsync();
        partial void OnSyncTokenChanged(string value) => _ = SaveSyncSettingsAsync();
        partial void OnSyncBranchChanged(string value) => _ = SaveSyncSettingsAsync();

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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                StatusMessage = $"选择文件夹失败: {ex.Message}";
            }
        }

        [RelayCommand]
        private void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"无法打开链接: {ex.Message}";
            }
        }

        // ===== 用户数据管理 =====

        [RelayCommand]
        private void RefreshUserData()
        {
            UserDataFiles.Clear();
            var dataDir = System.IO.Path.Combine(AppContext.BaseDirectory, "Data");
            if (!System.IO.Directory.Exists(dataDir)) return;

            foreach (var file in System.IO.Directory.GetFiles(dataDir, "*.json"))
            {
                var info = new System.IO.FileInfo(file);
                string category = System.IO.Path.GetFileNameWithoutExtension(file) switch
                {
                    "settings" => "应用设置",
                    "dashboard_layout" => "仪表盘布局",
                    var n when n.StartsWith("plugin_") => "插件设置",
                    _ => "其他数据"
                };
                UserDataFiles.Add(new UserDataFileInfo
                {
                    FileName = System.IO.Path.GetFileName(file),
                    FilePath = file,
                    Category = category,
                    SizeBytes = info.Length,
                    LastModified = info.LastWriteTime
                });
            }
            StatusMessage = $"已加载 {UserDataFiles.Count} 个数据文件";
        }

        [RelayCommand]
        private void ViewFileContent(UserDataFileInfo? file)
        {
            if (file == null || !System.IO.File.Exists(file.FilePath)) return;
            try
            {
                file.Content = System.IO.File.ReadAllText(file.FilePath);
                file.IsContentVisible = !file.IsContentVisible;
            }
            catch (Exception ex)
            {
                StatusMessage = $"读取失败: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task DeleteDataFileAsync(UserDataFileInfo? file)
        {
            if (file == null) return;
            try
            {
                if (System.IO.File.Exists(file.FilePath))
                    System.IO.File.Delete(file.FilePath);
                UserDataFiles.Remove(file);
                StatusMessage = $"已删除: {file.FileName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"删除失败: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// 用户数据文件信息
    /// </summary>
    public partial class UserDataFileInfo : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string Category { get; set; } = "";
        public long SizeBytes { get; set; }
        public DateTime LastModified { get; set; }

        public string SizeDisplay => SizeBytes switch
        {
            >= 1024 => $"{SizeBytes / 1024.0:F1} KB",
            _ => $"{SizeBytes} B"
        };

        private bool _isContentVisible;
        public bool IsContentVisible { get => _isContentVisible; set => SetProperty(ref _isContentVisible, value); }

        private string? _content;
        public string? Content { get => _content; set => SetProperty(ref _content, value); }
    }
}
