using Avalonia.Threading;
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
        private FileSystemWatcher? _folderWatcher;
        private readonly HashSet<string> _pendingOps = new();
        private readonly object _lock = new();

        public ObservableCollection<PluginInfo> PluginInfos => _pluginInfos;

        public string PluginsDirectory => _pluginsDirectory;

        // 插件状态改变事件
        public event Action<string, PluginStateChange>? PluginStateChanged;

        /// <summary>
        /// 确保操作在 UI 线程执行（ObservableCollection 非线程安全）
        /// </summary>
        private void RunOnUI(Action action)
        {
            if (Dispatcher.UIThread.CheckAccess())
                action();
            else
                Dispatcher.UIThread.Post(action);
        }

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

            // 启动 Plugins 文件夹热重载监视
            StartFolderWatcher();
        }

        /// <summary>
        /// 启动 Plugins 文件夹监视（新增/删除/改名）
        /// </summary>
        private void StartFolderWatcher()
        {
            try
            {
                _folderWatcher = new FileSystemWatcher(_pluginsDirectory, "*.dll")
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };

                _folderWatcher.Created += async (sender, e) =>
                {
                    await System.Threading.Tasks.Task.Delay(600);
                    await OnPluginFileCreatedAsync(e.FullPath);
                };

                _folderWatcher.Deleted += (sender, e) =>
                {
                    OnPluginFileDeleted(e.FullPath);
                };

                _folderWatcher.Renamed += (sender, e) =>
                {
                    OnPluginFileDeleted(e.OldFullPath);
                    _ = OnPluginFileCreatedAsync(e.FullPath);
                };

                _folderWatcher.Changed += async (sender, e) =>
                {
                    await System.Threading.Tasks.Task.Delay(600);
                    await OnPluginFileCreatedAsync(e.FullPath);
                };

                _logger.Information("Plugins 文件夹热重载监视已启动: {Path}", _pluginsDirectory);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "启动文件夹监视失败");
            }
        }

        /// <summary>
        /// 新 DLL 创建时自动加载
        /// </summary>
        private async System.Threading.Tasks.Task OnPluginFileCreatedAsync(string filePath)
        {
            filePath = Path.GetFullPath(filePath);
            if (!File.Exists(filePath)) return;

            lock (_lock)
            {
                if (_pendingOps.Contains(filePath)) return;
                if (_pluginInfos.Any(p =>
                    string.Equals(p.DllPath, filePath, StringComparison.OrdinalIgnoreCase)))
                    return;
                _pendingOps.Add(filePath);
            }

            try
            {
                _logger.Information("检测到插件 DLL: {Path}", filePath);
                _notificationService.ShowInfo($"检测到插件: {Path.GetFileName(filePath)}");

                var plugin = await LoadPluginAsync(filePath);
                if (plugin != null)
                {
                    _notificationService.ShowSuccess($"插件已加载: {plugin.Name} v{plugin.Version}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "自动加载失败: {Path}", filePath);
            }
            finally
            {
                lock (_lock) { _pendingOps.Remove(filePath); }
            }
        }

        /// <summary>
        /// DLL 被删除时自动卸载对应插件
        /// </summary>
        private void OnPluginFileDeleted(string filePath)
        {
            filePath = Path.GetFullPath(filePath);
            lock (_lock)
            {
                if (_pendingOps.Contains(filePath)) return;
                _pendingOps.Add(filePath);
            }

            try
            {
                // 通过 DLL 路径匹配查找被删除的插件
                var pluginInfo = _pluginInfos.FirstOrDefault(p =>
                    string.Equals(p.DllPath, filePath, StringComparison.OrdinalIgnoreCase));

                if (pluginInfo != null)
                {
                    var name = pluginInfo.Name;
                    _logger.Information("检测到插件 DLL 已删除: {Path}, 卸载插件 {PluginId}", filePath, pluginInfo.Id);
                    UnloadPlugin(pluginInfo.Id);
                    _notificationService.ShowInfo($"插件已自动卸载: {name}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "自动卸载失败: {Path}", filePath);
            }
            finally
            {
                lock (_lock) { _pendingOps.Remove(filePath); }
            }
        }

        /// <summary>
        /// 手动同步：扫描 ./Plugins 文件夹，加载新 DLL，移除已删除的插件
        /// </summary>
        public async Task SyncPluginsFromFolderAsync()
        {
            if (!Directory.Exists(_pluginsDirectory))
            {
                _logger.Warning("插件目录不存在: {Path}", _pluginsDirectory);
                return;
            }

            var dllFiles = Directory.GetFiles(_pluginsDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            _logger.Information("同步插件: 文件夹中有 {Count} 个 DLL", dllFiles.Length);

            // 1. 移除 DLL 文件已不存在的插件
            var toRemove = _pluginInfos
                .Where(p => !dllFiles.Any(f =>
                    string.Equals(f, p.DllPath, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var info in toRemove)
            {
                _logger.Information("同步移除无效插件: {Id} ({DllPath})", info.Id, info.DllPath);
                UnloadPlugin(info.Id);
            }

            // 2. 加载新 DLL
            foreach (var dllFile in dllFiles)
            {
                if (_pluginInfos.Any(p =>
                    string.Equals(p.DllPath, dllFile, StringComparison.OrdinalIgnoreCase)))
                    continue;

                try
                {
                    await LoadPluginAsync(dllFile);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "同步加载插件失败: {Path}", dllFile);
                }
            }

            _notificationService.ShowSuccess($"插件同步完成: {_pluginInfos.Count(p => p.IsLoaded)} 个已加载");
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
        /// 加载单个插件（基于 DLL 路径和插件 ID 去重）
        /// </summary>
        public async Task<IPlugin?> LoadPluginAsync(string dllPath)
        {
            if (!File.Exists(dllPath))
            {
                var msg = $"文件不存在: {Path.GetFileName(dllPath)}";
                _logger.Warning("插件加载失败: {Msg}", msg);
                _notificationService.ShowError($"插件加载失败: {msg}");
                return null;
            }

            // 规范化路径，确保与文件夹监视器路径一致
            dllPath = Path.GetFullPath(dllPath);

            // 去重：已加载过的 DLL 路径不重复加载
            lock (_lock)
            {
                if (_pluginInfos.Any(p =>
                    string.Equals(p.DllPath, dllPath, StringComparison.OrdinalIgnoreCase)))
                {
                    var msg = $"插件已安装，跳过重复加载: {Path.GetFileName(dllPath)}";
                    _logger.Information(msg);
                    _notificationService.ShowWarning(msg);
                    return null;
                }
            }

            try
            {
                _logger.Information("开始加载插件: {Path}", dllPath);

                // 创建隔离的加载上下文
                var context = new PluginLoadContext(dllPath);
                Assembly assembly;
                try
                {
                    assembly = context.LoadFromAssemblyPath(Path.GetFullPath(dllPath));
                }
                catch (Exception ex)
                {
                    var msg = $"DLL 加载失败（可能损坏或依赖缺失）: {Path.GetFileName(dllPath)}";
                    _logger.Error(ex, msg);
                    _notificationService.ShowError(msg);
                    throw;
                }

                // 查找实现 IPlugin 接口的类型
                var pluginType = assembly.GetTypes()
                    .FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

                if (pluginType == null)
                {
                    var msg = $"非有效插件 DLL（未实现 IPlugin）: {Path.GetFileName(dllPath)}";
                    _logger.Error(msg);
                    _notificationService.ShowError(msg);
                    throw new InvalidOperationException(msg);
                }

                // 实例化插件
                IPlugin? plugin;
                try
                {
                    plugin = (IPlugin?)Activator.CreateInstance(pluginType);
                }
                catch (Exception ex)
                {
                    var msg = $"插件实例化失败: {Path.GetFileName(dllPath)}";
                    _logger.Error(ex, msg);
                    _notificationService.ShowError(msg);
                    throw;
                }

                if (plugin == null)
                {
                    var msg = $"无法创建插件实例: {Path.GetFileName(dllPath)}";
                    _logger.Error(msg);
                    _notificationService.ShowError(msg);
                    throw new InvalidOperationException(msg);
                }

                // 初始化插件
                plugin.Initialize();

                // 去重：相同 ID 的插件已存在则跳过
                if (_loadedPlugins.ContainsKey(plugin.Id))
                {
                    var msg = $"插件 ID 冲突，已存在同 ID 插件: {plugin.Id} ({Path.GetFileName(dllPath)})";
                    _logger.Warning(msg);
                    _notificationService.ShowError(msg);
                    return null;
                }

                // 存储插件和上下文
                _loadedPlugins[plugin.Id] = plugin;
                _pluginContexts[plugin.Id] = context;

                // 创建插件信息（Author 统一为 Efficiency Workshop Team）
                var pluginInfo = new PluginInfo
                {
                    Id = plugin.Id,
                    Name = plugin.Name,
                    Version = plugin.Version,
                    Description = plugin.Description,
                    Author = "Efficiency Workshop Team",
                    DllPath = dllPath,
                    IsEnabled = true,
                    IsRunning = true,
                    IsLoaded = true,
                    InstalledDate = DateTime.Now
                };

                RunOnUI(() => _pluginInfos.Add(pluginInfo));

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
                RunOnUI(() => _pluginInfos.Add(errorInfo));

                _logger.Error(ex, "加载插件失败: {Path}", dllPath);
                _notificationService.ShowError($"加载插件失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 卸载插件（清理 PluginInfo、已加载实例、上下文，无论是否加载成功）
        /// </summary>
        public void UnloadPlugin(string pluginId)
        {
            RunOnUI(() =>
            {
                var pluginInfo = _pluginInfos.FirstOrDefault(p => p.Id == pluginId);
                if (pluginInfo != null)
                {
                    _pluginInfos.Remove(pluginInfo);
                }
            });

            // 2. 如果有已加载的插件实例
            if (_loadedPlugins.TryGetValue(pluginId, out var plugin))
            {
                try
                {
                    _logger.Information("开始卸载插件: {PluginId}", pluginId);

                    var hotReloadManager = DependencyInjection.ServiceContainer.GetService<PluginHotReloadManager>();
                    hotReloadManager?.StopWatching(pluginId);

                    plugin.Shutdown();
                    _loadedPlugins.Remove(pluginId);

                    if (_pluginContexts.TryGetValue(pluginId, out var context))
                    {
                        context.Unload();
                        _pluginContexts.Remove(pluginId);
                    }

                    PluginStateChanged?.Invoke(pluginId, PluginStateChange.Unloaded);

                    _logger.Information("插件已卸载: {PluginId}", pluginId);
                    _notificationService.ShowInfo($"插件已卸载: {plugin.Name}");
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
        /// 停止文件夹监视
        /// </summary>
        public void StopFolderWatcher()
        {
            if (_folderWatcher != null)
            {
                _folderWatcher.EnableRaisingEvents = false;
                _folderWatcher.Dispose();
                _folderWatcher = null;
                _logger.Information("已停止 Plugins 文件夹监视");
            }
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

