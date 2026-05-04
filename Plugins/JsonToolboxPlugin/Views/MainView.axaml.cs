using Avalonia.Controls;
using Avalonia.Input;
using JsonToolboxPlugin.ViewModels;

namespace JsonToolboxPlugin
{
    public partial class MainView : UserControl
    {
        private MainViewModel? _viewModel;

        public MainView()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // 注册快捷键
            var keyBinding = new KeyBinding
            {
                Gesture = new KeyGesture(Key.Enter, KeyModifiers.Control),
                Command = _viewModel.FormatJsonCommand
            };
            KeyBindings.Add(keyBinding);
        }

        public void Dispose()
        {
            _viewModel?.Dispose();
            _viewModel = null;
        }
    }
}
