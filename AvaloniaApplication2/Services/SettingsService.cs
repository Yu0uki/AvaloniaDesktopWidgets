using AvaloniaApplication2.Models;
using AvaloniaApplication2.Infrastructure;
using Serilog;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaApplication2.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private readonly string _dataDirectory;
        private AppSettings _settings;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private const int CurrentConfigVersion = 1;

        public AppSettings Settings => _settings;

        public SettingsService()
        {
            _logger = LoggingConfig.Logger.ForContext<SettingsService>();

            _dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
                _logger.Information("创建设置目录: {Path}", _dataDirectory);
            }

            _settingsFilePath = Path.Combine(_dataDirectory, "settings.json");
            _settings = LoadSettings();
            MigrateIfNeeded();
        }

        private AppSettings LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    _logger.Information("设置已加载: {Path}", _settingsFilePath);
                    return settings ?? new AppSettings();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "加载设置失败: {Path}", _settingsFilePath);
                    return new AppSettings();
                }
            }

            _logger.Information("设置文件不存在，使用默认设置");
            return new AppSettings();
        }

        /// <summary>
        /// 配置版本迁移
        /// </summary>
        private void MigrateIfNeeded()
        {
            if (_settings.ConfigVersion >= CurrentConfigVersion) return;
            if (_settings.ConfigVersion == 0)
            {
                _logger.Information("从配置版本 0 迁移到 {Version}", CurrentConfigVersion);
                _settings.ConfigVersion = CurrentConfigVersion;
                if (_settings.PluginSettings == null)
                    _settings.PluginSettings = new();
                if (_settings.NotificationLog == null)
                    _settings.NotificationLog = new();
                _ = SaveSettingsAsync();
            }
        }

        /// <summary>
        /// 原子写入：临时文件 → 原子替换，SemaphoreSlim 防止并发冲突
        /// </summary>
        public async Task SaveSettingsAsync()
        {
            await _writeLock.WaitAsync();
            try
            {
                var tmpPath = _settingsFilePath + ".tmp";
                var json = JsonSerializer.Serialize(_settings, JsonOptions);
                await File.WriteAllTextAsync(tmpPath, json);
                File.Move(tmpPath, _settingsFilePath, overwrite: true);
                _logger.Debug("设置已保存: {Path}", _settingsFilePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "保存设置失败: {Path}", _settingsFilePath);
                throw;
            }
            finally
            {
                _writeLock.Release();
            }
        }

        // ===== 插件元数据持久化 =====

        /// <summary>
        /// 读取插件设置原始 JSON，返回 null 表示无保存数据
        /// </summary>
        public async Task<string?> GetPluginSettingsRawAsync(string pluginId)
        {
            var path = GetPluginSettingsPath(pluginId);
            if (!File.Exists(path)) return null;
            try { return await File.ReadAllTextAsync(path); }
            catch { return null; }
        }

        /// <summary>
        /// 保存插件设置（原始 JSON 字符串）
        /// </summary>
        public async Task SavePluginSettingsRawAsync(string pluginId, string json)
        {
            var path = GetPluginSettingsPath(pluginId);
            await _writeLock.WaitAsync();
            try
            {
                var tmpPath = path + ".tmp";
                await File.WriteAllTextAsync(tmpPath, json);
                File.Move(tmpPath, path, overwrite: true);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task<T?> GetPluginSettingsAsync<T>(string pluginId) where T : class, new()
        {
            var path = GetPluginSettingsPath(pluginId);
            if (!File.Exists(path)) return new T();

            try
            {
                var json = await File.ReadAllTextAsync(path);
                return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "加载插件设置失败: {PluginId}", pluginId);
                return new T();
            }
        }

        public async Task SavePluginSettingsAsync<T>(string pluginId, T data)
        {
            var path = GetPluginSettingsPath(pluginId);
            await _writeLock.WaitAsync();
            try
            {
                var tmpPath = path + ".tmp";
                var json = JsonSerializer.Serialize(data, JsonOptions);
                await File.WriteAllTextAsync(tmpPath, json);
                File.Move(tmpPath, path, overwrite: true);
                _logger.Debug("插件设置已保存: {PluginId}", pluginId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "保存插件设置失败: {PluginId}", pluginId);
                throw;
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private string GetPluginSettingsPath(string pluginId)
        {
            var safeId = SanitizeFileName(pluginId);
            return Path.Combine(_dataDirectory, $"plugin_{safeId}.json");
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        // ===== Dashboard 布局持久化 =====

        public async Task SaveDashboardLayoutAsync<T>(T layout) where T : class
        {
            var path = Path.Combine(_dataDirectory, "dashboard_layout.json");
            await _writeLock.WaitAsync();
            try
            {
                var tmpPath = path + ".tmp";
                var json = JsonSerializer.Serialize(layout, JsonOptions);
                await File.WriteAllTextAsync(tmpPath, json);
                File.Move(tmpPath, path, overwrite: true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "保存仪表盘布局失败");
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task<T?> GetDashboardLayoutAsync<T>() where T : class, new()
        {
            var path = Path.Combine(_dataDirectory, "dashboard_layout.json");
            if (!File.Exists(path)) return new T();

            try
            {
                var json = await File.ReadAllTextAsync(path);
                return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "加载仪表盘布局失败");
                return new T();
            }
        }

        // ===== 通知日志持久化 =====

        public async Task AddNotificationLogAsync(string message, string type)
        {
            _settings.NotificationLog.Insert(0, new NotificationLogEntry
            {
                Message = message,
                Type = type,
                Timestamp = DateTime.Now
            });

            while (_settings.NotificationLog.Count > 50)
                _settings.NotificationLog.RemoveAt(_settings.NotificationLog.Count - 1);

            await SaveSettingsAsync();
        }

        public async Task ClearNotificationLogAsync()
        {
            _settings.NotificationLog.Clear();
            await SaveSettingsAsync();
        }

        // ===== 单个设置更新方法 =====

        public async Task UpdateThemeAsync(string theme)
        {
            _settings.Theme = theme;
            await SaveSettingsAsync();
        }

        public async Task UpdatePluginsDirectoryAsync(string path)
        {
            _settings.PluginsDirectory = path;
            await SaveSettingsAsync();
        }

        public async Task UpdateAutoLoadPluginsAsync(bool enabled)
        {
            _settings.AutoLoadPlugins = enabled;
            await SaveSettingsAsync();
        }

        public async Task UpdateAccentColorAsync(int accentColorIndex)
        {
            _settings.AccentColorIndex = accentColorIndex;
            await SaveSettingsAsync();
        }

        public async Task UpdateLanguageAsync(int languageIndex)
        {
            _settings.LanguageIndex = languageIndex;
            await SaveSettingsAsync();
        }

        public async Task UpdateBackgroundOpacityAsync(double opacity)
        {
            _settings.BackgroundOpacity = opacity;
            await SaveSettingsAsync();
        }

        public async Task UpdateCustomBackgroundImageAsync(bool enabled)
        {
            _settings.CustomBackgroundImage = enabled;
            await SaveSettingsAsync();
        }

        public async Task UpdateBackgroundImagePathAsync(string path)
        {
            _settings.BackgroundImagePath = path;
            await SaveSettingsAsync();
        }

        public async Task UpdateDefaultStartupPageAsync(int index)
        {
            _settings.DefaultStartupPageIndex = index;
            await SaveSettingsAsync();
        }

        public async Task UpdateSidebarLayoutAsync(bool expanded, double width)
        {
            _settings.SidebarExpanded = expanded;
            _settings.SidebarWidth = width;
            await SaveSettingsAsync();
        }

        public async Task UpdateWindowPositionAsync(double? x, double? y)
        {
            _settings.WindowX = x;
            _settings.WindowY = y;
            await SaveSettingsAsync();
        }
    }
}
