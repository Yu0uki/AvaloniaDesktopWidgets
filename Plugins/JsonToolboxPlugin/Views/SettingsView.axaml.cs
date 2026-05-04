using Avalonia.Controls;
using JsonToolboxPlugin.ViewModels;

namespace JsonToolboxPlugin
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }
    }
}
