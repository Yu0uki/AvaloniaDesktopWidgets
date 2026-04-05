using Avalonia.Controls;
using AvaloniaApplication2.DependencyInjection;
using AvaloniaApplication2.Infrastructure;
using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Collections.ObjectModel;

namespace AvaloniaApplication2.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly PluginManager _pluginManager;
        private readonly SettingsService _settingsService;
        private readonly NotificationService _notificationService;
        private readonly ILogger _logger;

        [ObservableProperty]
        private object? currentPage;

        [ObservableProperty]
        private string windowTitle = "效率工坊";

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

        // 通知集合
        public ObservableCollection<NotificationMessage> Notifications => _notificationService.Notifications;

        public MainWindowViewModel(PluginManager pluginManager, SettingsService settingsService)
        {
            _pluginManager = pluginManager;
            _settingsService = settingsService;
            _notificationService = NotificationService.Instance;
            _logger = LoggingConfig.Logger.ForContext<MainWindowViewModel>();

            // 默认显示仪表盘
            CurrentPage = new DashboardViewModel(_pluginManager);
            
            _logger.Information("MainWindowViewModel 已初始化");
        }

        /// <summary>
        /// 导航到仪表盘
        /// </summary>
        [RelayCommand]
        private void NavigateToDashboard()
        {
            SelectedNavIndex = 0;
            CurrentPage = new DashboardViewModel(_pluginManager);
        }

        /// <summary>
        /// 导航到插件管理器
        /// </summary>
        [RelayCommand]
        public void NavigateToPluginManager()
        {
            SelectedNavIndex = 1;
            CurrentPage = new PluginManagerViewModel(_pluginManager);
        }

        /// <summary>
        /// 导航到设置页面
        /// </summary>
        [RelayCommand]
        private void NavigateToSettings()
        {
            SelectedNavIndex = 2;
            CurrentPage = new SettingsViewModel(_settingsService);
        }

        /// <summary>
        /// 最小化窗口
        /// </summary>
        [RelayCommand]
        private void MinimizeWindow()
        {
            // 由视图处理
        }

        /// <summary>
        /// 最大化/还原窗口
        /// </summary>
        [RelayCommand]
        private void ToggleMaximize()
        {
            IsMaximized = !IsMaximized;
        }

        /// <summary>
        /// 显示插件视图
        /// </summary>
        public void ShowPluginView(string pluginName, Control pluginView)
        {
            _logger.Information("显示插件视图: {PluginName}, 控件类型: {ControlType}", 
                pluginName, pluginView.GetType().FullName);
            StatusMessage = $"正在运行: {pluginName}";
            CurrentPluginName = pluginName;
            IsShowingPluginView = true;
            CurrentPage = pluginView;
            _logger.Information("CurrentPage 已设置为: {PageType}", CurrentPage?.GetType().FullName);
        }

        /// <summary>
        /// 返回仪表盘
        /// </summary>
        [RelayCommand]
        private void NavigateBackToDashboard()
        {
            _logger.Information("返回仪表盘");
            IsShowingPluginView = false;
            CurrentPluginName = "";
            StatusMessage = "";
            NavigateToDashboard();
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        [RelayCommand]
        private void CloseWindow()
        {
            // 由视图处理
        }
    }
}
