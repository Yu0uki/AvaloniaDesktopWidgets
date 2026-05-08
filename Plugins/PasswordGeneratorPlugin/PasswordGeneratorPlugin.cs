using AvaloniaApplication2.Core;
using Avalonia.Controls;
using System.IO;

namespace PasswordGeneratorPlugin
{
    public class PasswordGeneratorPlugin : IPlugin
    {
        public string Id => "PasswordGenerator";
        public string Name => "密码生成器";
        public string Version => "1.0.0";
        public string Description => "安全密码生成工具，支持自定义长度、大小写、数字和特殊字符";
        public string Author => "Efficiency Workshop Team";

        public void Initialize() { }
        public void Activate() { }
        public void Deactivate() { }
        public void Shutdown() { }

        public Control GetMainView() => new MainView();
        public Control? GetSettingsView() => null;
        public Stream? GetIcon() => null;
    }
}
