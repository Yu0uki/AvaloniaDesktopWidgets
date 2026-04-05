# Avalonia 插件系统开发指南

## 📋 目录

- [概述](#概述)
- [插件架构](#插件架构)
- [快速开始](#快速开始)
- [核心接口](#核心接口)
- [项目结构](#项目结构)
- [视图开发](#视图开发)
- [设置界面](#设置界面)
- [ViewModel 模式](#viewmodel-模式)
- [构建与部署](#构建与部署)
- [最佳实践](#最佳实践)
- [常见问题](#常见问题)

---

## 概述

本项目的插件系统基于 **Avalonia UI** 框架，采用 **AssemblyLoadContext** 实现插件隔离加载。插件以独立的 DLL 文件形式存在，无需额外的配置文件，主程序会自动扫描并加载 `Plugins` 目录下的所有 DLL 文件。

### 核心特性

✅ **零配置加载** - 只需将 DLL 放入 Plugins 目录  
✅ **隔离加载** - 每个插件在独立的 AssemblyLoadContext 中运行  
✅ **热重载支持** - 开发时可自动重新加载修改后的插件  
✅ **完整生命周期管理** - Initialize → Activate → Deactivate → Shutdown  
✅ **自定义视图** - 支持主视图和设置视图  
✅ **依赖共享** - 插件可复用主程序的依赖库，减小体积  

---

## 插件架构

```
┌─────────────────────────────────────────┐
│         AvaloniaApplication2            │
│          (主应用程序)                    │
│                                         │
│  ┌───────────────────────────────────┐  │
│  │      PluginManager                │  │
│  │  - 扫描 Plugins 目录              │  │
│  │  - 加载/卸载插件                  │  │
│  │  - 管理插件生命周期               │  │
│  └──────────────┬────────────────────┘  │
│                 │                        │
│  ┌──────────────▼────────────────────┐  │
│  │   PluginLoadContext (隔离上下文)   │  │
│  │  - 独立的 AssemblyLoadContext     │  │
│  │  - 依赖回退到主程序               │  │
│  └──────────────┬────────────────────┘  │
│                 │                        │
└─────────────────┼────────────────────────┘
                  │
    ┌─────────────┴─────────────┐
    │                           │
┌───▼────┐              ┌──────▼──────┐
│Plugin A│              │ Plugin B    │
│.dll    │              │ .dll        │
└────────┘              └─────────────┘
```

### 关键组件

| 组件 | 位置 | 职责 |
|------|------|------|
| `IPlugin` | `Core/IPlugin.cs` | 定义插件契约接口 |
| `PluginManager` | `Services/PluginManager.cs` | 管理插件的加载、卸载和生命周期 |
| `PluginLoadContext` | `Services/PluginLoadContext.cs` | 提供隔离的加载环境 |
| `PluginHotReloadManager` | `Services/PluginHotReloadManager.cs` | 支持开发时热重载 |
| `PluginInfo` | `Core/PluginInfo.cs` | 插件元数据模型 |

---

## 快速开始

### 1. 创建插件项目

```bash
# 在 Plugins 目录下创建新的类库项目
cd Plugins
dotnet new classlib -n MyPlugin
cd MyPlugin
```

### 2. 修改项目文件

编辑 `MyPlugin.csproj`：

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
    <!-- ⭐ 关键：不复制依赖项，由主程序提供 -->
    <CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>
  </PropertyGroup>

  <ItemGroup>
    <!-- Avalonia UI 依赖（版本需与主程序一致） -->
    <PackageReference Include="Avalonia" Version="11.3.11" />
    <PackageReference Include="Avalonia.Desktop" Version="11.3.11" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.11" />
    
    <!-- MVVM 工具包 -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.1" />
  </ItemGroup>

  <ItemGroup>
    <!-- ⭐ 引用主项目以访问 IPlugin 接口 -->
    <ProjectReference Include="..\..\AvaloniaApplication2\AvaloniaApplication2.csproj" />
  </ItemGroup>

  <!-- 可选：Release 构建后自动复制到 Plugins 目录 -->
  <Target Name="PostBuild" AfterTargets="PostBuildEvent" Condition="'$(Configuration)' == 'Release'">
    <Copy SourceFiles="$(TargetPath)" 
          DestinationFolder="$(MSBuildThisFileDirectory)..\..\" 
          SkipUnchangedFiles="true" />
  </Target>

</Project>
```

### 3. 实现 IPlugin 接口

创建 `MyPlugin.cs`：

```csharp
using AvaloniaApplication2.Core;
using Avalonia.Controls;
using System.IO;

namespace MyPlugin
{
    public class MyPlugin : IPlugin
    {
        // 插件元数据
        public string Id => "MyPlugin";
        public string Name => "我的插件";
        public string Version => "1.0.0";
        public string Description => "这是一个示例插件";
        public string Author => "Your Name";

        // 生命周期方法
        public void Initialize()
        {
            // 插件加载时调用一次
            // 初始化资源、加载配置等
        }

        public void Activate()
        {
            // 切换到插件视图时调用
            // 启动定时器、订阅事件等
        }

        public void Deactivate()
        {
            // 切换出插件视图时调用
            // 暂停定时器、取消订阅等
        }

        public void Shutdown()
        {
            // 卸载插件时调用
            // 释放资源、保存状态等
        }

        // 视图方法
        public Control GetMainView()
        {
            // ⭐ 重要：每次都创建新实例，避免 Visual Tree 冲突
            return new MainView();
        }

        public Control? GetSettingsView()
        {
            // 可选：返回设置视图，不需要则返回 null
            return new SettingsView();
        }

        public Stream? GetIcon()
        {
            // 可选：返回插件图标
            return null;
        }
    }
}
```

### 4. 构建并测试

```bash
# 构建 Release 版本（会自动复制到 Plugins 目录）
dotnet build -c Release

# 或者手动复制
copy bin\Release\net10.0\MyPlugin.dll ..\..\

# 启动主程序验证
cd ..\..
dotnet run
```

---

## 核心接口

### IPlugin 接口详解

```csharp
public interface IPlugin
{
    // ===== 元数据属性 =====
    
    /// <summary>
    /// 插件唯一标识符（建议使用 PascalCase，如 "WorldClock"）
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 插件显示名称（支持中文，如 "世界时钟"）
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 插件版本号（语义化版本，如 "1.0.0"）
    /// </summary>
    string Version { get; }

    /// <summary>
    /// 插件功能描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 插件作者信息
    /// </summary>
    string Author { get; }

    // ===== 生命周期方法 =====
    
    /// <summary>
    /// 初始化插件（加载时调用一次）
    /// 用途：加载配置、初始化资源、建立连接等
    /// </summary>
    void Initialize();

    /// <summary>
    /// 激活插件（切换到插件视图时调用）
    /// 用途：启动定时器、开始数据更新、订阅事件等
    /// </summary>
    void Activate();

    /// <summary>
    /// 停用插件（切换出插件视图时调用）
    /// 用途：暂停定时器、停止数据更新、取消订阅等
    /// </summary>
    void Deactivate();

    /// <summary>
    /// 关闭插件（卸载时调用）
    /// 用途：释放资源、保存状态、断开连接等
    /// </summary>
    void Shutdown();

    // ===== 视图方法 =====
    
    /// <summary>
    /// 获取插件主界面
    /// ⚠️ 注意：每次调用都应返回新实例
    /// </summary>
    Control GetMainView();

    /// <summary>
    /// 获取插件设置界面（可选）
    /// 返回 null 表示不支持设置
    /// ⚠️ 注意：每次调用都应返回新实例
    /// </summary>
    Control? GetSettingsView();

    /// <summary>
    /// 获取插件图标（可选）
    /// 返回 null 表示使用默认图标
    /// </summary>
    Stream? GetIcon();
}
```

### 生命周期流程图

```
加载插件
   ↓
Initialize()  ← 只调用一次
   ↓
[插件已加载，等待激活]
   ↓
用户切换到插件视图
   ↓
Activate()    ← 可多次调用
   ↓
[插件正在运行]
   ↓
用户切换到其他视图
   ↓
Deactivate()  ← 可多次调用
   ↓
[插件已暂停]
   ↓
用户卸载插件
   ↓
Shutdown()    ← 只调用一次
   ↓
[插件已卸载]
```

---

## 项目结构

### 推荐的插件目录结构

```
MyPlugin/
├── ViewModels/           # ViewModel 层
│   ├── MainViewModel.cs
│   └── SettingsViewModel.cs
├── Views/                # 视图层（XAML + Code-behind）
│   ├── MainView.axaml
│   ├── MainView.axaml.cs
│   ├── SettingsView.axaml
│   └── SettingsView.axaml.cs
├── Models/               # 数据模型（可选）
│   └── MyDataModel.cs
├── Services/             # 服务层（可选）
│   └── MyService.cs
├── Resources/            # 资源文件（可选）
│   └── Icons/
├── MyPlugin.cs           # 插件主类（实现 IPlugin）
├── MyPlugin.csproj       # 项目文件
└── README.md             # 插件说明文档（可选）
```

### 最小化结构（简单插件）

```
MyPlugin/
├── MainView.axaml
├── MainView.axaml.cs
├── MyPlugin.cs
└── MyPlugin.csproj
```

---

## 视图开发

### 主视图（MainView）

主视图是插件的核心界面，展示插件的主要功能。

#### XAML 示例

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:viewModels="clr-namespace:MyPlugin.ViewModels"
             x:DataType="viewModels:MainViewModel"
             x:Class="MyPlugin.MainView">

  <UserControl.DataContext>
    <viewModels:MainViewModel />
  </UserControl.DataContext>

  <Grid Margin="20">
    <Grid.RowDefinitions>
      <RowDefinition Height="Auto"/>
      <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- 标题 -->
    <TextBlock Grid.Row="0" 
               Text="🎯 我的插件" 
               FontSize="24" 
               FontWeight="Bold"
               HorizontalAlignment="Center"
               Margin="0,0,0,20"/>

    <!-- 主内容区 -->
    <StackPanel Grid.Row="1" Spacing="15">
      <TextBlock Text="{Binding Message}" 
                 FontSize="16"
                 HorizontalAlignment="Center"/>
      
      <Button Content="点击我" 
              Command="{Binding DoSomethingCommand}"
              HorizontalAlignment="Center"
              Width="150"
              Height="40"/>
    </StackPanel>
  </Grid>
</UserControl>
```

#### Code-behind 示例

```csharp
using Avalonia.Controls;

namespace MyPlugin
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }
    }
}
```

### ⚠️ 关键注意事项

1. **每次返回新实例**
   ```csharp
   public Control GetMainView()
   {
       // ✅ 正确：每次都创建新实例
       return new MainView();
       
       // ❌ 错误：缓存实例会导致 Visual Tree 冲突
       // return _cachedView;
   }
   ```

2. **DataContext 设置**
   - 在 XAML 中通过 `<UserControl.DataContext>` 设置
   - 或在 Code-behind 的构造函数中设置

3. **样式一致性**
   - 使用 Fluent 主题
   - 遵循主应用的配色方案
   - 使用标准的间距和字体大小

---

## 设置界面

### 何时需要设置界面？

✅ 需要用户自定义参数  
✅ 需要持久化配置  
✅ 需要调整插件行为  
❌ 功能简单，无需配置  
❌ 所有参数都有合理默认值  

### 设置视图示例

#### SettingsView.axaml

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:viewModels="clr-namespace:MyPlugin.ViewModels"
             x:DataType="viewModels:SettingsViewModel"
             x:Class="MyPlugin.SettingsView">

  <UserControl.DataContext>
    <viewModels:SettingsViewModel />
  </UserControl.DataContext>

  <ScrollViewer Padding="20">
    <StackPanel Spacing="20" MaxWidth="500">
      
      <!-- 标题 -->
      <TextBlock Text="⚙️ 插件设置" 
                 FontSize="20" 
                 FontWeight="Bold"/>

      <!-- 设置组 1 -->
      <Border Background="#F5F5F5" CornerRadius="8" Padding="15">
        <StackPanel Spacing="10">
          <TextBlock Text="📋 基本设置" 
                     FontWeight="Bold" 
                     FontSize="16"/>
          
          <TextBox Text="{Binding Setting1}" 
                   Watermark="请输入..."
                   Height="35"/>
          
          <CheckBox Content="启用选项" 
                    IsChecked="{Binding EnableOption}"/>
        </StackPanel>
      </Border>

      <!-- 设置组 2 -->
      <Border Background="#E3F2FD" CornerRadius="8" Padding="15">
        <StackPanel Spacing="10">
          <TextBlock Text="🎨 外观设置" 
                     FontWeight="Bold" 
                     FontSize="16"/>
          
          <ComboBox ItemsSource="{Binding ThemeOptions}"
                    SelectedItem="{Binding SelectedTheme}"
                    Height="35"/>
        </StackPanel>
      </Border>

      <!-- 保存按钮 -->
      <Button Content="💾 保存设置" 
              HorizontalAlignment="Right"
              Width="120" 
              Height="40"
              Classes="accent"
              Command="{Binding SaveSettingsCommand}"/>

      <TextBlock Text="💡 提示: 某些设置需要重启后生效" 
                 FontSize="11" 
                 Foreground="#999999" 
                 HorizontalAlignment="Right"/>
    </StackPanel>
  </ScrollViewer>
</UserControl>
```

#### SettingsViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace MyPlugin.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        // 绑定属性
        [ObservableProperty]
        private string setting1 = "";

        [ObservableProperty]
        private bool enableOption = true;

        [ObservableProperty]
        private string selectedTheme = "Light";

        // 下拉选项
        public ObservableCollection<string> ThemeOptions { get; } = new()
        {
            "Light",
            "Dark",
            "System"
        };

        public SettingsViewModel()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            // 从配置文件或主应用设置中加载
            // 示例：
            // var settings = File.ReadAllText("settings.json");
            // 反序列化并赋值
        }

        private void SaveSettings()
        {
            // 保存设置到配置文件
            // 示例：
            // var json = JsonSerializer.Serialize(this);
            // File.WriteAllText("settings.json", json);
        }

        [RelayCommand]
        private void SaveSettingsCommand()
        {
            SaveSettings();
            // 可以显示保存成功的通知
        }
    }
}
```

### 设置持久化策略

#### 方案 1：独立 JSON 文件

```csharp
private const string SettingsFile = "myplugin_settings.json";

private void SaveSettings()
{
    var settings = new
    {
        Setting1 = this.Setting1,
        EnableOption = this.EnableOption,
        SelectedTheme = this.SelectedTheme
    };
    
    var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
    { 
        WriteIndented = true 
    });
    
    File.WriteAllText(SettingsFile, json);
}

private void LoadSettings()
{
    if (File.Exists(SettingsFile))
    {
        var json = File.ReadAllText(SettingsFile);
        var settings = JsonSerializer.Deserialize<MySettings>(json);
        
        if (settings != null)
        {
            Setting1 = settings.Setting1;
            EnableOption = settings.EnableOption;
            SelectedTheme = settings.SelectedTheme;
        }
    }
}
```

#### 方案 2：使用主应用的 SettingsService

```csharp
// 在主应用中注册插件设置
_settingsService.Settings.PluginSettings["MyPlugin"] = new Dictionary<string, object>
{
    { "Setting1", "value" },
    { "EnableOption", true }
};

// 在插件中读取
var settings = _settingsService.Settings.PluginSettings["MyPlugin"];
```

---

## ViewModel 模式

### 推荐架构

```
┌─────────────────────┐
│      View (XAML)    │  ← UI 层，纯声明式
└──────────┬──────────┘
           │ Data Binding
┌──────────▼──────────┐
│   ViewModel         │  ← 业务逻辑层
│   - ObservableProp  │     处理用户交互
│   - RelayCommand    │     管理状态
└──────────┬──────────┘
           │ 调用
┌──────────▼──────────┐
│   Model/Service     │  ← 数据层
│   - 数据模型        │     数据处理
│   - 业务服务        │     外部 API
└─────────────────────┘
```

### ViewModel 最佳实践

#### 1. 使用 CommunityToolkit.Mvvm

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Timers;

namespace MyPlugin.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        // =====  observable 属性 =====
        
        [ObservableProperty]
        private string message = "Hello, Plugin!";

        [ObservableProperty]
        private int counter = 0;

        [ObservableProperty]
        private bool isLoading = false;

        // ===== 命令 =====
        
        [RelayCommand]
        private async Task DoSomethingAsync()
        {
            IsLoading = true;
            try
            {
                // 异步操作
                await Task.Delay(1000);
                Message = "操作完成！";
                Counter++;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanReset))]
        private void Reset()
        {
            Counter = 0;
            Message = "已重置";
        }

        private bool CanReset() => Counter > 0;

        // ===== 定时器示例 =====
        
        private Timer? _timer;

        public MainViewModel()
        {
            StartTimer();
        }

        private void StartTimer()
        {
            _timer = new Timer(1000);
            _timer.Elapsed += (s, e) => UpdateData();
            _timer.Start();
        }

        private void UpdateData()
        {
            // 注意：Timer 回调不在 UI 线程
            // 需要使用 Dispatcher 或直接绑定（Avalonia 会自动处理）
        }

        // ===== 资源清理 =====
        
        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}
```

#### 2. 属性变更通知

```csharp
// 方式 1：自动生成（推荐）
[ObservableProperty]
private string name = "";

// 方式 2：部分方法（用于额外逻辑）
partial void OnNameChanged(string value)
{
    // 当 Name 改变时执行
    Console.WriteLine($"Name changed to: {value}");
}

// 方式 3：手动实现（复杂场景）
private string _description;
public string Description
{
    get => _description;
    set => SetProperty(ref _description, value);
}
```

#### 3. 命令参数

```csharp
[RelayCommand]
private void DeleteItem(int id)
{
    // 处理删除逻辑
}

// XAML 中使用
<Button Content="删除" 
        Command="{Binding DeleteItemCommand}"
        CommandParameter="123"/>
```

---

## 构建与部署

### 项目配置要点

#### 1. 目标框架

```xml
<TargetFramework>net10.0</TargetFramework>
```
必须与主程序一致。

#### 2. 禁用依赖复制

```xml
<CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>
```
这是减小插件体积的关键配置。

#### 3. 依赖版本对齐

确保以下包的版本与主程序完全一致：

```xml
<PackageReference Include="Avalonia" Version="11.3.11" />
<PackageReference Include="Avalonia.Desktop" Version="11.3.11" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.11" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.1" />
```

查看主程序的 `.csproj` 文件确认版本号。

### 自动化部署

#### 方法 1：MSBuild PostBuild（推荐）

```xml
<Target Name="PostBuild" AfterTargets="PostBuildEvent" Condition="'$(Configuration)' == 'Release'">
  <Copy SourceFiles="$(TargetPath)" 
        DestinationFolder="$(MSBuildThisFileDirectory)..\..\" 
        SkipUnchangedFiles="true" />
  <Message Text="✅ 插件已部署到 Plugins 目录" Importance="high" />
</Target>
```

#### 方法 2：手动复制

```bash
# Debug 模式
copy bin\Debug\net10.0\MyPlugin.dll ..\..\

# Release 模式
copy bin\Release\net10.0\MyPlugin.dll ..\..\
```

#### 方法 3：PowerShell 脚本

```powershell
# deploy.ps1
$pluginName = "MyPlugin"
$source = "bin\Release\net10.0\$pluginName.dll"
$destination = "..\..\$pluginName.dll"

if (Test-Path $source) {
    Copy-Item $source $destination -Force
    Write-Host "✅ $pluginName.dll 已部署" -ForegroundColor Green
} else {
    Write-Host "❌ 找不到 $source" -ForegroundColor Red
}
```

### 验证部署

```bash
# 检查 Plugins 目录
dir Plugins\*.dll

# 应该看到：
# WorldClockPlugin.dll
# MyPlugin.dll
```

---

## 最佳实践

### 1. 插件设计原则

#### ✅ 单一职责
每个插件专注于一个功能领域：
- ❌ 一个插件包含时钟、计算器、记事本
- ✅ 三个独立插件：时钟插件、计算器插件、记事本插件

#### ✅ 松耦合
- 不直接依赖其他插件
- 通过主应用的服务进行通信
- 使用事件或消息总线解耦

#### ✅ 自包含
- 插件应能独立工作
- 不假设其他插件的存在
- 优雅处理依赖缺失的情况

### 2. 性能优化

#### 延迟初始化
```csharp
private HeavyService? _service;

private HeavyService Service => 
    _service ??= new HeavyService();
```

#### 资源管理
```csharp
public void Shutdown()
{
    // 释放所有资源
    _timer?.Dispose();
    _connection?.Close();
    _cache?.Clear();
}
```

#### 避免内存泄漏
```csharp
public void Deactivate()
{
    // 取消所有订阅
    _eventAggregator.Unsubscribe(this);
    _timer?.Stop();
}

public void Activate()
{
    // 重新订阅
    _eventAggregator.Subscribe(this);
    _timer?.Start();
}
```

### 3. 错误处理

#### 防御性编程
```csharp
public void Initialize()
{
    try
    {
        LoadConfiguration();
        InitializeServices();
    }
    catch (ConfigurationException ex)
    {
        Log.Error(ex, "配置加载失败，使用默认配置");
        UseDefaultConfiguration();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "插件初始化失败");
        throw; // 让 PluginManager 处理
    }
}
```

#### 用户友好的错误提示
```csharp
[RelayCommand]
private async Task SaveDataAsync()
{
    try
    {
        await _dataService.SaveAsync();
        ShowNotification("保存成功", NotificationType.Success);
    }
    catch (IOException ex)
    {
        ShowNotification($"保存失败：{ex.Message}", NotificationType.Error);
    }
}
```

### 4. 用户体验

#### 响应式设计
```xml
<!-- 适应不同窗口大小 -->
<Grid>
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width="*" MinWidth="300"/>
    <ColumnDefinition Width="Auto"/>
  </Grid.ColumnDefinitions>
</Grid>
```

#### 加载状态
```csharp
[ObservableProperty]
private bool isLoading = false;

[ObservableProperty]
private string loadingMessage = "加载中...";

// XAML
<StackPanel IsVisible="{Binding IsLoading}">
  <ProgressBar IsIndeterminate="True"/>
  <TextBlock Text="{Binding LoadingMessage}"/>
</StackPanel>
```

#### 空状态
```xml
<StackPanel IsVisible="{Binding !HasData}">
  <TextBlock Text="📭 暂无数据" 
             FontSize="18"
             HorizontalAlignment="Center"/>
  <Button Content="添加数据" 
          Command="{Binding AddDataCommand}"/>
</StackPanel>
```

### 5. 代码规范

#### 命名约定
```csharp
// 类名：PascalCase
public class WorldClockPlugin : IPlugin { }

// 接口：IPascalCase
public interface IDataService { }

// 方法：PascalCase
public void Initialize() { }

// 属性：PascalCase
public string PluginName { get; set; }

// 私有字段：_camelCase
private Timer _timer;

// 局部变量：camelCase
var itemCount = 0;
```

#### 注释规范
```csharp
/// <summary>
/// 计算两个日期的天数差
/// </summary>
/// <param name="startDate">起始日期</param>
/// <param name="endDate">结束日期</param>
/// <returns>天数差</returns>
public int CalculateDaysDifference(DateTime startDate, DateTime endDate)
{
    // 实现细节注释
    var difference = endDate - startDate;
    return (int)difference.TotalDays;
}
```

### 6. 测试建议

#### 单元测试 ViewModel
```csharp
[Test]
public void IncrementCounter_ShouldIncreaseCounter()
{
    // Arrange
    var viewModel = new MainViewModel();
    var initialCount = viewModel.Counter;

    // Act
    viewModel.IncrementCommand.Execute(null);

    // Assert
    Assert.AreEqual(initialCount + 1, viewModel.Counter);
}
```

#### 集成测试插件加载
```csharp
[Test]
public async Task LoadPlugin_ShouldSucceed()
{
    // Arrange
    var pluginManager = new PluginManager(settingsService);
    var pluginPath = "Plugins/MyPlugin.dll";

    // Act
    var plugin = await pluginManager.LoadPluginAsync(pluginPath);

    // Assert
    Assert.IsNotNull(plugin);
    Assert.AreEqual("MyPlugin", plugin.Id);
}
```

---

## 常见问题

### Q1: 为什么我的插件没有被加载？

**检查清单：**
1. ✅ DLL 是否在 `Plugins` 目录下？
2. ✅ 是否实现了 `IPlugin` 接口？
3. ✅ 类是否是 `public` 且非抽象？
4. ✅ 是否有无参构造函数？
5. ✅ 查看日志文件 `Logs/app.log` 中的错误信息

**调试步骤：**
```csharp
// 在 PluginManager.cs 中添加日志
_logger.Information("尝试加载: {Path}", dllPath);
_logger.Information("找到的类型: {Types}", assembly.GetTypes());
```

### Q2: 为什么出现 "Visual Tree 冲突" 错误？

**原因：** 缓存了视图实例，导致同一个控件被添加到多个父容器。

**解决：**
```csharp
// ❌ 错误
private Control _cachedView;
public Control GetMainView() => _cachedView ??= new MainView();

// ✅ 正确
public Control GetMainView() => new MainView();
```

### Q3: 如何与其他插件通信？

**方法 1：通过主应用服务**
```csharp
// 在主应用中注册共享服务
services.AddSingleton<ISharedDataService, SharedDataService>();

// 在插件中注入（需要修改插件加载逻辑）
var dataService = ServiceContainer.GetService<ISharedDataService>();
```

**方法 2：事件总线**
```csharp
// 定义事件
public class PluginEvent
{
    public string Source { get; set; }
    public string EventType { get; set; }
    public object? Data { get; set; }
}

// 发布事件
_eventBus.Publish(new PluginEvent 
{ 
    Source = "PluginA", 
    EventType = "DataUpdated" 
});

// 订阅事件
_eventBus.Subscribe<PluginEvent>(e => 
{
    if (e.EventType == "DataUpdated")
    {
        // 处理
    }
});
```

### Q4: 如何调试插件？

**方法 1：附加调试器**
```bash
# 启动主程序
dotnet run

# 在 Visual Studio 中：调试 → 附加到进程 → 选择 dotnet.exe
```

**方法 2：日志输出**
```csharp
// 使用 Serilog
Log.Information("插件初始化开始");
Log.Debug("配置值: {@Config}", config);
Log.Error(ex, "发生错误");
```

**方法 3：断点调试**
```csharp
// 在插件代码中设置断点
public void Initialize()
{
    System.Diagnostics.Debugger.Break(); // 触发断点
    // ...
}
```

### Q5: 插件体积为什么这么大？

**原因：** 包含了所有依赖项。

**解决：**
```xml
<!-- 确保设置了这个属性 -->
<CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>
```

**验证：**
```bash
# 检查输出目录
dir bin\Release\net10.0\

# 应该只有：
# MyPlugin.dll (几十 KB)
# MyPlugin.pdb

# 不应该有：
# Avalonia.dll
# CommunityToolkit.Mvvm.dll
# ... 等其他依赖
```

### Q6: 如何实现热重载？

主程序已内置热重载支持：

1. 确保启用了热重载：
```csharp
// 在 App.axaml.cs 或 Program.cs 中
var hotReloadManager = new PluginHotReloadManager(pluginManager);
hotReloadManager.Enable();
```

2. 修改插件代码后重新编译：
```bash
dotnet build -c Release
```

3. 插件会自动重新加载（监视文件变化）

### Q7: 如何处理异步操作？

```csharp
[RelayCommand]
private async Task LoadDataAsync()
{
    IsLoading = true;
    try
    {
        // 异步加载数据
        var data = await _apiService.GetDataAsync();
        Items.Clear();
        foreach (var item in data)
        {
            Items.Add(item);
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "加载数据失败");
        ShowError("加载失败，请重试");
    }
    finally
    {
        IsLoading = false;
    }
}
```

### Q8: 插件可以访问主应用的哪些资源？

**可以访问：**
- ✅ 所有 NuGet 包（Avalonia, CommunityToolkit.Mvvm 等）
- ✅ `AvaloniaApplication2.Core` 中的类型
- ✅ 主应用注册的服务（通过 ServiceContainer）

**不能直接访问：**
- ❌ 主应用的内部实现细节
- ❌ 其他插件的内部类型
- ❌ 主应用的私有成员

---

## 附录

### A. 完整的插件模板

创建一个名为 `PluginTemplate` 的项目作为起点：

```
PluginTemplate/
├── ViewModels/
│   ├── MainViewModel.cs
│   └── SettingsViewModel.cs
├── Views/
│   ├── MainView.axaml
│   ├── MainView.axaml.cs
│   ├── SettingsView.axaml
│   └── SettingsView.axaml.cs
├── PluginTemplate.cs
├── PluginTemplate.csproj
└── README.md
```

**下载地址：** （待提供）

### B. 参考资源

- [Avalonia 官方文档](https://docs.avaloniaui.net/)
- [CommunityToolkit.Mvvm 文档](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [AssemblyLoadContext 文档](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.loader.assemblyloadcontext)
- [本项目示例插件：WorldClockPlugin](../Plugins/WorldClockPlugin/)

### C. 版本历史

| 版本 | 日期 | 说明 |
|------|------|------|
| 1.0.0 | 2026-04-05 | 初始版本，基于 WorldClockPlugin 实践总结 |

### D. 联系与支持

- 问题反馈：提交 Issue
- 插件分享：欢迎贡献到 Plugins 目录
- 文档改进：欢迎提交 PR

---

**祝您开发愉快！** 🎉

如有任何问题，请参考本指南或查看 WorldClockPlugin 示例代码。
