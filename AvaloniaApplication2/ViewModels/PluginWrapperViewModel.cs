using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

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

        private readonly string pluginId;
        private readonly MainWindowViewModel mainWindowVM;

        public PluginWrapperViewModel(string pluginId, string pluginName, Control pluginContent, MainWindowViewModel mainWindowVM)
        {
            this.pluginId = pluginId;
            this.PluginName = pluginName;
            this.PluginContent = pluginContent;
            this.mainWindowVM = mainWindowVM;
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
        /// 关闭插件
        /// </summary>
        [RelayCommand]
        private void ClosePlugin()
        {
            mainWindowVM.ClosePluginView(pluginId);
        }
    }
}
