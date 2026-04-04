using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace AvaloniaApplication2.ViewModels
{
    /// <summary>
    /// 设置视图模型
    /// </summary>
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;

        [ObservableProperty]
        private string selectedTheme;

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
            SelectedTheme = settingsService.Settings.Theme;
            AutoLoadPlugins = settingsService.Settings.AutoLoadPlugins;
            PluginsDirectory = settingsService.Settings.PluginsDirectory;
        }

        /// <summary>
        /// 保存主题设置
        /// </summary>
        [RelayCommand]
        private async Task SaveThemeAsync()
        {
            await _settingsService.UpdateThemeAsync(SelectedTheme);
            StatusMessage = "主题设置已保存";
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
        private void SelectFolder()
        {
            // TODO: 实现文件夹选择对话框
            StatusMessage = "请使用文件对话框选择文件夹（功能待实现）";
        }
    }
}
