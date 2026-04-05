# 插件模板使用说明

## 📦 如何使用此模板

### 方法 1：直接复制（推荐）

1. **复制整个 Template 文件夹**
   ```bash
   # 在 Plugins 目录下
   cp -r Template MyNewPlugin
   cd MyNewPlugin
   ```

2. **重命名文件和类**
   - `PluginTemplate.csproj` → `MyNewPlugin.csproj`
   - `PluginTemplate.cs` → `MyNewPlugin.cs`
   - 修改 `.csproj` 中的项目名称
   - 修改所有 `.cs` 文件中的命名空间：`PluginTemplate` → `MyNewPlugin`

3. **修改插件元数据**
   编辑 `MyNewPlugin.cs`：
   ```csharp
   public string Id => "MyNewPlugin";
   public string Name => "我的新插件";
   public string Version => "1.0.0";
   public string Description => "插件描述";
   public string Author => "作者名";
   ```

4. **实现业务逻辑**
   - 编辑 `ViewModels/MainViewModel.cs` 添加业务逻辑
   - 编辑 `Views/MainView.axaml` 设计界面
   - 如需要设置，编辑 `SettingsViewModel.cs` 和 `SettingsView.axaml`

5. **构建并测试**
   ```bash
   dotnet build -c Release
   # DLL 会自动复制到 Plugins 目录
   ```

### 方法 2：从头创建

参考 [`PLUGIN_QUICK_REFERENCE.md`](../PLUGIN_QUICK_REFERENCE.md) 中的"5分钟创建插件"章节。

---

## 📁 模板结构说明

```
Template/
├── ViewModels/
│   ├── MainViewModel.cs        # 主视图 ViewModel（含完整示例）
│   └── SettingsViewModel.cs    # 设置视图 ViewModel（含完整示例）
├── Views/
│   ├── MainView.axaml          # 主视图 XAML（含布局示例）
│   ├── MainView.axaml.cs       # 主视图 Code-behind
│   ├── SettingsView.axaml      # 设置视图 XAML（含表单示例）
│   └── SettingsView.axaml.cs   # 设置视图 Code-behind
├── PluginTemplate.cs           # 插件主类（IPlugin 实现）
├── PluginTemplate.csproj       # 项目文件（已配置好依赖）
└── README.md                   # 本文件
```

---

## ✨ 模板特性

### 已包含的功能

✅ **完整的生命周期管理**
- Initialize / Activate / Deactivate / Shutdown

✅ **MVVM 架构**
- Observable 属性
- RelayCommand 命令
- 异步操作支持

✅ **UI 组件示例**
- 文本显示
- 按钮和命令
- 加载状态
- 错误提示
- 表单控件（TextBox, CheckBox, ComboBox, NumericUpDown）

✅ **设置界面**
- 多种设置类型
- 保存/重置功能
- 设置持久化框架

✅ **最佳实践**
- 资源清理（IDisposable）
- 错误处理
- 用户反馈
- 响应式设计

---

## 🔧 自定义指南

### 1. 简化为最小插件

如果不需要设置界面：

```csharp
// PluginTemplate.cs
public Control? GetSettingsView()
{
    return null; // 返回 null
}
```

删除以下文件：
- `ViewModels/SettingsViewModel.cs`
- `Views/SettingsView.axaml`
- `Views/SettingsView.axaml.cs`

### 2. 添加新功能

在 `MainViewModel.cs` 中添加：

```csharp
[ObservableProperty]
private string newData = "";

[RelayCommand]
private void NewFeature()
{
    // 实现新功能
}
```

在 `MainView.axaml` 中添加 UI：

```xml
<Button Content="新功能" 
        Command="{Binding NewFeatureCommand}"/>
```

### 3. 添加数据模型

创建 `Models/` 目录：

```csharp
// Models/MyData.cs
namespace PluginTemplate.Models
{
    public class MyData
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
```

### 4. 添加服务层

创建 `Services/` 目录：

```csharp
// Services/DataService.cs
namespace PluginTemplate.Services
{
    public class DataService
    {
        public async Task<List<MyData>> GetDataAsync()
        {
            // 实现数据获取逻辑
        }
    }
}
```

---

## 📝 开发检查清单

开始开发前：
- [ ] 已重命名所有文件和类
- [ ] 已修改命名空间
- [ ] 已更新插件元数据
- [ ] 已确认依赖版本与主程序一致

开发过程中：
- [ ] 遵循 MVVM 模式
- [ ] 正确处理异步操作
- [ ] 添加适当的错误处理
- [ ] 实现资源清理

发布前：
- [ ] Release 构建成功
- [ ] 测试所有功能
- [ ] 测试视图切换
- [ ] 测试设置保存（如有）
- [ ] 检查内存泄漏
- [ ] 编写插件文档

---

## 🐛 常见问题

### Q: 如何调试模板？

A: 在代码中添加断点，然后附加调试器到运行的主程序。

### Q: 可以删除不需要的文件吗？

A: 可以。如果不需要设置界面，可以删除相关文件并让 `GetSettingsView()` 返回 null。

### Q: 如何添加第三方库？

A: 在 `.csproj` 中添加 `<PackageReference>`，确保版本兼容。

### Q: 模板中的示例代码必须保留吗？

A: 不必。这些只是示例，您可以根据需要修改或删除。

---

## 📚 相关文档

- 📖 [完整开发指南](../../doc/PLUGIN_DEVELOPMENT_GUIDE.md)
- 📋 [快速参考](../PLUGIN_QUICK_REFERENCE.md)
- 💻 [示例插件：WorldClockPlugin](../WorldClockPlugin/)

---

**祝您开发顺利！** 🚀
