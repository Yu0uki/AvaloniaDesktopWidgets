using Avalonia.Controls;
using NotepadPlugin.ViewModels;

namespace NotepadPlugin
{
    /// <summary>
    /// 记事本设置视图
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }
    }
}
