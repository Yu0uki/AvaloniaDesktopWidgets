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

        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            if (_isDragOver) return;
            _isDragOver = true;

            if (e.Data.Contains(Avalonia.Input.DataFormats.Files))
            {
                var files = e.Data.GetFiles();
                if (files != null && files.Any(f => f.Path.LocalPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                {
                    e.DragEffects = DragDropEffects.Copy;
                }
                else
                {
                    e.DragEffects = DragDropEffects.None;
                }
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
        }

        private void OnDragLeave(object? sender, DragEventArgs e)
        {
            _isDragOver = false;
        }

        private async void OnDrop(object? sender, DragEventArgs e)
        {
            _isDragOver = false;

            // 如果当前已在插件管理页面，不再处理（由 PluginManagerView 处理）
            if (_viewModel?.CurrentPage is PluginManagerViewModel)
                return;

            if (!e.Data.Contains(Avalonia.Input.DataFormats.Files))
                return;

            var files = e.Data.GetFiles();
            if (files == null) return;

            var dllFiles = files
                .Select(f => f.Path.LocalPath)
                .Where(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!dllFiles.Any()) return;

            // 不在插件管理页面时，先跳转再处理
            _viewModel?.NavigateToPluginManager();
            await System.Threading.Tasks.Task.Delay(150);

            if (_viewModel?.CurrentPage is PluginManagerViewModel newPluginVM)
            {
                await newPluginVM.OnDropAsync(dllFiles);
            }
        }

        /// <summary>
        /// 搜索框按键事件：回车触发网页搜索
        /// </summary>
        private void OnSearchBoxKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _viewModel != null)
            {
                e.Handled = true;
                _viewModel.WebSearchCommand.Execute(null);
            }
        }

        #endregion
    }
}