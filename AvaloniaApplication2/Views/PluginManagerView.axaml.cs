using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaApplication2.ViewModels;
using System;
using System.Linq;

namespace AvaloniaApplication2.Views
{
    public partial class PluginManagerView : UserControl
    {
        private bool _isDragOver;

        public PluginManagerView()
        {
            InitializeComponent();
            
            // 注册拖拽事件
            this.AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            this.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            this.AddHandler(DragDrop.DropEvent, OnDrop);
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

        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            // 防止重复触发
            if (_isDragOver) return;

            // 检查是否包含文件
            if (e.Data.Contains(Avalonia.Input.DataFormats.Files))
            {
                var files = e.Data.GetFiles();
                if (files != null && files.Any(f => f.Path.LocalPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                {
                    e.DragEffects = Avalonia.Input.DragDropEffects.Copy;
                    
                    // 直接显示覆盖层，不通过 ViewModel
                    _isDragOver = true;
                    DragOverlay.IsVisible = true;
                }
                else
                {
                    e.DragEffects = Avalonia.Input.DragDropEffects.None;
                }
            }
            else
            {
                e.DragEffects = Avalonia.Input.DragDropEffects.None;
            }
        }

        private void OnDragLeave(object? sender, DragEventArgs e)
        {
            // 清除视觉反馈
            if (_isDragOver)
            {
                _isDragOver = false;
                DragOverlay.IsVisible = false;
            }
        }

        private async void OnDrop(object? sender, DragEventArgs e)
        {
            // 隐藏覆盖层
            DragOverlay.IsVisible = false;
            _isDragOver = false;

            var dataObject = e.Data;
            if (dataObject.Contains(Avalonia.Input.DataFormats.Files))
            {
                var files = dataObject.GetFiles();
                if (files != null)
                {
                    var filePaths = files.Select(f => f.Path.LocalPath).ToList();
                    var dllFiles = filePaths.Where(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)).ToList();
                    
                    if (dllFiles.Any())
                    {
                        if (DataContext is PluginManagerViewModel pluginManagerVM)
                        {
                            await pluginManagerVM.OnDropAsync(dllFiles);
                        }
                    }
                }
            }
        }
    }
}
