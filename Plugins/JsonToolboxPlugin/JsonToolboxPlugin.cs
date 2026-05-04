using AvaloniaApplication2.Core;
using Avalonia.Controls;
using System.IO;

namespace JsonToolboxPlugin
{
    /// <summary>
    /// JSON 工具箱插件 - 提供 JSON 格式化、压缩、验证、转义等常用工具
    /// </summary>
    public class JsonToolboxPlugin : IPlugin
    {
        public string Id => "JsonToolbox";
        public string Name => "JSON 工具箱";
        public string Version => "1.0.0";
        public string Description => "JSON 格式化、压缩、验证、转义/反转义、C# 类生成等开发常用工具";
        public string Author => "Efficiency Workshop";

        public void Initialize() { }

        public void Activate() { }

        public void Deactivate() { }

        public void Shutdown() { }

        public Control GetMainView()
        {
            return new MainView();
        }

        public Control? GetSettingsView()
        {
            return new SettingsView();
        }

        public Stream? GetIcon() => null;
    }
}
