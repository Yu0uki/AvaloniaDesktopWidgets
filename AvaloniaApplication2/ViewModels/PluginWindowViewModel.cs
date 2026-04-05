using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApplication2.ViewModels
{
    /// <summary>
    /// 插件独立窗口视图模型
    /// </summary>
    public partial class PluginWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string pluginId = "";

        [ObservableProperty]
        private string pluginName = "";

        [ObservableProperty]
        private Control? pluginContent;

        public MainWindowViewModel? MainWindowVM { get; set; }
    }
}
