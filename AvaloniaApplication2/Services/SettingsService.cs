using AvaloniaApplication2.Models;
using AvaloniaApplication2.Infrastructure;
using Serilog;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System;

namespace AvaloniaApplication2.Services
{
    /// <summary>
    /// 设置服务 - 管理应用程序设置的持久化
    /// </summary>
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings _settings;
        private readonly ILogger _logger;

        public AppSettings Settings => _settings;

        public SettingsService()
        {
            _logger = LoggingConfig.Logger.ForContext<SettingsService>();
            
            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
                _logger.Information("创建设置目录: {Path}", dataDirectory);
            }

            _settingsFilePath = Path.Combine(dataDirectory, "settings.json");
            _settings = LoadSettings();
        }

        /// <summary>
        /// 加载设置
        /// </summary>
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
        /// 保存设置
        /// </summary>
        public async Task SaveSettingsAsync()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(_settings, options);
                await File.WriteAllTextAsync(_settingsFilePath, json);
                
                _logger.Debug("设置已保存: {Path}", _settingsFilePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "保存设置失败: {Path}", _settingsFilePath);
                throw;
            }
        }

        /// <summary>
        /// 更新主题设置
        /// </summary>
        public async Task UpdateThemeAsync(string theme)
        {
            _settings.Theme = theme;
            await SaveSettingsAsync();
        }

        /// <summary>
        /// 更新插件目录
        /// </summary>
        public async Task UpdatePluginsDirectoryAsync(string path)
        {
            _settings.PluginsDirectory = path;
            await SaveSettingsAsync();
        }

        /// <summary>
        /// 更新自动加载设置
        /// </summary>
        public async Task UpdateAutoLoadPluginsAsync(bool enabled)
        {
            _settings.AutoLoadPlugins = enabled;
            await SaveSettingsAsync();
        }
    }
}
