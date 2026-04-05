using AvaloniaApplication2.Core;
using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Collections.Specialized;

namespace AvaloniaApplication2.ViewModels
{
    /// <summary>
    /// 仪表盘视图模型
    /// </summary>
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly PluginManager _pluginManager;

        [ObservableProperty]
        private int totalPlugins;

        [ObservableProperty]
        private int loadedPlugins;

        [ObservableProperty]
        private int enabledPlugins;

        [ObservableProperty]
        private string recentActivity = "暂无活动";

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
            
            UpdateStatistics();
        }

        /// <summary>
        /// 插件集合变化事件处理
        /// </summary>
        private void OnPluginCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 为新添加的插件订阅属性变化事件
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
            
            // 为移除的插件取消订阅
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
            // 当 IsEnabled 或 IsLoaded 属性变化时更新统计
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
    }
}
