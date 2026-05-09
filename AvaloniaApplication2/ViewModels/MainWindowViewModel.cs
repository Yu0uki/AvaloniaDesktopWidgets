using Avalonia.Controls;
using AvaloniaApplication2.Core;
using AvaloniaApplication2.DependencyInjection;
using AvaloniaApplication2.Infrastructure;
using AvaloniaApplication2.Models;
using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaApplication2.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly PluginManager _pluginManager;
        private readonly SettingsService _settingsService;
        private readonly NotificationService _notificationService;
        private readonly ILogger _logger;
        private int _unreadCount;

        public event EventHandler? WindowMinimizeRequested;
        public event EventHandler<WindowStateEventArgs>? WindowMaximizeRequested;
        public event EventHandler? WindowCloseRequested;

        [ObservableProperty]
        private object? currentPage;

        [ObservableProperty]
        private string windowTitle = "EfficiencyWorkshop 5.0.1";

        [ObservableProperty]
        private bool isMaximized;

        [ObservableProperty]
        private int selectedNavIndex = 0;

        [ObservableProperty]
        private string statusMessage = "";

        [ObservableProperty]
        private bool isShowingPluginView = false;

        [ObservableProperty]
        private string currentPluginName = "";

        [ObservableProperty]
        private string? selectedPluginId;

        [ObservableProperty]
        private bool isSidebarExpanded = true;

        [ObservableProperty]
        private string sidebarWidth = "220";

        [ObservableProperty]
        private string searchQuery = "";

        partial void OnSearchQueryChanged(string value)
        {
            if (EnableSearchHistory && SearchHistoryItems.Count > 0)
                ShowSearchHistory = true;
        }

        [ObservableProperty]
        private bool showSearchHistory;

        [ObservableProperty]
        private bool enableSearchHistory = true;

        public ObservableCollection<string> SearchHistoryItems { get; } = new();

        // 通知中心
        [ObservableProperty]
        private bool isNotificationCenterOpen = false;

        [ObservableProperty]
        private string notificationSummary = "";

        [ObservableProperty]
        private bool hasUnreadNotifications = false;

        [ObservableProperty]
        private int unreadNotificationCount = 0;

        // 最大化图标：根据 IsMaximized 状态动态切换
        public string MaximizeIcon => IsMaximized ? FluentIcons.Restore : FluentIcons.Maximize;

        public ObservableCollection<PluginInfo> RunningPlugins { get; }
        public ObservableCollection<NotificationMessage> Notifications => _notificationService.Notifications;
        public ObservableCollection<NotificationLogEntry> NotificationHistory { get; } = new();

        public event EventHandler? FocusSearchRequested;

        public MainWindowViewModel(PluginManager pluginManager, SettingsService settingsService)
        {
            _pluginManager = pluginManager;
            _settingsService = settingsService;
            _notificationService = NotificationService.Instance;
            _logger = LoggingConfig.Logger.ForContext<MainWindowViewModel>();

            RunningPlugins = new ObservableCollection<PluginInfo>();

            _pluginManager.PluginStateChanged += OnPluginStateChanged;

            CurrentPage = new DashboardViewModel(_pluginManager);

            foreach (var entry in _settingsService.Settings.NotificationLog.Take(50))
                NotificationHistory.Add(entry);
            UpdateNotificationSummary();

            EnableSearchHistory = _settingsService.Settings.EnableSearchHistory;
            foreach (var h in _settingsService.Settings.SearchHistory.Take(20))
                SearchHistoryItems.Add(h);

            _notificationService.NotificationAdded += OnNotificationAdded;

            _logger.Information("MainWindowViewModel 已初始化");
        }

        partial void OnIsMaximizedChanged(bool value)
        {
            OnPropertyChanged(nameof(MaximizeIcon));
        }

        private async void OnNotificationAdded(object? sender, NotificationMessage e)
        {
            _unreadCount++;
            UnreadNotificationCount = _unreadCount;
            HasUnreadNotifications = _unreadCount > 0;

            await _settingsService.AddNotificationLogAsync(e.Message, e.Type.ToString());
            NotificationHistory.Insert(0, new NotificationLogEntry
            {
                Message = e.Message,
                Type = e.Type.ToString(),
                Timestamp = e.Timestamp
            });
            while (NotificationHistory.Count > 50)
                NotificationHistory.RemoveAt(NotificationHistory.Count - 1);
            UpdateNotificationSummary();
        }

        private void UpdateNotificationSummary()
        {
            var errors = NotificationHistory.Count(e => e.Type == "Error");
            var warnings = NotificationHistory.Count(e => e.Type == "Warning");
            var successes = NotificationHistory.Count(e => e.Type == "Success");

            var parts = new System.Collections.Generic.List<string>();
            if (errors > 0) parts.Add($"{errors} 个错误");
            if (warnings > 0) parts.Add($"{warnings} 个警告");
            if (successes > 0) parts.Add($"{successes} 条成功");
            NotificationSummary = parts.Count > 0 ? "今日: " + string.Join("，", parts) : "暂无新消息";
        }

        public async Task SaveWindowPositionAsync(double x, double y)
        {
            await _settingsService.UpdateWindowPositionAsync(x, y);
        }

        private void OnPluginStateChanged(string pluginId, Services.PluginStateChange stateChange)
        {
            switch (stateChange)
            {
                case Services.PluginStateChange.Started:
                    var startedPluginInfo = _pluginManager.PluginInfos.FirstOrDefault(p => p.Id == pluginId);
                    if (startedPluginInfo != null && !RunningPlugins.Any(p => p.Id == pluginId))
                        RunningPlugins.Add(startedPluginInfo);
                    break;

                case Services.PluginStateChange.Stopped:
                case Services.PluginStateChange.Unloaded:
                    var pluginInfo = RunningPlugins.FirstOrDefault(p => p.Id == pluginId);
                    if (pluginInfo != null)
                        RunningPlugins.Remove(pluginInfo);
                    if (SelectedPluginId == pluginId)
                        NavigateBackToDashboard();
                    break;
            }
        }

        [RelayCommand]
        private void NavigateToDashboard()
        {
            SelectedNavIndex = 0;
            CurrentPage = new DashboardViewModel(_pluginManager);
        }

        [RelayCommand]
        public void NavigateToPluginManager()
        {
            SelectedNavIndex = 1;
            CurrentPage = new PluginManagerViewModel(_pluginManager);
        }

        [RelayCommand]
        private void NavigateToPluginMarket()
        {
            SelectedNavIndex = 4;
            CurrentPage = new PluginMarketViewModel(_pluginManager);
        }

        [RelayCommand]
        private void NavigateToSettings()
        {
            SelectedNavIndex = 2;
            CurrentPage = new SettingsViewModel(_settingsService);
        }

        [RelayCommand]
        private void NavigateToLogsPage()
        {
            SelectedNavIndex = 3;
            CurrentPage = new LogsViewModel(_settingsService);
        }

        [RelayCommand]
        private void NavigateToLogs()
        {
            // 从通知中心点击后导航到独立的日志页面
            IsNotificationCenterOpen = false;
            NavigateToLogsPage();
        }

        [RelayCommand]
        private void MinimizeWindow()
        {
            WindowMinimizeRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void ToggleMaximize()
        {
            IsMaximized = !IsMaximized;
            WindowMaximizeRequested?.Invoke(this, new WindowStateEventArgs(IsMaximized));
        }

        [RelayCommand]
        private void CloseWindow()
        {
            WindowCloseRequested?.Invoke(this, EventArgs.Empty);
        }

        // ===== 通知中心 =====

        [RelayCommand]
        private void ToggleNotificationCenter()
        {
            IsNotificationCenterOpen = !IsNotificationCenterOpen;
            if (IsNotificationCenterOpen)
            {
                // 标记所有通知为已读
                _unreadCount = 0;
                UnreadNotificationCount = 0;
                HasUnreadNotifications = false;
            }
        }

        [RelayCommand]
        private async Task ClearNotifications()
        {
            NotificationHistory.Clear();
            _notificationService.ClearNotifications();
            await _settingsService.ClearNotificationLogAsync();
            _unreadCount = 0;
            UnreadNotificationCount = 0;
            HasUnreadNotifications = false;
            IsNotificationCenterOpen = false;
            UpdateNotificationSummary();
        }

        // ===== 插件导航 =====

        public void ShowPluginView(string pluginId, string pluginName, Control pluginView)
        {
            StatusMessage = $"正在运行: {pluginName}";
            CurrentPluginName = pluginName;
            SelectedPluginId = pluginId;
            IsShowingPluginView = true;
            CurrentPage = pluginView;
        }

        [RelayCommand]
        private void NavigateToPlugin(string pluginId)
        {
            try
            {
                var plugin = _pluginManager.GetPlugin(pluginId);
                if (plugin != null)
                {
                    _pluginManager.StartPlugin(pluginId);
                    _pluginManager.ActivatePlugin(pluginId);

                    var mainView = plugin.GetMainView();
                    if (mainView != null)
                    {
                        var wrapperVM = new PluginWrapperViewModel(
                            pluginId, plugin.Name, mainView, plugin, this);

                        var wrapperView = new Views.PluginWrapperView
                        {
                            DataContext = wrapperVM
                        };

                        ShowPluginView(pluginId, plugin.Name, wrapperView);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "导航到插件失败: {PluginId}", pluginId);
                _notificationService.ShowError($"打开插件失败: {ex.Message}");
            }
        }

        [RelayCommand]
        public void ClosePluginView(string pluginId)
        {
            try
            {
                if (CurrentPage is Views.PluginWrapperView wrapperView &&
                    wrapperView.DataContext is PluginWrapperViewModel wrapperVM)
                {
                    wrapperVM.Dispose();
                }

                _pluginManager.StopPlugin(pluginId);

                var pluginInfo = RunningPlugins.FirstOrDefault(p => p.Id == pluginId);
                if (pluginInfo != null)
                    RunningPlugins.Remove(pluginInfo);

                NavigateBackToDashboard();
                _notificationService.ShowInfo("插件已关闭");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "关闭插件失败: {PluginId}", pluginId);
            }
        }

        public async Task OpenPluginAsWindowAsync(string pluginId)
        {
            try
            {
                var plugin = _pluginManager.GetPlugin(pluginId);
                if (plugin != null)
                {
                    var pluginView = plugin.GetMainView();

                    var pluginWindowVM = new PluginWindowViewModel
                    {
                        PluginId = pluginId,
                        PluginName = plugin.Name,
                        PluginContent = pluginView,
                        MainWindowVM = this
                    };

                    var pluginWindow = new Views.PluginWindow
                    {
                        DataContext = pluginWindowVM
                    };

                    pluginWindow.Closed += (s, e) =>
                    {
                        _pluginManager.DeactivatePlugin(pluginId);
                        pluginWindowVM.Dispose();
                    };

                    pluginWindow.Show();
                    _notificationService.ShowSuccess($"已在独立窗口中打开: {plugin.Name}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "打开独立窗口失败: {PluginId}", pluginId);
                _notificationService.ShowError($"打开独立窗口失败: {ex.Message}");
            }
        }

        public void ShowPluginSettingsView(string pluginId, string pluginName, Control settingsView)
        {
            try
            {
                var wrapperVM = new PluginWrapperViewModel(
                    pluginId, $"{pluginName} - 设置", settingsView, null, this);

                var wrapperView = new Views.PluginWrapperView { DataContext = wrapperVM };

                IsShowingPluginView = true;
                CurrentPluginName = $"{pluginName} - 设置";
                SelectedPluginId = pluginId;
                CurrentPage = wrapperView;
                StatusMessage = $"正在配置: {pluginName}";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "显示插件设置失败: {PluginId}", pluginId);
                _notificationService.ShowError($"打开设置失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private void NavigateBackToDashboard()
        {
            CurrentPage = null;
            IsShowingPluginView = false;
            CurrentPluginName = "";
            SelectedPluginId = null;
            StatusMessage = "";
            NavigateToDashboard();
        }

        [RelayCommand]
        private void WebSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery)) return;
            try
            {
                var encoded = Uri.EscapeDataString(SearchQuery.Trim());
                var url = $"https://www.bing.com/search?q={encoded}";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                // 保存搜索历史
                if (EnableSearchHistory)
                {
                    var q = SearchQuery.Trim();
                    SearchHistoryItems.Remove(q);
                    SearchHistoryItems.Insert(0, q);
                    while (SearchHistoryItems.Count > 20)
                        SearchHistoryItems.RemoveAt(SearchHistoryItems.Count - 1);

                    _settingsService.Settings.SearchHistory = SearchHistoryItems.ToList();
                    _ = _settingsService.SaveSettingsAsync();
                }

                _notificationService.ShowInfo($"正在搜索: {SearchQuery}");
                SearchQuery = "";
                ShowSearchHistory = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "网页搜索失败: {Query}", SearchQuery);
                _notificationService.ShowError("搜索失败，请检查浏览器是否可用");
            }
        }

        [RelayCommand]
        private void FocusSearch() { ShowSearchHistory = EnableSearchHistory && SearchHistoryItems.Count > 0; }

        [RelayCommand]
        private void SelectSearchHistory(string query)
        {
            SearchQuery = query;
            ShowSearchHistory = false;
            WebSearch();
        }

        [RelayCommand]
        private async Task ClearSearchHistory()
        {
            SearchHistoryItems.Clear();
            _settingsService.Settings.SearchHistory.Clear();
            await _settingsService.SaveSettingsAsync();
            ShowSearchHistory = false;
        }

        [RelayCommand]
        private async Task ToggleSidebar()
        {
            IsSidebarExpanded = !IsSidebarExpanded;
            SidebarWidth = IsSidebarExpanded ? "220" : "64";
            await _settingsService.UpdateSidebarLayoutAsync(IsSidebarExpanded, double.Parse(SidebarWidth));
        }
    }

    public class WindowStateEventArgs : EventArgs
    {
        public bool IsMaximized { get; }
        public WindowStateEventArgs(bool isMaximized) => IsMaximized = isMaximized;
    }
}
