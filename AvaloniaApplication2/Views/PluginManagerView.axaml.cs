using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaApplication2.ViewModels;
using System;
using System.Linq;

namespace AvaloniaApplication2.Views
{
    public partial class PluginManagerView : UserControl
    {
        private int _dragCount;

        public PluginManagerView()
        {
            InitializeComponent();

            this.AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            this.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            this.AddHandler(DragDrop.DropEvent, OnDrop);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is PluginManagerViewModel vm)
            {
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

        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            if (!e.Data.Contains(Avalonia.Input.DataFormats.Files))
                return;

            var files = e.Data.GetFiles();
            if (files == null || !files.Any(f =>
                f.Path.LocalPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                return;

            // 使用 Opacity 而非 IsVisible 避免触发布局重算
            if (_dragCount == 0)
                DragOverlay.Opacity = 1;

            _dragCount++;
        }

        private void OnDragLeave(object? sender, DragEventArgs e)
        {
            _dragCount--;
            if (_dragCount <= 0)
            {
                _dragCount = 0;
                DragOverlay.Opacity = 0;
            }
        }

        private async void OnDrop(object? sender, DragEventArgs e)
        {
            _dragCount = 0;
            DragOverlay.Opacity = 0;

            if (!e.Data.Contains(Avalonia.Input.DataFormats.Files))
                return;

            var files = e.Data.GetFiles();
            if (files == null) return;

            var dllFiles = files
                .Select(f => f.Path.LocalPath)
                .Where(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (dllFiles.Any() && DataContext is PluginManagerViewModel vm)
            {
                // 立即异步处理，不阻塞 UI 线程
                _ = vm.OnDropAsync(dllFiles);
            }
        }
    }
}
