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
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaApplication2.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly PluginManager _pluginManager;
        private readonly HttpClient _httpClient = new();
        private Timer? _timer;
        private DateTime _lastWeatherFetch = DateTime.MinValue;
        private static readonly TimeSpan WeatherCacheDuration = TimeSpan.FromMinutes(30);

        // 城市坐标 (lat, lon) — 用于 Open-Meteo API
        private static readonly Dictionary<string, (double lat, double lon)> CityCoordinates = new()
        {
            ["北京市"] = (39.90, 116.41),
            ["上海市"] = (31.23, 121.47),
            ["广州市"] = (23.13, 113.26),
            ["深圳市"] = (22.54, 114.06),
            ["杭州市"] = (30.29, 120.15),
            ["成都市"] = (30.57, 104.07),
            ["武汉市"] = (30.59, 114.31),
            ["南京市"] = (32.06, 118.80),
            ["西安市"] = (34.26, 108.94),
            ["重庆市"] = (29.43, 106.91),
            ["扬州市"] = (32.39, 119.42),
        };

        // wttr.in 天气代码 → (图标, 描述)
        private static readonly Dictionary<string, (string icon, string desc)> WttrCodeMap = new()
        {
            ["113"] = ("☀️", "晴朗"),
            ["116"] = ("⛅", "多云"),
            ["119"] = ("☁️", "阴天"),
            ["122"] = ("☁️", "阴天"),
            ["143"] = ("🌫️", "雾"),
            ["176"] = ("🌦️", "阵雨"),
            ["179"] = ("🌨️", "阵雪"),
            ["182"] = ("🌨️", "雨夹雪"),
            ["185"] = ("🌨️", "冻雨"),
            ["200"] = ("⛈️", "雷暴"),
            ["227"] = ("🌨️", "雪"),
            ["230"] = ("🌨️", "暴雪"),
            ["248"] = ("🌫️", "雾"),
            ["260"] = ("🌫️", "雾"),
            ["263"] = ("🌦️", "小雨"),
            ["266"] = ("🌦️", "小雨"),
            ["281"] = ("🌨️", "冻雨"),
            ["284"] = ("🌨️", "冻雨"),
            ["293"] = ("🌦️", "小雨"),
            ["296"] = ("🌦️", "小雨"),
            ["299"] = ("🌧️", "中雨"),
            ["302"] = ("🌧️", "中雨"),
            ["305"] = ("🌧️", "大雨"),
            ["308"] = ("🌧️", "大雨"),
            ["311"] = ("🌨️", "冻雨"),
            ["314"] = ("🌨️", "冻雨"),
            ["317"] = ("🌨️", "雨夹雪"),
            ["320"] = ("🌨️", "雨夹雪"),
            ["323"] = ("🌨️", "小雪"),
            ["326"] = ("🌨️", "小雪"),
            ["329"] = ("🌨️", "中雪"),
            ["332"] = ("🌨️", "中雪"),
            ["335"] = ("🌨️", "大雪"),
            ["338"] = ("🌨️", "大雪"),
            ["350"] = ("🌨️", "冰雹"),
            ["353"] = ("🌦️", "阵雨"),
            ["356"] = ("🌧️", "大雨"),
            ["359"] = ("🌧️", "暴雨"),
            ["362"] = ("🌨️", "雨夹雪"),
            ["365"] = ("🌨️", "雨夹雪"),
            ["368"] = ("🌨️", "小雪"),
            ["371"] = ("🌨️", "大雪"),
            ["374"] = ("🌨️", "冰雹"),
            ["377"] = ("🌨️", "冰雹"),
            ["386"] = ("⛈️", "雷阵雨"),
            ["389"] = ("⛈️", "大雷雨"),
            ["392"] = ("⛈️", "雷阵雪"),
            ["395"] = ("🌨️", "大雪"),
        };

        [ObservableProperty] private int totalPlugins;
        [ObservableProperty] private int loadedPlugins;
        [ObservableProperty] private int enabledPlugins;
        [ObservableProperty] private string recentActivity = "暂无活动";
        [ObservableProperty] private string greeting = "早上好";
        [ObservableProperty] private string currentTime = "00:00";
        [ObservableProperty] private string currentDate = "";
        [ObservableProperty] private string weatherLocation = "扬州市";
        [ObservableProperty] private string weatherTemperature = "--°C";
        [ObservableProperty] private string weatherIcon = "☀️";
        [ObservableProperty] private string weatherDescription = "加载中...";
        [ObservableProperty] private string selectedCity = "扬州市";
        [ObservableProperty] private int cpuUsage = 32;
        [ObservableProperty] private int memoryUsage = 84;
        [ObservableProperty] private string newTodoText = "";
        [ObservableProperty] private string newQuickAppText = "";
        [ObservableProperty] private bool isAddingQuickApp;
        [ObservableProperty] private bool isEditingLayout;
        [ObservableProperty] private bool showWeatherCard = true;
        [ObservableProperty] private bool showQuickAppsCard = true;
        [ObservableProperty] private bool showSystemMonitorCard = true;
        [ObservableProperty] private bool showTodoCard = true;
        [ObservableProperty] private bool showClipboardCard = true;
        [ObservableProperty] private bool showPluginStatsCard = true;

        public ObservableCollection<QuickAppInfo> QuickApps { get; } = new();
        public ObservableCollection<TodoItem> TodoItems { get; } = new();
        public ObservableCollection<ClipboardItem> ClipboardHistory { get; } = new();

        public ObservableCollection<string> Cities { get; } = new()
        {
            "北京市", "上海市", "广州市", "深圳市", "杭州市",
            "成都市", "武汉市", "南京市", "西安市", "重庆市", "扬州市"
        };

        public DashboardViewModel(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;

            _pluginManager.PluginInfos.CollectionChanged += OnPluginCollectionChanged;
            foreach (var plugin in _pluginManager.PluginInfos)
                plugin.PropertyChanged += OnPluginPropertyChanged;

            LoadDashboardData();
            EnsureDefaultSystemTools();
            UpdateStatistics();
            UpdateGreeting();
            UpdateTime();

            _ = FetchWeatherAsync(SelectedCity);

            _timer = new Timer(UpdateTimerCallback, null, 1000, 1000);
        }

        partial void OnSelectedCityChanged(string value)
        {
            WeatherLocation = value;
            _ = FetchWeatherAsync(value, force: true);
        }

        private async Task FetchWeatherAsync(string city, bool force = false)
        {
            // 非强制刷新时，遵守缓存策略
            if (!force
                && (DateTime.Now - _lastWeatherFetch) < WeatherCacheDuration
                && WeatherLocation == city && WeatherTemperature != "--°C")
                return;

            // 优先 wttr.in，回退 Open-Meteo
            var result = await TryFetchFromWttrAsync(city)
                      ?? (CityCoordinates.TryGetValue(city, out var c)
                          ? await TryFetchFromOpenMeteoAsync(c.lat, c.lon)
                          : null);

            if (result != null)
            {
                WeatherTemperature = result.Value.temp;
                WeatherIcon = result.Value.icon;
                WeatherDescription = result.Value.desc;
                _lastWeatherFetch = DateTime.Now;
            }
            else if (WeatherTemperature == "--°C")
            {
                WeatherTemperature = "24°C";
                WeatherIcon = "☀️";
                WeatherDescription = "离线";
            }
        }

        private async Task<(string temp, string icon, string desc)?> TryFetchFromWttrAsync(string city)
        {
            try
            {
                var encoded = Uri.EscapeDataString(city);
                var url = $"https://wttr.in/{encoded}?format=j1";
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("curl/8.0");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                var json = await _httpClient.GetStringAsync(url, cts.Token);

                using var doc = JsonDocument.Parse(json);
                var current = doc.RootElement.GetProperty("current_condition")[0];

                var tempC = current.GetProperty("temp_C").GetString() ?? "?";
                var code = current.GetProperty("weatherCode").GetString() ?? "";
                var weatherDesc = current.GetProperty("weatherDesc")[0]
                    .GetProperty("value").GetString() ?? "";

                var icon = WttrCodeMap.TryGetValue(code, out var w) ? w.icon : "🌤️";
                return ($"{tempC}°C", icon, weatherDesc);
            }
            catch
            {
                return null;
            }
        }

        private async Task<(string temp, string icon, string desc)?> TryFetchFromOpenMeteoAsync(double lat, double lon)
        {
            try
            {
                var url = $"https://api.open-meteo.com/v1/forecast" +
                          $"?latitude={lat}&longitude={lon}" +
                          $"&current_weather=true&timezone=Asia/Shanghai";

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                var json = await _httpClient.GetStringAsync(url, cts.Token);

                using var doc = JsonDocument.Parse(json);
                var current = doc.RootElement.GetProperty("current_weather");
                var tempC = current.GetProperty("temperature").GetDouble();
                var code = current.GetProperty("weathercode").GetInt32();

                var desc = code switch
                {
                    0 => "晴朗", 1 => "大部晴朗", 2 => "多云", 3 => "阴天",
                    45 or 48 => "雾",
                    >= 51 and <= 55 => "毛毛雨", >= 61 and <= 65 => "雨",
                    >= 71 and <= 77 => "雪", >= 80 and <= 82 => "阵雨",
                    >= 85 and <= 86 => "阵雪", >= 95 => "雷暴",
                    _ => "未知"
                };
                var icon = code switch
                {
                    0 => "☀️", 1 => "🌤️", 2 => "⛅", 3 => "☁️",
                    45 or 48 => "🌫️",
                    >= 51 and <= 55 => "🌦️", >= 61 and <= 65 => "🌧️",
                    >= 71 and <= 77 => "🌨️", >= 80 and <= 82 => "🌦️",
                    >= 85 and <= 86 => "🌨️", >= 95 => "⛈️",
                    _ => "🌤️"
                };
                return ($"{tempC:F0}°C", icon, desc);
            }
            catch
            {
                return null;
            }
        }

        private int _tickCount;
        private void UpdateTimerCallback(object? state)
        {
            UpdateTime();
            UpdateSystemMonitor();

            // 每 30 分钟刷新一次天气
            if (++_tickCount % 1800 == 0)
                _ = FetchWeatherAsync(SelectedCity);
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

        /// <summary>
        /// 确保默认 Windows 系统工具存在于快捷应用中（每次启动时补齐）
        /// </summary>
        private void EnsureDefaultSystemTools()
        {
            var defaults = new (string name, string icon)[]
            {
                ("notepad", "📝"),
                ("powershell", "⌨️"),
                ("mspaint", "🎨"),
                ("soundrecorder", "🎤"),
                ("calc", "🧮"),
                ("explorer", "📂"),
                ("ms-settings:", "⚙️"),
            };

            foreach (var (name, icon) in defaults)
            {
                if (!QuickApps.Any(a => a.Name == name))
                {
                    QuickApps.Add(new QuickAppInfo { Name = name, Icon = icon });
                }
            }
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
        private async Task SelectQuickAppExeAsync()
        {
            try
            {
                var lifetime = App.Current?.ApplicationLifetime
                    as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
                if (lifetime?.MainWindow?.StorageProvider is { } storage)
                {
                    var fileTypes = new Avalonia.Platform.Storage.FilePickerFileType[]
                    {
                        new("可执行程序") { Patterns = new[] { "*.exe", "*.bat", "*.cmd", "*.lnk" } }
                    };
                    var files = await storage.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                    {
                        Title = "选择可执行程序",
                        AllowMultiple = false,
                        FileTypeFilter = fileTypes
                    });

                    if (files != null && files.Count > 0)
                    {
                        var filePath = files[0].Path.LocalPath;
                        var name = System.IO.Path.GetFileNameWithoutExtension(filePath);
                        var icon = ExtractExeIcon(filePath, name);
                        QuickApps.Add(new QuickAppInfo
                        {
                            Name = name,
                            Icon = icon,
                            ExePath = filePath
                        });
                        SaveDashboardData();
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "选择可执行程序失败");
            }
        }

        private static string ExtractExeIcon(string exePath, string appName)
        {
            try
            {
                using var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                if (icon == null) return "🖥️";

                using var bitmap = icon.ToBitmap();
                var iconDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "icons");
                System.IO.Directory.CreateDirectory(iconDir);
                var pngPath = System.IO.Path.Combine(iconDir, $"{appName}.png");
                bitmap.Save(pngPath, System.Drawing.Imaging.ImageFormat.Png);
                return pngPath;
            }
            catch
            {
                return "🖥️";
            }
        }

        [RelayCommand]
        private void LaunchQuickApp(QuickAppInfo? app)
        {
            if (app == null) return;

            try
            {
                var target = string.IsNullOrWhiteSpace(app.ExePath)
                    ? app.Name
                    : app.ExePath;

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = target,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "启动快捷应用失败: {App}", app.Name);
            }
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
        private void ResetLayout()
        {
            ShowWeatherCard = true;
            ShowQuickAppsCard = true;
            ShowSystemMonitorCard = true;
            ShowTodoCard = true;
            ShowClipboardCard = true;
            ShowPluginStatsCard = true;
            IsEditingLayout = false;
            SaveDashboardData();
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
                        Icon = q.Icon,
                        ExePath = q.ExePath,
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
                    WeatherLocation = data.SelectedCity;
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
                    QuickApps.Add(new QuickAppInfo { Name = q.Name, Icon = q.Icon, ExePath = q.ExePath });
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
            SelectedCity = "扬州市";
            WeatherLocation = "扬州市";

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

            // 系统工具由 EnsureDefaultSystemTools() 统一补齐

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

        // 自定义 exe 路径（为空时使用 Name 作为系统命令启动）
        [ObservableProperty]
        private string exePath = "";
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
