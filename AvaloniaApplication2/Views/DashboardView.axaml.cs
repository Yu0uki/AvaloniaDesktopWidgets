using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaApplication2.ViewModels;
using System;

namespace AvaloniaApplication2.Views
{
    public partial class DashboardView : UserControl
    {
        private DispatcherTimer? _clipboardTimer;
        private string? _lastClipboardContent;

        private DashboardViewModel? VM => DataContext as DashboardViewModel;

        public DashboardView()
        {
            InitializeComponent();
            Loaded += (s, e) => StartClipboardMonitoring();
            DataContextChanged += (s, e) =>
            {
                var vm = VM;
                if (vm != null)
                {
                    vm.PropertyChanged += OnVmPropertyChanged;
                }
            };
        }

        private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 填充高度通过 XAML 绑定自动更新
        }

        private void StartClipboardMonitoring()
        {
            _clipboardTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _clipboardTimer.Tick += async (s, e) => await CheckClipboardAsync();
            _clipboardTimer.Start();
        }

        private async System.Threading.Tasks.Task CheckClipboardAsync()
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel?.Clipboard is { } clipboard)
                {
                    var text = await clipboard.GetTextAsync();
                    if (!string.IsNullOrEmpty(text) && text != _lastClipboardContent)
                    {
                        _lastClipboardContent = text;
                        VM?.AddClipboardItem(text);
                    }
                }
            }
            catch { }
        }

        private void OnQuickAppTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not StyledElement el) return;
            if (el.DataContext is QuickAppInfo app)
                VM?.LaunchQuickAppCommand.Execute(app);
        }

        private async void OnClipboardItemTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not StyledElement el) return;
            if (el.DataContext is not ClipboardItem item) return;
            try
            {
                var tl = TopLevel.GetTopLevel(this);
                if (tl?.Clipboard is { } cb)
                    await cb.SetTextAsync(item.Content);
            }
            catch { }
        }
    }
}
