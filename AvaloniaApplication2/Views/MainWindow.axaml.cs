using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaApplication2.ViewModels;
using System;
using System.Linq;

namespace AvaloniaApplication2
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;
        private bool _isDragOver;
        private TextBox? _searchBox;

        public MainWindow()
        {
            InitializeComponent();
            
            // 启用拖拽事件处理
            this.AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            this.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            this.AddHandler(DragDrop.DropEvent, OnDrop);
            
            // 监听 DataContext 变化
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            // 取消之前的事件订阅（如果存在）
            if (_viewModel != null)
            {
                _viewModel.WindowMinimizeRequested -= OnWindowMinimizeRequested;
                _viewModel.WindowMaximizeRequested -= OnWindowMaximizeRequested;
                _viewModel.WindowCloseRequested -= OnWindowCloseRequested;
                _viewModel.FocusSearchRequested -= OnFocusSearchRequested;
            }

            _viewModel = DataContext as MainWindowViewModel;
            
            if (_viewModel != null)
            {
                // 订阅窗口控制事件
                _viewModel.WindowMinimizeRequested += OnWindowMinimizeRequested;
                _viewModel.WindowMaximizeRequested += OnWindowMaximizeRequested;
                _viewModel.WindowCloseRequested += OnWindowCloseRequested;
                _viewModel.FocusSearchRequested += OnFocusSearchRequested;
            }
            
            // 查找搜索框并绑定快捷键
            if (_searchBox == null)
            {
                // 延迟查找搜索框
                Dispatcher.UIThread.Post(() =>
                {
                    _searchBox = this.FindDescendantOfType<TextBox>();
                }, DispatcherPriority.Loaded);
            }
        }

        private void OnWindowMinimizeRequested(object? sender, EventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnWindowMaximizeRequested(object? sender, ViewModels.WindowStateEventArgs e)
        {
            WindowState = e.IsMaximized ? WindowState.Maximized : WindowState.Normal;
        }

        private void OnWindowCloseRequested(object? sender, EventArgs e)
        {
            Close();
        }
        
        private void OnFocusSearchRequested(object? sender, EventArgs e)
        {
            _searchBox?.Focus();
            _searchBox?.SelectAll();
        }

        /// <summary>
        /// 顶部栏鼠标按下事件 - 用于拖动窗口
        /// </summary>
        private void OnTopBarPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            // 只有在左键点击且没有点击按钮时才拖动
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                // 检查是否点击了按钮或其他交互元素
                var source = e.Source as Avalonia.Visual;
                if (source != null)
                {
                    // 如果点击的是按钮，不拖动
                    if (source.FindAncestorOfType<Button>() != null)
                    {
                        return;
                    }
                }
                
                // 开始拖动窗口
                BeginMoveDrag(e);
            }
        }

        #region 拖拽处理

        private async void OnDragEnter(object? sender, DragEventArgs e)
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
                    
                    // 如果当前是插件管理器页面，触发视觉反馈
                    if (_viewModel?.CurrentPage is PluginManagerViewModel pluginVM)
                    {
                        _isDragOver = true;
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
            if (_isDragOver && _viewModel?.CurrentPage is PluginManagerViewModel pluginVM)
            {
                _isDragOver = false;
                pluginVM.OnDragLeave();
            }
        }

        private async void OnDrop(object? sender, DragEventArgs e)
        {
            // 重置拖拽状态
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