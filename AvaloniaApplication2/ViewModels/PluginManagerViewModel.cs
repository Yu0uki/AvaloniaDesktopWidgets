using AvaloniaApplication2.Core;
using AvaloniaApplication2.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace AvaloniaApplication2.ViewModels
{
    /// <summary>
    /// 插件管理器视图模型
    /// </summary>
    public partial class PluginManagerViewModel : ViewModelBase
    {
        private readonly PluginManager _pluginManager;

        public ObservableCollection<PluginInfo> Plugins => _pluginManager.PluginInfos;

        [ObservableProperty]
        private bool isDragOver;

        [ObservableProperty]
        private string statusMessage = "拖拽 DLL 文件到此处加载插件";

        public PluginManagerViewModel(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        /// <summary>
        /// 启用插件
        /// </summary>
        [RelayCommand]
        private async Task EnablePluginAsync(string pluginId)
        {
            await _pluginManager.EnablePluginAsync(pluginId);
            StatusMessage = $"插件已启用";
        }

        /// <summary>
        /// 禁用插件
        /// </summary>
        [RelayCommand]
        private async Task DisablePluginAsync(string pluginId)
        {
            await _pluginManager.DisablePluginAsync(pluginId);
            StatusMessage = $"插件已禁用";
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        [RelayCommand]
        private async Task UninstallPluginAsync(string pluginId)
        {
            try
            {
                await _pluginManager.DeletePluginAsync(pluginId);
                StatusMessage = $"插件已卸载";
            }
            catch (Exception ex)
            {
                StatusMessage = $"卸载失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 打开插件
        /// </summary>
        [RelayCommand]
        private void OpenPlugin(string pluginId)
        {
            var plugin = _pluginManager.GetPlugin(pluginId);
            if (plugin != null)
            {
                _pluginManager.ActivatePlugin(pluginId);
                StatusMessage = $"已打开插件: {plugin.Name}";
            }
        }

        /// <summary>
        /// 处理拖拽进入
        /// </summary>
        public void OnDragEnter()
        {
            IsDragOver = true;
            StatusMessage = "释放以加载插件";
        }

        /// <summary>
        /// 处理拖拽离开
        /// </summary>
        public void OnDragLeave()
        {
            IsDragOver = false;
            StatusMessage = "拖拽 DLL 文件到此处加载插件";
        }

        /// <summary>
        /// 处理文件放置
        /// </summary>
        public async Task OnDropAsync(IEnumerable<string> filePaths)
        {
            IsDragOver = false;

            foreach (var filePath in filePaths)
            {
                if (filePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var plugin = await _pluginManager.LoadPluginAsync(filePath);
                        if (plugin != null)
                        {
                            StatusMessage = $"插件加载成功: {plugin.Name}";
                        }
                        else
                        {
                            StatusMessage = $"插件加载失败";
                        }
                    }
                    catch (System.Exception ex)
                    {
                        StatusMessage = $"加载失败: {ex.Message}";
                    }
                }
            }
        }
    }
}
