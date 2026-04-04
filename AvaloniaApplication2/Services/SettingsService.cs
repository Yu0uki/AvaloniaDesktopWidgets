using AvaloniaApplication2.Models;
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

        public AppSettings Settings => _settings;

        public SettingsService()
        {
            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
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
                    return settings ?? new AppSettings();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载设置失败: {ex.Message}");
                    return new AppSettings();
                }
            }

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
                
                Console.WriteLine("设置已保存");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存设置失败: {ex.Message}");
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
