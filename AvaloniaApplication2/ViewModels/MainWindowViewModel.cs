using Avalonia.Controls;
using AvaloniaApplication2.Core;
using AvaloniaApplication2.DependencyInjection;
using AvaloniaApplication2.Infrastructure;
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

        [ObservableProperty]
        private string? selectedPluginId;

        // 已启动的插件列表（用于导航栏显示）
        public ObservableCollection<PluginInfo> RunningPlugins { get; }

        // 通知集合
        public ObservableCollection<NotificationMessage> Notifications => _notificationService.Notifications;

        public MainWindowViewModel(PluginManager pluginManager, SettingsService settingsService)
        {
            _pluginManager = pluginManager;
            _settingsService = settingsService;
            _notificationService = NotificationService.Instance;
            _logger = LoggingConfig.Logger.ForContext<MainWindowViewModel>();

            // 初始化运行插件列表
            RunningPlugins = new ObservableCollection<PluginInfo>();
            
            // 订阅插件状态改变事件
            _pluginManager.PluginStateChanged += OnPluginStateChanged;

            // 默认显示仪表盘
            CurrentPage = new DashboardViewModel(_pluginManager);
            
            _logger.Information("MainWindowViewModel 已初始化");
        }

        /// <summary>
        /// 处理插件状态改变
        /// </summary>
        private void OnPluginStateChanged(string pluginId, Services.PluginStateChange stateChange)
        {
            switch (stateChange)
            {
                case Services.PluginStateChange.Started:
                    // 添加到运行列表
                    var startedPluginInfo = _pluginManager.PluginInfos.FirstOrDefault(p => p.Id == pluginId);
                    if (startedPluginInfo != null && !RunningPlugins.Any(p => p.Id == pluginId))
                    {
                        RunningPlugins.Add(startedPluginInfo);
                    }
                    break;
                    
                case Services.PluginStateChange.Stopped:
                case Services.PluginStateChange.Unloaded:
                    // 从运行列表中移除
                    var pluginInfo = RunningPlugins.FirstOrDefault(p => p.Id == pluginId);
                    if (pluginInfo != null)
                    {
                        RunningPlugins.Remove(pluginInfo);
                    }
                    
                    // 如果当前显示的是该插件，则返回仪表盘
                    if (SelectedPluginId == pluginId)
                    {
                        NavigateBackToDashboard();
                    }
                    break;
            }
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
        public void ShowPluginView(string pluginId, string pluginName, Control pluginView)
        {
            _logger.Information("显示插件视图: {PluginName}, 控件类型: {ControlType}", 
                pluginName, pluginView.GetType().FullName);
            
            StatusMessage = $"正在运行: {pluginName}";
            CurrentPluginName = pluginName;
            SelectedPluginId = pluginId;
            IsShowingPluginView = true;
            CurrentPage = pluginView;
            
            // 注意：RunningPlugins的添加由OnPluginStateChanged事件处理，避免重复添加
            
            _logger.Information("CurrentPage 已设置为: {PageType}", CurrentPage?.GetType().FullName);
        }

        /// <summary>
        /// 导航到指定插件
        /// </summary>
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
                        // 创建插件包装器视图模型
                        var wrapperVM = new PluginWrapperViewModel(
                            pluginId,
                            plugin.Name,
                            mainView,
                            plugin,  // 传递 IPlugin 实例
                            this
                        );
                        
                        // 创建包装器视图
                        var wrapperView = new Views.PluginWrapperView
                        {
                            DataContext = wrapperVM
                        };
                        
                        // 显示插件视图（使用包装器）
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

        /// <summary>
        /// 关闭插件视图
        /// </summary>
        [RelayCommand]
        public void ClosePluginView(string pluginId)
        {
            try
            {
                _pluginManager.StopPlugin(pluginId);
                
                // 从运行列表中移除
                var pluginInfo = RunningPlugins.FirstOrDefault(p => p.Id == pluginId);
                if (pluginInfo != null)
                {
                    RunningPlugins.Remove(pluginInfo);
                }
                
                // 返回仪表盘
                NavigateBackToDashboard();
                
                _notificationService.ShowInfo("插件已关闭");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "关闭插件失败: {PluginId}", pluginId);
            }
        }

        /// <summary>
        /// 打开插件为独立窗口
        /// </summary>
        public async Task OpenPluginAsWindowAsync(string pluginId)
        {
            try
            {
                var plugin = _pluginManager.GetPlugin(pluginId);
                if (plugin != null)
                {
                    var pluginView = plugin.GetMainView();
                    
                    // 创建新窗口
                    var pluginWindow = new Views.PluginWindow
                    {
                        DataContext = new ViewModels.PluginWindowViewModel
                        {
                            PluginId = pluginId,
                            PluginName = plugin.Name,
                            PluginContent = pluginView,
                            MainWindowVM = this
                        }
                    };
                    
                    // 显示窗口
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

        /// <summary>
        /// 显示插件设置视图
        /// </summary>
        public void ShowPluginSettingsView(string pluginId, string pluginName, Control settingsView)
        {
            try
            {
                _logger.Information("显示插件设置: {PluginName}", pluginName);
                
                // 创建包装器视图模型（使用设置视图）
                var wrapperVM = new PluginWrapperViewModel(
                    pluginId,
                    $"{pluginName} - 设置",
                    settingsView,
                    null,  // 设置视图不需要 IPlugin 实例
                    this
                );
                
                // 创建包装器视图
                var wrapperView = new Views.PluginWrapperView
                {
                    DataContext = wrapperVM
                };
                
                // 显示设置视图
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

        /// <summary>
        /// 返回仪表盘
        /// </summary>
        [RelayCommand]
        private void NavigateBackToDashboard()
        {
            _logger.Information("返回仪表盘");
            IsShowingPluginView = false;
            CurrentPluginName = "";
            SelectedPluginId = null;
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
