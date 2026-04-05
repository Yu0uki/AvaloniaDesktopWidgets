# 插件开发速查表

## 🚀 5分钟创建插件

### 1. 创建项目
```bash
cd Plugins
dotnet new classlib -n MyPlugin
cd MyPlugin
```

### 2. 编辑 .csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
    <CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.3.11" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\AvaloniaApplication2\AvaloniaApplication2.csproj" />
  </ItemGroup>
</Project>
```

### 3. 实现 IPlugin
```csharp
using AvaloniaApplication2.Core;
using Avalonia.Controls;

namespace MyPlugin
{
    public class MyPlugin : IPlugin
    {
        public string Id => "MyPlugin";
        public string Name => "我的插件";
        public string Version => "1.0.0";
        public string Description => "描述";
        public string Author => "作者";

        public void Initialize() { }
        public void Activate() { }
        public void Deactivate() { }
        public void Shutdown() { }

        public Control GetMainView() => new MainView();
        public Control? GetSettingsView() => null; // 或 new SettingsView()
        public Stream? GetIcon() => null;
    }
}
```

### 4. 创建视图
```xml
<!-- MainView.axaml -->
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyPlugin.MainView">
  <TextBlock Text="Hello Plugin!" 
             HorizontalAlignment="Center" 
             VerticalAlignment="Center"/>
</UserControl>
```

```csharp
// MainView.axaml.cs
using Avalonia.Controls;
namespace MyPlugin {
    public partial class MainView : UserControl {
        public MainView() => InitializeComponent();
    }
}
```

### 5. 构建部署
```bash
dotnet build -c Release
# DLL 自动复制到 Plugins 目录
```

---

## 📋 核心接口速查

### IPlugin 成员

| 成员 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `Id` | property | ✅ | 唯一标识，如 "WorldClock" |
| `Name` | property | ✅ | 显示名称，支持中文 |
| `Version` | property | ✅ | 版本号，如 "1.0.0" |
| `Description` | property | ✅ | 功能描述 |
| `Author` | property | ✅ | 作者信息 |
| `Initialize()` | method | ✅ | 加载时调用一次 |
| `Activate()` | method | ✅ | 切换到视图时调用 |
| `Deactivate()` | method | ✅ | 切换出视图时调用 |
| `Shutdown()` | method | ✅ | 卸载时调用一次 |
| `GetMainView()` | method | ✅ | 返回主视图（每次新实例） |
| `GetSettingsView()` | method | ⚠️ | 返回设置视图或 null |
| `GetIcon()` | method | ⚠️ | 返回图标流或 null |

---

## 🔄 生命周期

```
Load → Initialize() → [等待] → Activate() → [运行中] 
    → Deactivate() → [暂停] → Activate() → ... 
    → Shutdown() → Unload
```

**关键规则：**
- `Initialize()` 和 `Shutdown()` 各调用一次
- `Activate()` 和 `Deactivate()` 可多次调用
- `GetMainView()` 每次都返回新实例

---

## 🎨 ViewModel 模板

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyPlugin.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string message = "Hello";

        [ObservableProperty]
        private bool isLoading = false;

        [RelayCommand]
        private async Task DoWorkAsync()
        {
            IsLoading = true;
            try
            {
                await Task.Delay(1000);
                Message = "Done!";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
```

---

## ⚙️ 常用配置

### 项目文件关键属性
```xml
<!-- 必须与主程序一致 -->
<TargetFramework>net10.0</TargetFramework>

<!-- 减小体积的关键 -->
<CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>

<!-- 启用编译绑定 -->
<AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
```

### 依赖版本（必须对齐）
```xml
<PackageReference Include="Avalonia" Version="11.3.11" />
<PackageReference Include="Avalonia.Desktop" Version="11.3.11" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.1" />
```

---

## 🔧 常见问题快速解决

### ❌ 插件未加载
```bash
# 检查
1. DLL 在 Plugins 目录？
2. 实现了 IPlugin？
3. 类是 public 且非抽象？
4. 查看 Logs/app.log
```

### ❌ Visual Tree 冲突
```csharp
// ❌ 错误
private Control _view;
public Control GetMainView() => _view ??= new MainView();

// ✅ 正确
public Control GetMainView() => new MainView();
```

### ❌ 依赖缺失
```xml
<!-- 确保设置了 -->
<CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>
```

### ❌ 版本不匹配
```bash
# 检查主程序的 .csproj
grep "Version=" AvaloniaApplication2/AvaloniaApplication2.csproj

# 确保插件使用相同版本
```

---

## 📁 标准目录结构

```
MyPlugin/
├── ViewModels/
│   ├── MainViewModel.cs
│   └── SettingsViewModel.cs (可选)
├── Views/
│   ├── MainView.axaml
│   ├── MainView.axaml.cs
│   ├── SettingsView.axaml (可选)
│   └── SettingsView.axaml.cs (可选)
├── MyPlugin.cs          # IPlugin 实现
└── MyPlugin.csproj      # 项目文件
```

---

## 💡 最佳实践清单

### ✅ 必须做
- [ ] 实现所有 IPlugin 方法
- [ ] `GetMainView()` 返回新实例
- [ ] 设置 `CopyLocalLockFileAssemblies=false`
- [ ] 依赖版本与主程序一致
- [ ] 在 `Shutdown()` 中释放资源
- [ ] 处理异常，避免崩溃

### ⚠️ 建议做
- [ ] 使用 MVVM 模式
- [ ] ViewModel 实现 `IDisposable`
- [ ] 添加日志记录
- [ ] 提供设置界面（如需要）
- [ ] 编写 README.md
- [ ] 单元测试 ViewModel

### ❌ 避免做
- [ ] 缓存视图实例
- [ ] 直接访问其他插件
- [ ] 假设特定执行顺序
- [ ] 阻塞 UI 线程
- [ ] 忽略资源清理
- [ ] 硬编码路径

---

## 🔍 调试技巧

### 日志输出
```csharp
using Serilog;
var logger = Log.ForContext<MyPlugin>();
logger.Information("初始化开始");
logger.Error(ex, "发生错误");
```

### 断点调试
```csharp
public void Initialize()
{
    System.Diagnostics.Debugger.Break(); // 触发断点
}
```

### 附加调试器
```
Visual Studio → 调试 → 附加到进程 → dotnet.exe
```

---

## 📦 部署检查清单

发布前确认：
- [ ] Release 构建成功
- [ ] DLL 在 Plugins 目录
- [ ] 无额外依赖文件
- [ ] 测试加载正常
- [ ] 测试视图切换
- [ ] 测试设置保存（如有）
- [ ] 测试资源释放

---

## 🔗 有用链接

- 📖 [完整开发指南](./PLUGIN_DEVELOPMENT_GUIDE.md)
- 💻 [示例插件：WorldClockPlugin](../Plugins/WorldClockPlugin/)
- 🌐 [Avalonia 文档](https://docs.avaloniaui.net/)
- 🛠️ [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)

---

**提示：** 将此文件放在 Plugins 目录，方便随时查阅！
