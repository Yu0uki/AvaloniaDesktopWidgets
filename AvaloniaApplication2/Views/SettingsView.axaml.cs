using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaApplication2.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void ScrollToElement(Control target)
        {
            try
            {
                var content = SettingsScrollViewer.Content as Control;
                if (content == null) return;
                var offset = target.TranslatePoint(new Point(0, 0), content);
                if (offset.HasValue)
                {
                    var y = offset.Value.Y - 20;
                    if (y < 0) y = 0;
                    SettingsScrollViewer.Offset = new Vector(SettingsScrollViewer.Offset.X, y);
                }
            }
            catch { /* 滚动失败静默处理 */ }
        }

        private void OnGeneralTabClick(object? sender, RoutedEventArgs e) => ScrollToElement(GeneralSectionHeader);
        private void OnAppearanceTabClick(object? sender, RoutedEventArgs e) => ScrollToElement(AppearanceSectionHeader);
        private void OnPluginTabClick(object? sender, RoutedEventArgs e) => ScrollToElement(PluginSectionHeader);
        private void OnSyncTabClick(object? sender, RoutedEventArgs e) => ScrollToElement(SyncSectionHeader);
        private void OnUserDataTabClick(object? sender, RoutedEventArgs e) => ScrollToElement(UserDataSectionHeader);
        private void OnAdvancedTabClick(object? sender, RoutedEventArgs e) => ScrollToElement(AdvancedSectionHeader);
        private void OnAboutTabClick(object? sender, RoutedEventArgs e) => ScrollToElement(AboutSectionHeader);
    }
}
