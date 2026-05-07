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
        /// 停用插件（停止运行）
        /// </summary>
        [RelayCommand]
        private void StopPlugin(string pluginId)
        {
            try
            {
                _pluginManager.StopPlugin(pluginId);
                StatusMessage = $"插件已停止运行";
            }
            catch (Exception ex)
            {
                StatusMessage = $"停止失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 启动插件运行
        /// </summary>
        [RelayCommand]
        private void StartPlugin(string pluginId)
        {
            try
            {
                _pluginManager.StartPlugin(pluginId);
                StatusMessage = $"插件已启动运行";
            }
            catch (Exception ex)
            {
                StatusMessage = $"启动失败: {ex.Message}";
            }
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
            try
            {
                var plugin = _pluginManager.GetPlugin(pluginId);
                if (plugin != null)
                {
                    // 启动插件运行
                    _pluginManager.StartPlugin(pluginId);
                    _pluginManager.ActivatePlugin(pluginId);
                    
                    // 获取插件主视图
                    var mainView = plugin.GetMainView();
                    if (mainView != null)
                    {
                        // 创建插件包装器视图模型
                        var mainWindowVM = DependencyInjection.ServiceContainer.GetService<MainWindowViewModel>();
                        if (mainWindowVM != null)
                        {
                            var wrapperVM = new ViewModels.PluginWrapperViewModel(
                                pluginId, 
                                plugin.Name, 
                                mainView,
                                plugin,  // 传递 IPlugin 实例
                                mainWindowVM
                            );
                            
                            // 创建包装器视图
                            var wrapperView = new Views.PluginWrapperView
                            {
                                DataContext = wrapperVM
                            };
                            
                            // 显示插件视图
                            mainWindowVM.ShowPluginView(pluginId, plugin.Name, wrapperView);
                            StatusMessage = $"已打开插件: {plugin.Name}";
                        }
                        else
                        {
                            StatusMessage = "错误: 无法获取主窗口引用";
                        }
                    }
                    else
                    {
                        StatusMessage = $"错误: 插件 {plugin.Name} 没有提供主视图";
                    }
                }
                else
                {
                    StatusMessage = $"错误: 找不到插件 {pluginId}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"打开插件失败: {ex.Message}";
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
        /// 处理文件放置（异步复制 + 直接加载，无延迟）
        /// </summary>
        public async Task OnDropAsync(IEnumerable<string> filePaths)
        {
            IsDragOver = false;

            var pluginsDir = System.IO.Path.GetFullPath(_pluginManager.PluginsDirectory);
            var loadedCount = 0;
            var failCount = 0;

            foreach (var filePath in filePaths)
            {
                if (!filePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    var fileName = System.IO.Path.GetFileName(filePath);
                    var destPath = System.IO.Path.Combine(pluginsDir, fileName);

                    // 异步复制文件（不阻塞 UI）
                    using (var src = System.IO.File.OpenRead(filePath))
                    using (var dst = System.IO.File.Create(destPath))
                    {
                        await src.CopyToAsync(dst);
                    }

                    StatusMessage = $"正在加载: {fileName}...";

                    // 直接加载（LoadPluginAsync 内部已去重）
                    var plugin = await _pluginManager.LoadPluginAsync(destPath);
                    if (plugin != null)
                    {
                        loadedCount++;
                        StatusMessage = $"插件加载成功: {plugin.Name}";
                    }
                    else
                    {
                        failCount++;
                    }
                }
                catch (System.Exception ex)
                {
                    failCount++;
                    StatusMessage = $"加载失败: {ex.Message}";
                }
            }

            if (loadedCount > 0 || failCount == 0)
                StatusMessage = $"加载完成: {loadedCount} 个成功" + (failCount > 0 ? $", {failCount} 个失败" : "");
        }

        /// <summary>
        /// 手动刷新插件列表（同步 ./Plugins 文件夹）
        /// </summary>
        [RelayCommand]
        private async Task RefreshPluginsAsync()
        {
            StatusMessage = "正在同步插件...";
            await _pluginManager.SyncPluginsFromFolderAsync();
            StatusMessage = _pluginManager.PluginInfos.Count > 0
                ? $"同步完成: {_pluginManager.PluginInfos.Count(p => p.IsLoaded)} 个插件已加载"
                : "拖拽 DLL 文件到此处加载插件";
        }
    }
}
