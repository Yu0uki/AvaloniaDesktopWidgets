using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaApplication2.ViewModels;
using AvaloniaApplication2.Models;
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

            this.AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            this.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            this.AddHandler(DragDrop.DropEvent, OnDrop);

            this.DataContextChanged += OnDataContextChanged;

            // 窗口位置变化时保存
            this.PositionChanged += OnPositionChanged;
        }

        private async void OnPositionChanged(object? sender, PixelPointEventArgs e)
        {
            if (_viewModel != null && WindowState == WindowState.Normal)
            {
                await _viewModel.SaveWindowPositionAsync(e.Point.X, e.Point.Y);
            }
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
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
                _viewModel.WindowMinimizeRequested += OnWindowMinimizeRequested;
                _viewModel.WindowMaximizeRequested += OnWindowMaximizeRequested;
                _viewModel.WindowCloseRequested += OnWindowCloseRequested;
                _viewModel.FocusSearchRequested += OnFocusSearchRequested;
            }

            if (_searchBox == null)
            {
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
        /// 顶部栏拖动
        /// </summary>
        private void OnTopBarPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                var source = e.Source as Avalonia.Visual;
                if (source != null)
                {
                    if (source.FindAncestorOfType<Button>() != null)
                        return;
                }

                BeginMoveDrag(e);
            }
        }

        /// <summary>
        /// 通知中心条目点击 → 导航到日志页
        /// </summary>
        private void OnNotificationHistoryItemPressed(object? sender, PointerPressedEventArgs e)
        {
            _viewModel?.NavigateToLogsCommand.Execute(null);
            if (_viewModel != null)
                _viewModel.IsNotificationCenterOpen = false;
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

            _viewModel?.NavigateToPluginManager();
            await System.Threading.Tasks.Task.Delay(150);

            if (_viewModel?.CurrentPage is PluginManagerViewModel newPluginVM)
            {
                await newPluginVM.OnDropAsync(dllFiles);
            }
        }

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
