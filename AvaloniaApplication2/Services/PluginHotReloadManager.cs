using AvaloniaApplication2.Core;
using AvaloniaApplication2.Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaApplication2.Services
{
    /// <summary>
    /// 插件热重载管理器 - 监听插件文件变化并自动重新加载
    /// </summary>
    public class PluginHotReloadManager : IDisposable
    {
        private readonly PluginManager _pluginManager;
        private readonly ILogger _logger;
        private readonly Dictionary<string, FileSystemWatcher> _watchers = new();
        private readonly Dictionary<string, DateTime> _lastWriteTimes = new();
        private bool _isDisposed = false;

        public PluginHotReloadManager(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
            _logger = LoggingConfig.Logger.ForContext<PluginHotReloadManager>();
        }

        /// <summary>
        /// 开始监视插件文件变化
        /// </summary>
        public void StartWatching(string pluginId, string dllPath)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PluginHotReloadManager));

            if (_watchers.ContainsKey(pluginId))
            {
                _logger.Warning("插件 {PluginId} 已在监视中", pluginId);
                return;
            }

            try
            {
                var directory = Path.GetDirectoryName(dllPath);
                if (string.IsNullOrEmpty(directory))
                    return;

                var watcher = new FileSystemWatcher(directory)
                {
                    Filter = Path.GetFileName(dllPath),
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };

                watcher.Changed += async (sender, e) => await OnPluginFileChanged(pluginId, e.FullPath);
                watcher.Created += async (sender, e) => await OnPluginFileChanged(pluginId, e.FullPath);

                _watchers[pluginId] = watcher;
                _lastWriteTimes[pluginId] = File.GetLastWriteTime(dllPath);

                _logger.Information("开始监视插件: {PluginId}", pluginId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "无法监视插件: {PluginId}", pluginId);
            }
        }

        /// <summary>
        /// 停止监视指定插件
        /// </summary>
        public void StopWatching(string pluginId)
        {
            if (_watchers.TryGetValue(pluginId, out var watcher))
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                _watchers.Remove(pluginId);
                _lastWriteTimes.Remove(pluginId);
                
                _logger.Information("停止监视插件: {PluginId}", pluginId);
            }
        }

        /// <summary>
        /// 停止所有监视
        /// </summary>
        public void StopAllWatching()
        {
            foreach (var pluginId in _watchers.Keys.ToList())
            {
                StopWatching(pluginId);
            }
        }

        /// <summary>
        /// 插件文件变化处理
        /// </summary>
        private async Task OnPluginFileChanged(string pluginId, string filePath)
        {
            try
            {
                // 防抖：避免短时间内多次触发
                var lastWriteTime = File.GetLastWriteTime(filePath);
                if (_lastWriteTimes.TryGetValue(pluginId, out var lastTime))
                {
                    if ((lastWriteTime - lastTime).TotalMilliseconds < 500)
                    {
                        return;
                    }
                }

                _lastWriteTimes[pluginId] = lastWriteTime;

                _logger.Information("检测到插件文件变化: {PluginId}", pluginId);

                // 等待文件写入完成
                await Task.Delay(1000);

                // 检查文件是否可访问
                if (!IsFileAccessible(filePath))
                {
                    _logger.Warning("插件文件仍被锁定，稍后重试: {Path}", filePath);
                    return;
                }

                // 执行热重载
                await HotReloadPluginAsync(pluginId, filePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "处理插件文件变化失败: {PluginId}", pluginId);
            }
        }

        /// <summary>
        /// 检查文件是否可访问
        /// </summary>
        private bool IsFileAccessible(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 执行插件热重载
        /// </summary>
        private async Task HotReloadPluginAsync(string pluginId, string filePath)
        {
            try
            {
                _logger.Information("开始热重载插件: {PluginId}", pluginId);

                // 1. 停用插件
                _pluginManager.DeactivatePlugin(pluginId);

                // 2. 卸载插件
                _pluginManager.UnloadPlugin(pluginId);

                // 3. 等待资源释放
                await Task.Delay(500);

                // 4. 重新加载插件
                var plugin = await _pluginManager.LoadPluginAsync(filePath);

                if (plugin != null)
                {
                    // 5. 重新开始监视
                    StopWatching(pluginId);
                    StartWatching(pluginId, filePath);

                    // 6. 激活插件
                    _pluginManager.ActivatePlugin(pluginId);

                    _logger.Information("插件热重载成功: {PluginName} v{Version}", 
                        plugin.Name, plugin.Version);
                }
                else
                {
                    _logger.Error("插件热重载失败: {PluginId}", pluginId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "热重载插件时发生错误: {PluginId}", pluginId);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                StopAllWatching();
                _isDisposed = true;
            }
        }
    }
}
