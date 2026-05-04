using AvaloniaApplication2.Core;
using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;

namespace AvaloniaApplication2.ViewModels
{
    /// <summary>
    /// 仪表盘视图模型
    /// </summary>
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

        // 天气（示例数据）
        [ObservableProperty]
        private string weatherLocation = "北京市";

        [ObservableProperty]
        private string weatherTemperature = "24°C";

        [ObservableProperty]
        private string weatherIcon = "☀️";

        // 系统监控
        [ObservableProperty]
        private int cpuUsage = 32;

        [ObservableProperty]
        private int memoryUsage = 84;

        // 快捷应用列表
        public ObservableCollection<QuickAppInfo> QuickApps { get; } = new();

        // 待办事项列表
        public ObservableCollection<TodoItem> TodoItems { get; } = new();

        // 剪贴板历史
        public ObservableCollection<ClipboardItem> ClipboardHistory { get; } = new();

        public DashboardViewModel(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;

            // 监听插件集合变化
            _pluginManager.PluginInfos.CollectionChanged += OnPluginCollectionChanged;

            // 监听每个插件的属性变化
            foreach (var plugin in _pluginManager.PluginInfos)
            {
                plugin.PropertyChanged += OnPluginPropertyChanged;
            }

            // 初始化数据
            UpdateStatistics();
            UpdateGreeting();
            UpdateTime();
            InitializeQuickApps();
            InitializeTodoItems();
            InitializeClipboardHistory();

            // 启动定时器更新时间
            _timer = new Timer(UpdateTimerCallback, null, 1000, 1000);
        }

        private void UpdateTimerCallback(object? state)
        {
            UpdateTime();
            // 模拟系统监控数据更新（实际应用中应从系统 API 获取）
            UpdateSystemMonitor();
        }

        private void UpdateTime()
        {
            var now = DateTime.Now;
            CurrentTime = now.ToString("HH:mm");
            UpdateGreeting();

            // 更新日期（每分钟的 0 秒更新一次）
            if (now.Second == 0)
            {
                currentDate = now.ToString("yyyy年M月d日 dddd");
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
            // 模拟数据波动（实际应从系统 API 获取）
            var random = new Random();
            CpuUsage = Math.Clamp(CpuUsage + random.Next(-5, 6), 10, 90);
            MemoryUsage = Math.Clamp(MemoryUsage + random.Next(-2, 3), 50, 95);
        }

        private void InitializeQuickApps()
        {
            QuickApps.Add(new QuickAppInfo { Name = "终端控制台", Icon = "⌨️" });
            QuickApps.Add(new QuickAppInfo { Name = "代码编辑器", Icon = "📝" });
            QuickApps.Add(new QuickAppInfo { Name = "设计画板", Icon = "🎨" });
            QuickApps.Add(new QuickAppInfo { Name = "数据库管理", Icon = "💾" });
            QuickApps.Add(new QuickAppInfo { Name = "快捷便签", Icon = "📋" });
            QuickApps.Add(new QuickAppInfo { Name = "高级计算器", Icon = "🧮" });
            QuickApps.Add(new QuickAppInfo { Name = "翻译工具", Icon = "🌐" });
        }

        private void InitializeTodoItems()
        {
            TodoItems.Add(new TodoItem { Title = "完成前端原型设计", IsCompleted = true });
            TodoItems.Add(new TodoItem { Title = "技术栈选型会议", IsCompleted = false });
            TodoItems.Add(new TodoItem { Title = "编写底层接口", IsCompleted = false });
        }

        private void InitializeClipboardHistory()
        {
            ClipboardHistory.Add(new ClipboardItem { Content = "npm install @tauri-apps/cli", Time = DateTime.Now.AddMinutes(-5) });
            ClipboardHistory.Add(new ClipboardItem { Content = "https://github.com/tauri-apps...", Time = DateTime.Now.AddMinutes(-15) });
        }

        /// <summary>
        /// 插件集合变化事件处理
        /// </summary>
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

        /// <summary>
        /// 插件属性变化事件处理
        /// </summary>
        private void OnPluginPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PluginInfo.IsEnabled) ||
                e.PropertyName == nameof(PluginInfo.IsLoaded))
            {
                UpdateStatistics();
            }
        }

        /// <summary>
        /// 更新统计数据
        /// </summary>
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

        /// <summary>
        /// 刷新统计数据
        /// </summary>
        public void Refresh()
        {
            UpdateStatistics();
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }

    /// <summary>
    /// 快捷应用信息
    /// </summary>
    public partial class QuickAppInfo : ObservableObject
    {
        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string icon = "";
    }

    /// <summary>
    /// 待办事项
    /// </summary>
    public partial class TodoItem : ObservableObject
    {
        [ObservableProperty]
        private string title = "";

        [ObservableProperty]
        private bool isCompleted;
    }

    /// <summary>
    /// 剪贴板项目
    /// </summary>
    public partial class ClipboardItem : ObservableObject
    {
        [ObservableProperty]
        private string content = "";

        [ObservableProperty]
        private DateTime time;
    }
}
