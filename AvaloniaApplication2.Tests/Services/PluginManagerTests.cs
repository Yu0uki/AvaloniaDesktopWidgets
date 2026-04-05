using AvaloniaApplication2.Core;
using AvaloniaApplication2.Models;
using AvaloniaApplication2.Services;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace AvaloniaApplication2.Tests.Services
{
    /// <summary>
    /// PluginManager 单元测试
    /// </summary>
    public class PluginManagerTests : IDisposable
    {
        private readonly PluginManager _pluginManager;
        private readonly string _testPluginsDir;
        private readonly SettingsService _settingsService;

        public PluginManagerTests()
        {
            // 创建真实的设置服务（用于测试）
            _settingsService = new SettingsService();
            var testSettings = new AppSettings
            {
                PluginsDirectory = Path.Combine(Path.GetTempPath(), "TestPlugins_" + Guid.NewGuid().ToString()),
                AutoLoadPlugins = false,
                EnabledPlugins = new List<string>()
            };
            
            // 使用反射设置测试路径
            var fieldInfo = typeof(SettingsService).GetField("_settings", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(_settingsService, testSettings);
            }

            // 创建测试插件目录
            _testPluginsDir = testSettings.PluginsDirectory;
            if (!Directory.Exists(_testPluginsDir))
            {
                Directory.CreateDirectory(_testPluginsDir);
            }

            // 创建插件管理器实例
            _pluginManager = new PluginManager(_settingsService, NotificationService.Instance);
        }

        [Fact]
        public async Task LoadPluginAsync_InvalidPath_ThrowsFileNotFoundException()
        {
            // Arrange
            var invalidPath = "nonexistent.dll";

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(
                () => _pluginManager.LoadPluginAsync(invalidPath)
            );
        }

        [Fact]
        public void PluginInfos_ShouldReturnObservableCollection()
        {
            // Act
            var pluginInfos = _pluginManager.PluginInfos;

            // Assert
            Assert.NotNull(pluginInfos);
            Assert.IsType<System.Collections.ObjectModel.ObservableCollection<PluginInfo>>(pluginInfos);
        }

        [Fact]
        public async Task EnablePluginAsync_ValidPluginId_ShouldEnablePlugin()
        {
            // Arrange
            var pluginId = "TestPlugin";
            var pluginInfo = new PluginInfo
            {
                Id = pluginId,
                Name = "Test Plugin",
                DllPath = Path.Combine(_testPluginsDir, "test.dll"),
                IsEnabled = false,
                IsLoaded = false
            };
            _pluginManager.PluginInfos.Add(pluginInfo);

            // Act
            await _pluginManager.EnablePluginAsync(pluginId);

            // Assert
            Assert.True(pluginInfo.IsEnabled);
        }

        [Fact]
        public async Task DisablePluginAsync_ValidPluginId_ShouldDisablePlugin()
        {
            // Arrange
            var pluginId = "TestPlugin";
            var pluginInfo = new PluginInfo
            {
                Id = pluginId,
                Name = "Test Plugin",
                DllPath = Path.Combine(_testPluginsDir, "test.dll"),
                IsEnabled = true,
                IsLoaded = true
            };
            _pluginManager.PluginInfos.Add(pluginInfo);

            // Act
            await _pluginManager.DisablePluginAsync(pluginId);

            // Assert
            Assert.False(pluginInfo.IsEnabled);
        }

        [Fact]
        public void GetPlugin_NonExistentPlugin_ReturnsNull()
        {
            // Act
            var plugin = _pluginManager.GetPlugin("NonExistent");

            // Assert
            Assert.Null(plugin);
        }

        private void Dispose()
        {
            // 清理测试目录
            if (Directory.Exists(_testPluginsDir))
            {
                try
                {
                    Directory.Delete(_testPluginsDir, true);
                }
                catch
                {
                    // 忽略清理错误
                }
            }
        }

        void IDisposable.Dispose() => Dispose();
    }
}
