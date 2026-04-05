using Avalonia.Controls;
using WorldClockPlugin.ViewModels;

namespace WorldClockPlugin
{
    public partial class WorldClockView : UserControl
    {
        private WorldClockViewModel? _viewModel;

        public WorldClockView()
        {
            InitializeComponent();
            _viewModel = DataContext as WorldClockViewModel;
        }

        public void Dispose()
        {
            _viewModel?.Dispose();
        }
    }
}
