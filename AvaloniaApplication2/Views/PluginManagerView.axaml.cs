using Avalonia.Controls;
using AvaloniaApplication2.ViewModels;
using System;

namespace AvaloniaApplication2.Views
{
    public partial class PluginManagerView : UserControl
    {
        public PluginManagerView()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            
            if (DataContext is PluginManagerViewModel vm)
            {
                // 监听插件集合变化
                vm.Plugins.CollectionChanged += (s, args) => UpdateEmptyState();
                UpdateEmptyState();
            }
        }

        private void UpdateEmptyState()
        {
            if (DataContext is PluginManagerViewModel vm)
            {
                EmptyStateBorder.IsVisible = vm.Plugins.Count == 0;
            }
        }
    }
}
