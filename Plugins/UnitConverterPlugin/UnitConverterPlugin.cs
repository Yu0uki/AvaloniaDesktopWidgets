using AvaloniaApplication2.Core;
using Avalonia.Controls;
using System.IO;

namespace UnitConverterPlugin
{
    public class UnitConverterPlugin : IPlugin
    {
        public string Id => "UnitConverter";
        public string Name => "单位换算器";
        public string Version => "1.0.0";
        public string Description => "常用单位换算工具，支持长度、重量、温度等多种单位转换";
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
