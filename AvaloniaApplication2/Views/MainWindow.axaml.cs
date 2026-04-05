using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaApplication2.ViewModels;
using System;
using System.Linq;

namespace AvaloniaApplication2
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // 设置初始窗口状态
            this.ExtendClientAreaToDecorationsHint = true;
            this.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
            this.ExtendClientAreaTitleBarHeightHint = 35;
            
            // 启用拖拽事件处理
            this.AddHandler(DragDrop.DropEvent, OnDrop);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            _viewModel = DataContext as MainWindowViewModel;
            
            // 注册全局拖拽事件
            this.AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            this.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            this.AddHandler(DragDrop.DropEvent, OnDrop);
        }

        #region 标题栏事件处理
        
        private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                if (MaximizeButton != null)
                    MaximizeButton.Content = "☐";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                if (MaximizeButton != null)
                    MaximizeButton.Content = "❐";
            }
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
        
        #endregion

        #region 拖拽处理

        private async void OnDragEnter(object? sender, DragEventArgs e)
        {
            // 检查是否包含文件
            if (e.Data.Contains(Avalonia.Input.DataFormats.Files))
            {
                var files = e.Data.GetFiles();
                if (files != null && files.Any(f => f.Path.LocalPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                {
                    e.DragEffects = Avalonia.Input.DragDropEffects.Copy;
                    
                    // 如果当前是插件管理器页面，触发视觉反馈
                    if (_viewModel?.CurrentPage is PluginManagerViewModel pluginVM)
                    {
                        pluginVM.OnDragEnter();
                    }
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
            // 如果当前是插件管理器页面，清除视觉反馈
            if (_viewModel?.CurrentPage is PluginManagerViewModel pluginVM)
            {
                pluginVM.OnDragLeave();
            }
        }

        private async void OnDrop(object? sender, DragEventArgs e)
        {
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
                        // 如果当前是插件管理器页面，直接处理
                        if (_viewModel?.CurrentPage is PluginManagerViewModel pluginManagerVM)
                        {
                            await pluginManagerVM.OnDropAsync(dllFiles);
                        }
                        else
                        {
                            // 否则切换到插件管理页面并处理
                            _viewModel?.NavigateToPluginManager();
                            
                            // 等待页面切换后处理文件
                            if (_viewModel?.CurrentPage is PluginManagerViewModel newPluginVM)
                            {
                                await System.Threading.Tasks.Task.Delay(100); // 短暂延迟确保页面加载
                                await newPluginVM.OnDropAsync(dllFiles);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}