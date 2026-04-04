using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;

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
            UpdateStatistics();
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
                RecentActivity = "拖拽 DLL 文件到窗口以添加插件";
            }
            else if (LoadedPlugins == 0)
            {
                RecentActivity = $"已安装 {TotalPlugins} 个插件，点击启用开始使用";
            }
            else
            {
                RecentActivity = $"{LoadedPlugins} 个插件正在运行";
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
