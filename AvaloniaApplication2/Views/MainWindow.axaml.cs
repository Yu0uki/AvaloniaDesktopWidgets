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

        private async void OnDrop(object? sender, DragEventArgs e)
        {
            if (_viewModel?.CurrentPage is PluginManagerViewModel pluginManagerVM)
            {
                var dataTransfer = e.Data;
                if (dataTransfer.Contains(Avalonia.Input.DataFormats.Files))
                {
                    var files = dataTransfer.GetFiles();
                    if (files != null)
                    {
                        var filePaths = files.Select(f => f.Path.LocalPath).ToList();
                        await pluginManagerVM.OnDropAsync(filePaths);
                    }
                }
            }
        }

        #endregion
    }
}