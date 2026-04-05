using AvaloniaApplication2.Core;
using AvaloniaApplication2.Models;
using AvaloniaApplication2.Infrastructure;
using Serilog;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Loader;
using System.Collections.Generic;
using System;
using System.IO;

namespace AvaloniaApplication2.Services
{
    /// <summary>
    /// 插件状态改变类型
    /// </summary>
    public enum PluginStateChange
    {
        Started,    // 插件启动
        Stopped,    // 插件停止
        Unloaded    // 插件卸载
    }

    /// <summary>
    /// 插件管理器 - 负责加载、卸载和管理插件生命周期
    /// </summary>
    public class PluginManager
    {
        private readonly string _pluginsDirectory;
        private readonly Dictionary<string, IPlugin> _loadedPlugins = new();
        private readonly Dictionary<string, PluginLoadContext> _pluginContexts = new();
        private readonly ObservableCollection<PluginInfo> _pluginInfos = new();
        private readonly SettingsService _settingsService;
        private readonly NotificationService _notificationService;
        private readonly ILogger _logger;

        public ObservableCollection<PluginInfo> PluginInfos => _pluginInfos;

        // 插件状态改变事件
        public event Action<string, PluginStateChange>? PluginStateChanged;

        public PluginManager(SettingsService settingsService, NotificationService? notificationService = null)
        {
            _settingsService = settingsService;
            _notificationService = notificationService ?? NotificationService.Instance;
            _logger = LoggingConfig.Logger.ForContext<PluginManager>();
            
            // 解析插件目录路径
            var pluginsDir = _settingsService.Settings.PluginsDirectory;
            _pluginsDirectory = Path.GetFullPath(pluginsDir);
            
            // 如果配置的目录不存在，尝试从项目根目录查找
            if (!Directory.Exists(_pluginsDirectory))
            {
                // 尝试向上查找包含 Plugins 文件夹的目录
                var currentDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                while (currentDir != null)
                {
                    var potentialPluginsDir = Path.Combine(currentDir.FullName, "Plugins");
                    if (Directory.Exists(potentialPluginsDir))
                    {
                        _pluginsDirectory = potentialPluginsDir;
                        _logger.Information("找到项目根目录的插件文件夹: {Path}", _pluginsDirectory);
                        break;
                    }
                    currentDir = currentDir.Parent;
                }
            }
            
            _logger.Information("配置的插件目录: {ConfigPath}", pluginsDir);
            _logger.Information("实际使用的插件目录: {FullPath}", _pluginsDirectory);
            
            // 确保插件目录存在
            if (!Directory.Exists(_pluginsDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_pluginsDirectory);
                    _logger.Information("创建插件目录: {Path}", _pluginsDirectory);
                    _notificationService.ShowSuccess($"已创建插件目录: {_pluginsDirectory}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "创建插件目录失败: {Path}", _pluginsDirectory);
                    _notificationService.ShowError($"创建插件目录失败: {ex.Message}");
                }
            }
            else
            {
                _logger.Information("插件目录已存在: {Path}", _pluginsDirectory);
            }
        }

        /// <summary>
        /// 异步加载所有插件
        /// </summary>
        public async Task LoadPluginsAsync()
        {
            if (!_settingsService.Settings.AutoLoadPlugins)
            {
                _logger.Information("自动加载插件已禁用");
                return;
            }

            var dllFiles = Directory.GetFiles(_pluginsDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            _logger.Information("发现 {Count} 个 DLL 文件", dllFiles.Length);
            
            foreach (var dllFile in dllFiles)
            {
                try
                {
                    await LoadPluginAsync(dllFile);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "加载插件失败: {DllPath}", dllFile);
                }
            }
        }

