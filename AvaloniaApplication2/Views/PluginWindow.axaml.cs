using Avalonia.Controls;
using AvaloniaApplication2.Services;
using AvaloniaApplication2.DependencyInjection;

namespace AvaloniaApplication2.Views
{
    public partial class PluginWindow : Window
    {
        public PluginWindow()
        {
            InitializeComponent();

            // P0: 窗口关闭时停用插件，防止泄漏
            this.Closed += (s, e) =>
            {
                if (DataContext is ViewModels.PluginWindowViewModel vm)
                {
                    var pluginManager = ServiceContainer.GetService<PluginManager>();
                    pluginManager?.DeactivatePlugin(vm.PluginId);
                    vm.Dispose();
                }
            };
        }
    }
}
