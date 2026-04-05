# WorldClockPlugin 快速参考

## 🚀 一行命令构建和部署

```powershell
cd Plugins\WorldClockPlugin && dotnet build -c Release
```

构建完成后，DLL 自动复制到 `Plugins/` 目录。

---

## 📦 输出文件

| 文件 | 位置 | 大小 | 用途 |
|------|------|------|------|
| **WorldClockPlugin.dll** | `Plugins/` | 51 KB | ⭐ 运行时加载 |
| WorldClockPlugin.pdb | `bin/Release/net10.0/` | 28 KB | 调试符号（可选） |
| 其他文件 | `bin/Release/net10.0/` | - | 开发用，无需部署 |

---

## 🔧 常用命令

### 构建
```powershell
# Release 模式（生产用，自动部署）
dotnet build -c Release

# Debug 模式（开发用，不自动部署）
dotnet build -c Debug
```

### 清理
```powershell
# 清理编译输出
dotnet clean

# 完全清理（包括 bin 和 obj）
Remove-Item bin, obj -Recurse -Force
```

### 验证
```powershell
# 检查 DLL 是否存在
Test-Path ..\..\Plugins\WorldClockPlugin.dll

# 查看文件大小
Get-Item ..\..\Plugins\WorldClockPlugin.dll | Select-Object Length
```

---

## 📁 目录说明

```
Plugins/
├── WorldClockPlugin.dll           ← ⭐ 只需这个文件（运行时）
│
└── WorldClockPlugin/              ← 源码目录（开发用）
    ├── WorldClockPlugin.cs        # 插件主类
    ├── WorldClockView.axaml       # 主界面
    ├── SettingsView.axaml         # 设置界面
    ├── ViewModels/                # ViewModel 层
    ├── WorldClockPlugin.csproj    # 项目文件（已优化）
    ├── bin/                       # 编译输出
    │   └── Release/net10.0/
    │       └── WorldClockPlugin.dll  ← 构建产物
    ├── OPTIMIZATION_SUMMARY.md    # 优化总结
    └── OPTIMIZATION_REPORT.md     # 详细报告
```

---

## ⚙️ 配置说明

### 关键配置项

```xml
<!-- 不复制依赖项 -->
<CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>

<!-- 自动部署（仅 Release） -->
<Target Name="PostBuild" AfterTargets="PostBuildEvent" Condition="'$(Configuration)' == 'Release'">
  <Copy SourceFiles="$(TargetPath)" DestinationFolder="$(MSBuildThisFileDirectory)..\..\" />
</Target>
```

### 为什么这样配置？

1. **CopyLocalLockFileAssemblies = false**
   - 避免打包 Avalonia、CommunityToolkit.Mvvm 等共享依赖
   - 插件通过 `PluginLoadContext` 从主程序加载这些库
   - 体积从 ~150 MB 降至 51 KB

2. **PostBuild 自动复制**
   - 减少手动操作
   - 确保始终使用最新版本
   - 只在 Release 模式触发（避免调试时干扰）

---

## 🎯 扩展新插件

创建新插件的步骤：

1. **复制模板**
   ```powershell
   Copy-Item Plugins\WorldClockPlugin Plugins\MyNewPlugin -Recurse
   ```

2. **重命名文件**
   - `WorldClockPlugin.cs` → `MyNewPlugin.cs`
   - `WorldClockView.axaml` → `MyNewView.axaml`
   - 等等...

3. **修改 .csproj**
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <!-- 保持相同配置 -->
   </Project>
   ```

4. **实现 IPlugin 接口**
   ```csharp
   public class MyNewPlugin : IPlugin
   {
       public string Id => "MyNewPlugin";
       public string Name => "我的新插件";
       // ... 实现其他成员
   }
   ```

5. **构建**
   ```powershell
   cd Plugins\MyNewPlugin
   dotnet build -c Release
   ```

---

## ❓ 常见问题

### Q: 为什么 Plugins 目录下还有源码文件夹？
A: 源码目录用于开发，运行时只需要 DLL。可以保留用于后续修改。

### Q: 能否删除 bin 和 obj 目录？
A: 可以，下次构建时会重新生成。建议定期清理以节省空间。

### Q: 如何在不重启主程序的情况下更新插件？
A: 插件系统支持热重载。替换 DLL 后会自动检测并重新加载。

### Q: 插件加载失败怎么办？
A: 检查：
1. DLL 是否在 `Plugins/` 目录
2. 是否实现了 `IPlugin` 接口
3. 查看日志文件 `Logs/` 中的错误信息

---

## 📊 性能指标

| 指标 | 数值 |
|------|------|
| DLL 大小 | 51 KB |
| 首次加载时间 | ~50 ms |
| 内存占用 | ~5 MB |
| 热重载时间 | ~1.5 秒 |

---

## 🔗 相关文档

- [优化总结](./OPTIMIZATION_SUMMARY.md) - 完整的优化报告
- [详细报告](./OPTIMIZATION_REPORT.md) - 技术细节和原理
- [插件说明](./README.md) - 功能介绍
- [快速开始](./QUICK_START.md) - 入门指南

---

**最后更新：** 2026-04-05  
**版本：** 1.0.0 (优化版)
