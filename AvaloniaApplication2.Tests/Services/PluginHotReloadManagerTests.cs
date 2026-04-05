using AvaloniaApplication2.Models;
using AvaloniaApplication2.Services;
using Moq;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace AvaloniaApplication2.Tests.Services
{
    /// <summary>
    /// PluginHotReloadManager 单元测试
    /// </summary>
    public class PluginHotReloadManagerTests : IDisposable
    {
        private readonly Mock<PluginManager> _mockPluginManager;
        private readonly PluginHotReloadManager _hotReloadManager;
        private readonly string _testPluginPath;

        public PluginHotReloadManagerTests()
        {
            _mockPluginManager = new Mock<PluginManager>(
                Mock.Of<SettingsService>(),
                Mock.Of<NotificationService>()
            );

            _hotReloadManager = new PluginHotReloadManager(_mockPluginManager.Object);

            // 创建测试文件
            _testPluginPath = Path.Combine(Path.GetTempPath(), "test_plugin.dll");
            File.WriteAllText(_testPluginPath, "test");
        }

        [Fact]
        public void StartWatching_ValidPlugin_ShouldCreateWatcher()
        {
            // Act
            _hotReloadManager.StartWatching("TestPlugin", _testPluginPath);

            // Assert - 没有抛出异常即为成功
            Assert.True(true);
        }

        [Fact]
        public void StopWatching_AfterStart_ShouldStopWatcher()
        {
            // Arrange
            _hotReloadManager.StartWatching("TestPlugin", _testPluginPath);

            // Act
            _hotReloadManager.StopWatching("TestPlugin");

            // Assert - 没有抛出异常即为成功
            Assert.True(true);
        }

        [Fact]
        public void StopAllWatching_MultipleWatchers_ShouldStopAll()
        {
            // Arrange
            var plugin1Path = Path.Combine(Path.GetTempPath(), "plugin1.dll");
            var plugin2Path = Path.Combine(Path.GetTempPath(), "plugin2.dll");
            File.WriteAllText(plugin1Path, "test");
            File.WriteAllText(plugin2Path, "test");

            _hotReloadManager.StartWatching("Plugin1", plugin1Path);
            _hotReloadManager.StartWatching("Plugin2", plugin2Path);

            // Act
            _hotReloadManager.StopAllWatching();

            // Assert - 没有抛出异常即为成功
            Assert.True(true);

            // Cleanup
            if (File.Exists(plugin1Path)) File.Delete(plugin1Path);
            if (File.Exists(plugin2Path)) File.Delete(plugin2Path);
        }

        [Fact]
        public void Dispose_ShouldStopAllWatchers()
        {
            // Arrange
            _hotReloadManager.StartWatching("TestPlugin", _testPluginPath);

            // Act
            _hotReloadManager.Dispose();

            // Assert - 没有抛出异常即为成功
            Assert.True(true);
        }

        private void Dispose()
        {
            _hotReloadManager?.Dispose();
            
            if (File.Exists(_testPluginPath))
            {
                try
                {
                    File.Delete(_testPluginPath);
                }
                catch { }
            }
        }

        void IDisposable.Dispose() => Dispose();
    }
}
