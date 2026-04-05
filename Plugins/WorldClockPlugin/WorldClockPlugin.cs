using AvaloniaApplication2.Core;
using Avalonia.Controls;
using System.IO;

namespace WorldClockPlugin
{
    /// <summary>
    /// 世界时钟插件主类
    /// </summary>
    public class WorldClockPlugin : IPlugin
    {
        public string Id => "WorldClock";
        public string Name => "世界时钟";
        public string Version => "1.0.0";
        public string Description => "显示当前系统时间，支持时区切换、秒表和计时器功能";
        public string Author => "AvaloniaApplication2 Team";

        public void Initialize()
        {
            // 初始化插件
        }

        public void Activate()
        {
            // 激活插件时的处理
        }

        public void Deactivate()
        {
            // 停用插件时的清理工作
        }

        public void Shutdown()
        {
            // 关闭插件时的清理工作
        }

        public Control GetMainView()
        {
            // ⭐ 关键修复：每次都创建新实例，避免 Visual Tree 冲突
            return new WorldClockView();
        }

        public Control? GetSettingsView()
        {
            // ⭐ 关键修复：每次都创建新实例，避免 Visual Tree 冲突
            return new SettingsView();
        }

        public Stream? GetIcon()
        {
            // 返回插件图标（可选）
            return null;
        }
    }
}
