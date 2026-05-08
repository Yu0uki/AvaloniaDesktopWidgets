using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApplication2.ViewModels
{
    public partial class PluginWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string pluginId = "";

        [ObservableProperty]
        private string pluginName = "";

        [ObservableProperty]
        private Control? pluginContent;

        [ObservableProperty]
        private MainWindowViewModel? mainWindowVM;

        private bool _disposed;

        /// <summary>
        /// P0: 断开 Visual Tree 引用，防止内存泄漏
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            PluginContent = null;
            MainWindowVM = null;
        }
    }
}
