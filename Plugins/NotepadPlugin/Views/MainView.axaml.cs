using Avalonia.Controls;
using NotepadPlugin.ViewModels;

namespace NotepadPlugin
{
    /// <summary>
    /// 记事本主视图
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
