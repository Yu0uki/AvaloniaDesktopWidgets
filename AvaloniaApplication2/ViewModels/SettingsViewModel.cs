using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Linq;

namespace AvaloniaApplication2.ViewModels
{
    /// <summary>
    /// 设置视图模型
    /// </summary>
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;

        [ObservableProperty]
        private int selectedThemeIndex = 0; // 0=Dark, 1=Light, 2=System

        [ObservableProperty]
        private bool autoLoadPlugins;

        [ObservableProperty]
        private string pluginsDirectory;

        [ObservableProperty]
        private string appVersion = "1.0.0";

        [ObservableProperty]
        private string statusMessage = "";

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
            
            // 加载当前设置
            SelectedThemeIndex = settingsService.Settings.Theme.ToLower() switch
            {
                "light" => 1,
                "system" => 2,
                _ => 0 // dark
            };
            AutoLoadPlugins = settingsService.Settings.AutoLoadPlugins;
            PluginsDirectory = settingsService.Settings.PluginsDirectory;
        }

        /// <summary>
        /// 保存主题设置并应用
        /// </summary>
        [RelayCommand]
        private async Task SaveThemeAsync()
        {
            // 根据索引转换为字符串
            string themeValue = SelectedThemeIndex switch
            {
                1 => "Light",
                2 => "System",
                _ => "Dark"
            };
            
            await _settingsService.UpdateThemeAsync(themeValue);
            ApplyTheme(themeValue);
            StatusMessage = $"主题已切换为: {themeValue}";
        }

        /// <summary>
        /// 应用主题到应用程序
        /// </summary>
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

        /// <summary>
        /// 保存自动加载设置
        /// </summary>
        [RelayCommand]
        private async Task SaveAutoLoadAsync()
        {
            await _settingsService.UpdateAutoLoadPluginsAsync(AutoLoadPlugins);
            StatusMessage = "设置已保存";
        }

        /// <summary>
        /// 保存插件目录
        /// </summary>
        [RelayCommand]
        private async Task SavePluginsDirectoryAsync()
        {
            await _settingsService.UpdatePluginsDirectoryAsync(PluginsDirectory);
            StatusMessage = "插件目录已更新";
        }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        [RelayCommand]
        private async Task SelectFolderAsync()
        {
            try
            {
                // 获取主窗口以显示对话框
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
