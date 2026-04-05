using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using AvaloniaApplication2.Core;

namespace AvaloniaApplication2.ViewModels
{
    /// <summary>
    /// 插件包装器视图模型 - 用于在主窗口中显示插件
    /// </summary>
    public partial class PluginWrapperViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string pluginName;

        [ObservableProperty]
        private Control pluginContent;

        [ObservableProperty]
        private bool hasSettingsView;

        private readonly string pluginId;
        private readonly IPlugin? plugin;
        private readonly MainWindowViewModel mainWindowVM;

        public PluginWrapperViewModel(string pluginId, string pluginName, Control pluginContent, IPlugin? plugin, MainWindowViewModel mainWindowVM)
        {
            this.pluginId = pluginId;
            this.PluginName = pluginName;
            this.PluginContent = pluginContent;
            this.plugin = plugin;
            this.mainWindowVM = mainWindowVM;
            
            // 检查是否有设置视图
            this.HasSettingsView = plugin?.GetSettingsView() != null;
        }

        /// <summary>
        /// 打开为独立窗口
        /// </summary>
        [RelayCommand]
        private async Task OpenAsWindowAsync()
        {
            await mainWindowVM.OpenPluginAsWindowAsync(pluginId);
        }

        /// <summary>
        /// 打开插件设置
        /// </summary>
        [RelayCommand]
        private void OpenSettings()
        {
            if (plugin != null)
            {
                var settingsView = plugin.GetSettingsView();
                if (settingsView != null)
                {
                    mainWindowVM.ShowPluginSettingsView(pluginId, plugin.Name, settingsView);
                }
            }
        }

        /// <summary>
        /// 关闭插件
        /// </summary>
        [RelayCommand]
        private void ClosePlugin()
        {
            mainWindowVM.ClosePluginView(pluginId);
        }
    }
}
