using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace AvaloniaApplication2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 设置初始窗口状态
            this.ExtendClientAreaToDecorationsHint = true;
            this.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
            this.ExtendClientAreaTitleBarHeightHint = 40;

            // 添加默认插件信息
            UpdatePluginInfo("未选择插件", "请从左侧列表中选择一个插件以查看详细信息。");
        }

        #region 标题栏事件处理
        // 标题栏拖拽
        private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        // 最小化按钮
        private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // 最大化/还原按钮
        private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                if (MaximizeButton != null)
                    MaximizeButton.Content = "口";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                if (MaximizeButton != null)
                    MaximizeButton.Content = "❐";
            }
        }

        // 关闭按钮
        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region 导航按钮事件处理
        // 仪表盘按钮
        private void DashboardButton_Click(object? sender, RoutedEventArgs e)
        {
            // 清除插件选择
            if (PluginListBox != null)
                PluginListBox.SelectedItem = null;

            // 显示仪表盘内容
            ShowDashboard();

            // 更新信息面板
            UpdatePluginInfo("仪表盘", "系统概览和统计数据");
        }

        // 插件选择变化
        private void PluginListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (PluginListBox?.SelectedItem is ListBoxItem selectedItem)
            {
                if (selectedItem.Content is StackPanel stackPanel &&
                    stackPanel.Children.Count > 1 &&
                    stackPanel.Children[1] is TextBlock textBlock)
                {
                    string pluginName = textBlock.Text ?? "未知插件";
                    ShowPluginContent(pluginName);
                    UpdatePluginInfo(pluginName, GetPluginDescription(pluginName));
                }
            }
        }
        #endregion

        #region 内容显示方法
        // 显示仪表盘
        private void ShowDashboard()
        {
            if (ContentArea == null) return;

            var dashboardPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 20
            };

            var title = new TextBlock
            {
                Text = "仪表盘",
                Foreground = Brushes.White,
                FontSize = 24,
                FontWeight = FontWeight.Bold
            };

            var stats = new StackPanel
            {
                Spacing = 10
            };

            stats.Children.Add(CreateStatItem("已安装插件", "3"));
            stats.Children.Add(CreateStatItem("运行中插件", "2"));
            stats.Children.Add(CreateStatItem("系统资源占用", "15%"));

            dashboardPanel.Children.Add(title);
            dashboardPanel.Children.Add(stats);

            ContentArea.Child = dashboardPanel;
        }

        // 显示插件内容
        private void ShowPluginContent(string pluginName)
        {
            if (ContentArea == null) return;

            StackPanel contentPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 20
            };

            var title = new TextBlock
            {
                Text = pluginName,
                Foreground = Brushes.White,
                FontSize = 24,
                FontWeight = FontWeight.Bold
            };

            contentPanel.Children.Add(title);

            // 根据插件名称显示不同内容
            switch (pluginName)
            {
                case "时钟插件":
                    var timeText = new TextBlock
                    {
                        Foreground = Brushes.White,
                        FontSize = 48,
                        FontWeight = FontWeight.Bold
                    };

                    var dateText = new TextBlock
                    {
                        Foreground = Brushes.LightGray,
                        FontSize = 16
                    };

                    // 更新时间
                    UpdateTime();

                    // 启动定时器更新时钟
                    var timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(1)
                    };
                    timer.Tick += (s, e) => UpdateTime();
                    timer.Start();

                    void UpdateTime()
                    {
                        timeText.Text = DateTime.Now.ToString("HH:mm:ss");
                        dateText.Text = DateTime.Now.ToString("yyyy年MM月dd日 dddd");
                    }

                    contentPanel.Children.Add(timeText);
                    contentPanel.Children.Add(dateText);
                    break;

                case "记事本插件":
                    var textBox = new TextBox
                    {
                        Width = 400,
                        Height = 300,
                        AcceptsReturn = true,
                        TextWrapping = TextWrapping.Wrap,
                        Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                        Foreground = Brushes.White,
                        BorderBrush = Brushes.Gray
                    };

                    contentPanel.Children.Add(textBox);
                    break;

                case "天气预报":
                    var weatherPanel = new StackPanel
                    {
                        Spacing = 10
                    };

                    weatherPanel.Children.Add(new TextBlock
                    {
                        Text = "北京",
                        Foreground = Brushes.White,
                        FontSize = 20
                    });

                    weatherPanel.Children.Add(new TextBlock
                    {
                        Text = "25°C 晴朗",
                        Foreground = Brushes.LightBlue,
                        FontSize = 16
                    });

                    contentPanel.Children.Add(weatherPanel);
                    break;

                default:
                    var defaultText = new TextBlock
                    {
                        Text = $"插件 [{pluginName}] 加载中...",
                        Foreground = Brushes.Gray,
                        FontSize = 16
                    };
                    contentPanel.Children.Add(defaultText);
                    break;
            }

            ContentArea.Child = contentPanel;
        }

        // 更新插件信息
        private void UpdatePluginInfo(string title, string description)
        {
            if (PluginInfoContent == null) return;

            PluginInfoContent.Children.Clear();

            PluginInfoContent.Children.Add(new TextBlock
            {
                Text = title,
                Foreground = Brushes.White,
                FontSize = 18,
                FontWeight = FontWeight.Bold
            });

            PluginInfoContent.Children.Add(new Separator
            {
                Background = Brushes.Gray,
                Margin = new Thickness(0, 5, 0, 10)
            });

            PluginInfoContent.Children.Add(new TextBlock
            {
                Text = description,
                Foreground = new SolidColorBrush(Color.FromRgb(133, 133, 133)),
                TextWrapping = TextWrapping.Wrap
            });
        }

        // 获取插件描述
        private string GetPluginDescription(string pluginName)
        {
            return pluginName switch
            {
                "时钟插件" => "实时显示当前时间和日期。支持多种时间格式和时区显示。",
                "记事本插件" => "简单的文本编辑器。支持基本的文本编辑和保存功能。",
                "天气预报" => "显示当前天气情况和未来几天的天气预报。",
                "仪表盘" => "系统概览和统计数据。",
                _ => "这是一个桌面插件，点击启用后即可使用。"
            };
        }

        // 创建统计项
        private StackPanel CreateStatItem(string label, string value)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 20
            };

            panel.Children.Add(new TextBlock
            {
                Text = label + ":",
                Foreground = Brushes.Gray,
                Width = 100
            });

            panel.Children.Add(new TextBlock
            {
                Text = value,
                Foreground = Brushes.White,
                FontWeight = FontWeight.Bold
            });

            return panel;
        }
        #endregion
    }
}