        /// <summary>
        /// 加载单个插件
        /// </summary>
        public async Task<IPlugin?> LoadPluginAsync(string dllPath)
        {
            if (!File.Exists(dllPath))
            {
                _logger.Warning("插件文件不存在: {Path}", dllPath);
                throw new FileNotFoundException("插件文件不存在", dllPath);
            }

            try
            {
                _logger.Information("开始加载插件: {Path}", dllPath);
                
                // 创建隔离的加载上下文
                var context = new PluginLoadContext(dllPath);
                var assembly = context.LoadFromAssemblyPath(Path.GetFullPath(dllPath));

                // 查找实现 IPlugin 接口的类型
                var pluginType = assembly.GetTypes()
                    .FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

                if (pluginType == null)
                    throw new Exception("未找到实现 IPlugin 接口的类型");

                // 实例化插件
                var plugin = (IPlugin?)Activator.CreateInstance(pluginType);
                if (plugin == null)
                    throw new Exception("无法创建插件实例");

                // 初始化插件
                plugin.Initialize();

                // 存储插件和上下文
                _loadedPlugins[plugin.Id] = plugin;
                _pluginContexts[plugin.Id] = context;

                // 创建插件信息
                var pluginInfo = new PluginInfo
                {
                    Id = plugin.Id,
                    Name = plugin.Name,
                    Version = plugin.Version,
                    Description = plugin.Description,
                    Author = plugin.Author,
                    DllPath = dllPath,
                    IsEnabled = true,
                    IsRunning = true,
                    IsLoaded = true,
                    InstalledDate = DateTime.Now
                };

                _pluginInfos.Add(pluginInfo);

                // 更新设置
                if (!_settingsService.Settings.EnabledPlugins.Contains(plugin.Id))
                {
                    _settingsService.Settings.EnabledPlugins.Add(plugin.Id);
                    await _settingsService.SaveSettingsAsync();
                }

                _logger.Information("插件加载成功: {Name} v{Version}", plugin.Name, plugin.Version);
                _notificationService.ShowSuccess($"插件加载成功: {plugin.Name} v{plugin.Version}");
                
                // 启动热重载监视（如果启用）
                var hotReloadManager = DependencyInjection.ServiceContainer.GetService<PluginHotReloadManager>();
                if (hotReloadManager != null)
                {
                    hotReloadManager.StartWatching(plugin.Id, dllPath);
                }
                
                return plugin;
            }
            catch (Exception ex)
            {
                var errorInfo = new PluginInfo
                {
                    Id = Path.GetFileNameWithoutExtension(dllPath),
                    Name = Path.GetFileNameWithoutExtension(dllPath),
                    DllPath = dllPath,
                    IsEnabled = false,
                    IsLoaded = false,
                    Error = ex.Message
                };
                _pluginInfos.Add(errorInfo);
                
                _logger.Error(ex, "加载插件失败: {Path}", dllPath);
                _notificationService.ShowError($"加载插件失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        public void UnloadPlugin(string pluginId)
        {
            if (_loadedPlugins.TryGetValue(pluginId, out var plugin))
            {
                try
                {
                    _logger.Information("开始卸载插件: {PluginId}", pluginId);
                    
                    // 停止热重载监视
                    var hotReloadManager = DependencyInjection.ServiceContainer.GetService<PluginHotReloadManager>();
                    if (hotReloadManager != null)
                    {
                        hotReloadManager.StopWatching(pluginId);
                    }
                    
                    plugin.Shutdown();
                    
                    // 从集合中移除
                    _loadedPlugins.Remove(pluginId);
                    
                    var pluginInfo = _pluginInfos.FirstOrDefault(p => p.Id == pluginId);
                    if (pluginInfo != null)
                    {
                        _pluginInfos.Remove(pluginInfo);
                    }

                    // 卸载加载上下文
                    if (_pluginContexts.TryGetValue(pluginId, out var context))
                    {
                        context.Unload();
                        _pluginContexts.Remove(pluginId);
                    }
                    
                    // 触发状态改变事件
                    PluginStateChanged?.Invoke(pluginId, PluginStateChange.Unloaded);

                    _logger.Information("插件已卸载: {PluginId}", pluginId);
                    _notificationService.ShowInfo($"插件已卸载: {pluginId}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "卸载插件失败: {PluginId}", pluginId);
                }
            }
        }

        /// <summary>
        /// 启用插件
        /// </summary>
        public async Task EnablePluginAsync(string pluginId)
        {
            var pluginInfo = _pluginInfos.FirstOrDefault(p => p.Id == pluginId);
            if (pluginInfo != null)
            {
                pluginInfo.IsEnabled = true;
                
                if (!_settingsService.Settings.EnabledPlugins.Contains(pluginId))
                {
                    _settingsService.Settings.EnabledPlugins.Add(pluginId);
                    await _settingsService.SaveSettingsAsync();
                }

                // 如果插件未加载，则加载它
                if (!pluginInfo.IsLoaded)
                {
                    await LoadPluginAsync(pluginInfo.DllPath);
                }
            }
        }

        /// <summary>
        /// 禁用插件
        /// </summary>
        public async Task DisablePluginAsync(string pluginId)
        {
            var pluginInfo = _pluginInfos.FirstOrDefault(p => p.Id == pluginId);
            if (pluginInfo != null)
            {
                pluginInfo.IsEnabled = false;
                pluginInfo.IsRunning = false;
                
                _settingsService.Settings.EnabledPlugins.Remove(pluginId);
                await _settingsService.SaveSettingsAsync();

                // 停用插件但不卸载
                if (_loadedPlugins.TryGetValue(pluginId, out var plugin))
                {
                    plugin.Deactivate();
                }
            }
        }

        /// <summary>
        /// 停止插件运行（但保持安装状态）
        /// </summary>
        public void StopPlugin(string pluginId)
        {
            var pluginInfo = _pluginInfos.FirstOrDefault(p => p.Id == pluginId);
            if (pluginInfo != null)
            {
                pluginInfo.IsRunning = false;
                
                // 停用插件但不从内存中移除
                if (_loadedPlugins.TryGetValue(pluginId, out var plugin))
                {
                    plugin.Deactivate();
                }
                
                // 触发状态改变事件
                PluginStateChanged?.Invoke(pluginId, PluginStateChange.Stopped);
                
                _logger.Information("插件已停止运行: {PluginId}", pluginId);
                _notificationService.ShowInfo($"插件已停止: {pluginInfo.Name}");
            }
        }

        /// <summary>
        /// 启动插件运行
        /// </summary>
        public void StartPlugin(string pluginId)
        {
            var pluginInfo = _pluginInfos.FirstOrDefault(p => p.Id == pluginId);
            if (pluginInfo != null && pluginInfo.IsEnabled)
            {
                pluginInfo.IsRunning = true;
                
                // 激活插件
                if (_loadedPlugins.TryGetValue(pluginId, out var plugin))
                {
                    plugin.Activate();
                }
                
                // 触发状态改变事件
                PluginStateChanged?.Invoke(pluginId, PluginStateChange.Started);
                
                _logger.Information("插件已启动运行: {PluginId}", pluginId);
                _notificationService.ShowSuccess($"插件已启动: {pluginInfo.Name}");
            }
        }

        /// <summary>
        /// 获取已加载的插件实例
        /// </summary>
        public IPlugin? GetPlugin(string pluginId)
        {
            return _loadedPlugins.TryGetValue(pluginId, out var plugin) ? plugin : null;
        }

        /// <summary>
        /// 激活插件
        /// </summary>
        public void ActivatePlugin(string pluginId)
        {
            if (_loadedPlugins.TryGetValue(pluginId, out var plugin))
            {
                plugin.Activate();
                
                // 更新最后使用时间
                var pluginInfo = _pluginInfos.FirstOrDefault(p => p.Id == pluginId);
                if (pluginInfo != null)
                {
                    pluginInfo.LastUsed = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// 停用插件
        /// </summary>
        public void DeactivatePlugin(string pluginId)
        {
            if (_loadedPlugins.TryGetValue(pluginId, out var plugin))
            {
                plugin.Deactivate();
            }
        }

        /// <summary>
        /// 删除插件文件
        /// </summary>
        public async Task DeletePluginAsync(string pluginId)
        {
            // 先卸载插件
            UnloadPlugin(pluginId);

            var pluginInfo = _pluginInfos.FirstOrDefault(p => p.Id == pluginId);
            if (pluginInfo != null && File.Exists(pluginInfo.DllPath))
            {
                try
                {
                    File.Delete(pluginInfo.DllPath);
                    _pluginInfos.Remove(pluginInfo);
                    
                    _settingsService.Settings.EnabledPlugins.Remove(pluginId);
                    await _settingsService.SaveSettingsAsync();
                    
                    _logger.Information("插件已删除: {PluginId}", pluginId);
                    _notificationService.ShowSuccess($"插件已删除: {pluginId}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "删除插件文件失败: {PluginId}", pluginId);
                    throw;
                }
            }
        }
    }
}

