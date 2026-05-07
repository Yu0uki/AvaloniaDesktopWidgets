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

        public DashboardView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, EventArgs e)
        {
            StartClipboardMonitoring();
        }

        private void StartClipboardMonitoring()
        {
            _clipboardTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _clipboardTimer.Tick += async (s, e) => await CheckClipboard();
            _clipboardTimer.Start();
        }

        private async System.Threading.Tasks.Task CheckClipboard()
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel?.Clipboard is { } clipboard)
                {
#pragma warning disable CS0618
                    var text = await clipboard.GetTextAsync();
#pragma warning restore CS0618
                    if (!string.IsNullOrEmpty(text) && text != _lastClipboardContent)
                    {
                        _lastClipboardContent = text;
                        if (DataContext is DashboardViewModel vm)
                        {
                            vm.AddClipboardItem(text);
                        }
                    }
                }
            }
            catch
            {
                // 剪贴板访问可能因权限问题失败，忽略异常
            }
        }

        private void OnQuickAppTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not Control control) return;
            if (control.DataContext is QuickAppInfo app && DataContext is DashboardViewModel vm)
            {
                vm.LaunchQuickAppCommand.Execute(app);
            }
        }

        private async void OnClipboardItemTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not Control control)
                return;

            if (control.DataContext is not ClipboardItem item)
                return;

            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel?.Clipboard is { } clipboard)
                {
#pragma warning disable CS0618
                    await clipboard.SetTextAsync(item.Content);
#pragma warning restore CS0618
                }
            }
            catch
            {
                // 剪贴板写入可能失败
            }
        }
    }
}
