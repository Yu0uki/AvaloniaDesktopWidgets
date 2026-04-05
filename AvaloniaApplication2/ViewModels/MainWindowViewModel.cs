using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AvaloniaApplication2.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly PluginManager _pluginManager;
        private readonly SettingsService _settingsService;
        private readonly NotificationService _notificationService;

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

        // 通知集合
        public ObservableCollection<NotificationMessage> Notifications => _notificationService.Notifications;

        public MainWindowViewModel(PluginManager pluginManager, SettingsService settingsService)
        {
            _pluginManager = pluginManager;
            _settingsService = settingsService;
            _notificationService = NotificationService.Instance;

            // 默认显示仪表盘
            CurrentPage = new DashboardViewModel(_pluginManager);
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
        /// 关闭窗口
        /// </summary>
        [RelayCommand]
        private void CloseWindow()
        {
            // 由视图处理
        }
    }
}
