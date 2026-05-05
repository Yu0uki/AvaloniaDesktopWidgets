using AvaloniaApplication2.Core;
using AvaloniaApplication2.Models;
using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace AvaloniaApplication2.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly PluginManager _pluginManager;
        private Timer? _timer;

        [ObservableProperty]
        private int totalPlugins;

        [ObservableProperty]
        private int loadedPlugins;

        [ObservableProperty]
        private int enabledPlugins;

        [ObservableProperty]
        private string recentActivity = "暂无活动";

        // 问候语
        [ObservableProperty]
        private string greeting = "早上好";

        // 时间
        [ObservableProperty]
        private string currentTime = "00:00";

        // 日期
        [ObservableProperty]
        private string currentDate = "";

        // 天气
        [ObservableProperty]
        private string weatherLocation = "北京市";

        [ObservableProperty]
        private string weatherTemperature = "24°C";

        [ObservableProperty]
        private string weatherIcon = "☀️";

        [ObservableProperty]
        private string weatherDescription = "晴朗";

        // 所选城市
        [ObservableProperty]
        private string selectedCity = "北京市";

        // 系统监控
        [ObservableProperty]
        private int cpuUsage = 32;

        [ObservableProperty]
        private int memoryUsage = 84;

        // 新待办事项输入
        [ObservableProperty]
        private string newTodoText = "";

        // 新快捷应用输入
        [ObservableProperty]
        private string newQuickAppText = "";

        // 是否显示添加快捷应用输入框
        [ObservableProperty]
        private bool isAddingQuickApp;

        // 快捷应用列表
        public ObservableCollection<QuickAppInfo> QuickApps { get; } = new();

        // 待办事项列表
        public ObservableCollection<TodoItem> TodoItems { get; } = new();

        // 剪贴板历史
        public ObservableCollection<ClipboardItem> ClipboardHistory { get; } = new();

        // 布局编辑模式
        [ObservableProperty]
        private bool isEditingLayout;

        // 卡片可见性
        [ObservableProperty]
        private bool showWeatherCard = true;

        [ObservableProperty]
        private bool showQuickAppsCard = true;

        [ObservableProperty]
        private bool showSystemMonitorCard = true;

        [ObservableProperty]
        private bool showTodoCard = true;

        [ObservableProperty]
        private bool showClipboardCard = true;

        [ObservableProperty]
        private bool showPluginStatsCard = true;

        // 城市列表
        public ObservableCollection<string> Cities { get; } = new()
        {
            "北京市", "上海市", "广州市", "深圳市", "杭州市",
            "成都市", "武汉市", "南京市", "西安市", "重庆市", "扬州市"
        };

        // 各城市模拟天气数据
        private static readonly Dictionary<string, (string temp, string icon, string desc)> CityWeatherData = new()
        {
            ["北京市"] = ("24°C", "☀️", "晴朗"),
            ["上海市"] = ("26°C", "⛅", "多云"),
            ["广州市"] = ("30°C", "🌤️", "晴转多云"),
            ["深圳市"] = ("29°C", "☀️", "晴朗"),
            ["杭州市"] = ("25°C", "🌦️", "阵雨"),
            ["成都市"] = ("22°C", "☁️", "阴天"),
            ["武汉市"] = ("27°C", "⛅", "多云"),
            ["南京市"] = ("26°C", "🌤️", "晴转多云"),
            ["西安市"] = ("23°C", "☀️", "晴朗"),
            ["重庆市"] = ("28°C", "🌤️", "晴转多云"),
            ["扬州市"] = ("24°C", "☀️", "晴朗"),
        };

        public DashboardViewModel(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;

            _pluginManager.PluginInfos.CollectionChanged += OnPluginCollectionChanged;

            foreach (var plugin in _pluginManager.PluginInfos)
            {
                plugin.PropertyChanged += OnPluginPropertyChanged;
            }

            // 加载持久化数据
            LoadDashboardData();

            UpdateStatistics();
            UpdateGreeting();
            UpdateTime();

            _timer = new Timer(UpdateTimerCallback, null, 1000, 1000);
        }

        // 城市切换时更新天气
        partial void OnSelectedCityChanged(string value)
        {
            WeatherLocation = value;
            if (CityWeatherData.TryGetValue(value, out var weather))
            {
                WeatherTemperature = weather.temp;
                WeatherIcon = weather.icon;
                WeatherDescription = weather.desc;
            }
        }

        private void UpdateTimerCallback(object? state)
        {
            UpdateTime();
            UpdateSystemMonitor();
        }

        private void UpdateTime()
        {
            var now = DateTime.Now;
            CurrentTime = now.ToString("HH:mm");
            UpdateGreeting();

            if (now.Second == 0)
            {
                CurrentDate = now.ToString("yyyy年M月d日 dddd");
            }
        }

        private void UpdateGreeting()
        {
            var hour = DateTime.Now.Hour;
            Greeting = hour switch
            {
                >= 6 and < 12 => "早上好",
                >= 12 and < 14 => "中午好",
                >= 14 and < 18 => "下午好",
                >= 18 and < 22 => "晚上好",
                _ => "夜深了"
            };
        }

        private void UpdateSystemMonitor()
        {
            var random = new Random();
            CpuUsage = Math.Clamp(CpuUsage + random.Next(-5, 6), 10, 90);
            MemoryUsage = Math.Clamp(MemoryUsage + random.Next(-2, 3), 50, 95);
        }

        // ===== 待办事项命令 =====

        [RelayCommand]
        private void AddTodoItem()
        {
            if (string.IsNullOrWhiteSpace(NewTodoText))
                return;

            var item = new TodoItem { Title = NewTodoText.Trim(), IsCompleted = false };
            item.PropertyChanged += OnTodoItemPropertyChanged;
            TodoItems.Add(item);
            NewTodoText = "";
            SaveDashboardData();
        }

        [RelayCommand]
        private void RemoveTodoItem(TodoItem? item)
        {
            if (item != null)
            {
                item.PropertyChanged -= OnTodoItemPropertyChanged;
                TodoItems.Remove(item);
                SaveDashboardData();
            }
        }

        private void OnTodoItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TodoItem.IsCompleted))
            {
                SaveDashboardData();
            }
        }

        // ===== 快捷应用命令 =====

        [RelayCommand]
        private void ToggleAddQuickApp()
        {
            IsAddingQuickApp = !IsAddingQuickApp;
            if (!IsAddingQuickApp)
            {
                NewQuickAppText = "";
            }
        }

        [RelayCommand]
        private void AddQuickApp()
        {
            if (string.IsNullOrWhiteSpace(NewQuickAppText))
                return;

            QuickApps.Add(new QuickAppInfo { Name = NewQuickAppText.Trim(), Icon = "📌" });
            NewQuickAppText = "";
            IsAddingQuickApp = false;
            SaveDashboardData();
        }

        [RelayCommand]
        private void RemoveQuickApp(QuickAppInfo? app)
        {
            if (app != null)
            {
                QuickApps.Remove(app);
                SaveDashboardData();
            }
        }

        // ===== 布局编辑命令 =====

        [RelayCommand]
        private void ToggleLayoutEdit()
        {
            IsEditingLayout = !IsEditingLayout;
        }

        [RelayCommand]
        private void ToggleCard(string cardName)
        {
            switch (cardName)
            {
                case "Weather":
                    ShowWeatherCard = !ShowWeatherCard;
                    break;
                case "QuickApps":
                    ShowQuickAppsCard = !ShowQuickAppsCard;
                    break;
                case "SystemMonitor":
                    ShowSystemMonitorCard = !ShowSystemMonitorCard;
                    break;
                case "Todo":
                    ShowTodoCard = !ShowTodoCard;
                    break;
                case "Clipboard":
                    ShowClipboardCard = !ShowClipboardCard;
                    break;
                case "PluginStats":
                    ShowPluginStatsCard = !ShowPluginStatsCard;
                    break;
            }
            SaveDashboardData();
        }

        // ===== 剪贴板方法（由视图层调用） =====

        public void AddClipboardItem(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            // 去重：如果最新一条内容相同则跳过
            if (ClipboardHistory.Count > 0 && ClipboardHistory[0].Content == content)
                return;

            ClipboardHistory.Insert(0, new ClipboardItem { Content = content, Time = DateTime.Now });

            // 限制历史记录最多 50 条
            while (ClipboardHistory.Count > 50)
                ClipboardHistory.RemoveAt(ClipboardHistory.Count - 1);
        }

        // ===== 数据持久化 =====

        private string GetDataFilePath()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);
            return Path.Combine(dataDir, "dashboard.json");
        }

        private void SaveDashboardData()
        {
            try
            {
                var data = new DashboardData
                {
                    SelectedCity = SelectedCity,
                    TodoItems = TodoItems.Select(t => new DashboardTodoItem
                    {
                        Title = t.Title,
                        IsCompleted = t.IsCompleted
                    }).ToList(),
                    QuickApps = QuickApps.Select(q => new DashboardQuickApp
                    {
                        Name = q.Name,
                        Icon = q.Icon
                    }).ToList(),
                    Layout = new DashboardLayout
                    {
                        ShowWeatherCard = ShowWeatherCard,
                        ShowQuickAppsCard = ShowQuickAppsCard,
                        ShowSystemMonitorCard = ShowSystemMonitorCard,
                        ShowTodoCard = ShowTodoCard,
                        ShowClipboardCard = ShowClipboardCard,
                        ShowPluginStatsCard = ShowPluginStatsCard,
                    }
                };

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                File.WriteAllText(GetDataFilePath(), json);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "保存仪表盘数据失败");
            }
        }

        private void LoadDashboardData()
        {
            try
            {
                var path = GetDataFilePath();
                if (!File.Exists(path))
                {
                    // 首次使用，初始化默认数据
                    InitializeDefaultData();
                    return;
                }

                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<DashboardData>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (data == null)
                {
                    InitializeDefaultData();
                    return;
                }

                // 恢复城市选择
                if (!string.IsNullOrEmpty(data.SelectedCity) && Cities.Contains(data.SelectedCity))
                {
                    SelectedCity = data.SelectedCity;
                    // 手动触发天气更新，因为属性尚未初始化
                    WeatherLocation = data.SelectedCity;
                    if (CityWeatherData.TryGetValue(data.SelectedCity, out var weather))
                    {
                        WeatherTemperature = weather.temp;
                        WeatherIcon = weather.icon;
                        WeatherDescription = weather.desc;
                    }
                }

                // 恢复待办事项
                foreach (var t in data.TodoItems)
                {
                    var item = new TodoItem { Title = t.Title, IsCompleted = t.IsCompleted };
                    item.PropertyChanged += OnTodoItemPropertyChanged;
                    TodoItems.Add(item);
                }

                // 恢复快捷应用
                foreach (var q in data.QuickApps)
                {
                    QuickApps.Add(new QuickAppInfo { Name = q.Name, Icon = q.Icon });
                }

                // 恢复布局设置
                if (data.Layout != null)
                {
                    ShowWeatherCard = data.Layout.ShowWeatherCard;
                    ShowQuickAppsCard = data.Layout.ShowQuickAppsCard;
                    ShowSystemMonitorCard = data.Layout.ShowSystemMonitorCard;
                    ShowTodoCard = data.Layout.ShowTodoCard;
                    ShowClipboardCard = data.Layout.ShowClipboardCard;
                    ShowPluginStatsCard = data.Layout.ShowPluginStatsCard;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "加载仪表盘数据失败");
                InitializeDefaultData();
            }
        }

        private void InitializeDefaultData()
        {
            // 默认城市
            SelectedCity = "北京市";
            WeatherLocation = "北京市";
            WeatherTemperature = "24°C";
            WeatherIcon = "☀️";
            WeatherDescription = "晴朗";

            // 默认待办
            var defaults = new[]
            {
                new { Title = "完成前端原型设计", Completed = true },
                new { Title = "技术栈选型会议", Completed = false },
                new { Title = "编写底层接口", Completed = false }
            };
            foreach (var d in defaults)
            {
                var item = new TodoItem { Title = d.Title, IsCompleted = d.Completed };
                item.PropertyChanged += OnTodoItemPropertyChanged;
                TodoItems.Add(item);
            }

            // 默认快捷应用
            QuickApps.Add(new QuickAppInfo { Name = "终端控制台", Icon = "⌨️" });
            QuickApps.Add(new QuickAppInfo { Name = "代码编辑器", Icon = "📝" });
            QuickApps.Add(new QuickAppInfo { Name = "设计画板", Icon = "🎨" });
            QuickApps.Add(new QuickAppInfo { Name = "数据库管理", Icon = "💾" });
            QuickApps.Add(new QuickAppInfo { Name = "快捷便签", Icon = "📋" });
            QuickApps.Add(new QuickAppInfo { Name = "高级计算器", Icon = "🧮" });
            QuickApps.Add(new QuickAppInfo { Name = "翻译工具", Icon = "🌐" });

            // 默认剪贴板历史
            ClipboardHistory.Add(new ClipboardItem { Content = "npm install @tauri-apps/cli", Time = DateTime.Now.AddMinutes(-5) });
            ClipboardHistory.Add(new ClipboardItem { Content = "https://github.com/tauri-apps...", Time = DateTime.Now.AddMinutes(-15) });
        }

        // ===== 插件事件处理 =====

        private void OnPluginCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is PluginInfo plugin)
                    {
                        plugin.PropertyChanged += OnPluginPropertyChanged;
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is PluginInfo plugin)
                    {
                        plugin.PropertyChanged -= OnPluginPropertyChanged;
                    }
                }
            }

            UpdateStatistics();
        }

        private void OnPluginPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PluginInfo.IsEnabled) ||
                e.PropertyName == nameof(PluginInfo.IsLoaded))
            {
                UpdateStatistics();
            }
        }

        private void UpdateStatistics()
        {
            TotalPlugins = _pluginManager.PluginInfos.Count;
            LoadedPlugins = _pluginManager.PluginInfos.Count(p => p.IsLoaded);
            EnabledPlugins = _pluginManager.PluginInfos.Count(p => p.IsEnabled);

            if (TotalPlugins == 0)
            {
                RecentActivity = "拖拽 DLL 文件到窗口任意位置以添加插件";
            }
            else if (LoadedPlugins == 0)
            {
                RecentActivity = $"已安装 {TotalPlugins} 个插件，点击启用开始使用";
            }
            else
            {
                var activePlugins = _pluginManager.PluginInfos.Count(p => p.IsLoaded && p.IsEnabled);
                RecentActivity = $"{activePlugins} 个插件正在运行 | 总计 {TotalPlugins} 个插件";
            }
        }

        public void Refresh()
        {
            UpdateStatistics();
        }

        public void Dispose()
        {
            _timer?.Dispose();
            SaveDashboardData();
        }
    }

    public partial class QuickAppInfo : ObservableObject
    {
        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string icon = "";
    }

    public partial class TodoItem : ObservableObject
    {
        [ObservableProperty]
        private string title = "";

        [ObservableProperty]
        private bool isCompleted;
    }

    public partial class ClipboardItem : ObservableObject
    {
        [ObservableProperty]
        private string content = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TimeDisplay))]
        private DateTime time;

        public string TimeDisplay => Time.ToLocalTime().ToString("HH:mm");
    }
}
