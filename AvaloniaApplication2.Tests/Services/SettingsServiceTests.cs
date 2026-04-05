using AvaloniaApplication2.Models;
using AvaloniaApplication2.Services;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;

namespace AvaloniaApplication2.Tests.Services
{
    /// <summary>
    /// SettingsService 单元测试
    /// </summary>
    public class SettingsServiceTests : IDisposable
    {
        private readonly string _testSettingsPath;

        public SettingsServiceTests()
        {
            // 为测试创建临时设置文件路径
            _testSettingsPath = Path.Combine(Path.GetTempPath(), "test_settings.json");
        }

        [Fact]
        public void SettingsService_DefaultSettings_ShouldHaveCorrectValues()
        {
            // Arrange & Act
            var service = new SettingsService();

            // Assert
            Assert.NotNull(service.Settings);
            Assert.Equal("Dark", service.Settings.Theme);
            Assert.True(service.Settings.AutoLoadPlugins);
            Assert.NotNull(service.Settings.EnabledPlugins);
        }

        [Fact]
        public async Task SaveSettingsAsync_ValidSettings_ShouldSaveToFile()
        {
            // Arrange
            var service = new SettingsService();
            service.Settings.Theme = "Light";
            service.Settings.AutoLoadPlugins = false;

            // Act
            await service.SaveSettingsAsync();

            // Assert
            Assert.True(File.Exists(GetSettingsFilePath(service)));
            
            var savedJson = File.ReadAllText(GetSettingsFilePath(service));
            var savedSettings = JsonSerializer.Deserialize<AppSettings>(savedJson);
            
            Assert.NotNull(savedSettings);
            Assert.Equal("Light", savedSettings.Theme);
            Assert.False(savedSettings.AutoLoadPlugins);
        }

        [Fact]
        public async Task UpdateThemeAsync_ShouldUpdateThemeAndSave()
        {
            // Arrange
            var service = new SettingsService();

            // Act
            await service.UpdateThemeAsync("Light");

            // Assert
            Assert.Equal("Light", service.Settings.Theme);
        }

        [Fact]
        public async Task UpdateAutoLoadPluginsAsync_ShouldUpdateSettingAndSave()
        {
            // Arrange
            var service = new SettingsService();

            // Act
            await service.UpdateAutoLoadPluginsAsync(false);

            // Assert
            Assert.False(service.Settings.AutoLoadPlugins);
        }

        private string GetSettingsFilePath(SettingsService service)
        {
            // 使用反射获取私有字段（仅用于测试）
            var fieldInfo = typeof(SettingsService).GetField("_settingsFilePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return fieldInfo?.GetValue(service)?.ToString() ?? "";
        }

        private void Dispose()
        {
            // 清理测试文件
            if (File.Exists(_testSettingsPath))
            {
                File.Delete(_testSettingsPath);
            }
        }

        void IDisposable.Dispose() => Dispose();
    }
}
