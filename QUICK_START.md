# 快速开始指南

## 🚀 5分钟上手

### 1. 构建项目
```bash
cd f:\Programs\project\AvaloniaApplication2
dotnet build
```

### 2. 运行应用
```bash
dotnet run --project AvaloniaApplication2
```

### 3. 查看日志
应用运行时，日志会输出到：
- **控制台**：实时查看
- **文件**：`AvaloniaApplication2/bin/Debug/net10.0/Logs/app-YYYYMMDD.log`

### 4. 运行测试
```bash
cd AvaloniaApplication2.Tests
dotnet test
```

---

## 🔌 插件热重载演示

### 步骤1：创建测试插件
创建一个简单的类库项目实现 `IPlugin` 接口。

### 步骤2：加载插件
将编译好的 DLL 拖拽到应用的插件管理界面。

### 步骤3：修改插件代码
修改插件代码并重新编译。

### 步骤4：观察自动重载
将新的 DLL 复制到插件目录（覆盖原文件），应用会自动检测并重新加载插件。

查看日志确认：
```
[HH:MM:SS INF] 检测到插件文件变化: MyPlugin
[HH:MM:SS INF] 开始热重载插件: MyPlugin
[HH:MM:SS INF] 插件热重载成功: MyPlugin v1.0.1
```

---

## 📊 新功能使用

### 依赖注入
所有服务已通过 DI 容器管理：
```csharp
// 获取服务
var pluginManager = ServiceContainer.GetRequiredService<PluginManager>();
var settings = ServiceContainer.GetRequiredService<SettingsService>();
```

### 日志记录
在任何类中使用：
```csharp
using AvaloniaApplication2.Infrastructure;
using Serilog;

private readonly ILogger _logger = LoggingConfig.Logger.ForContext<MyClass>();

_logger.Information("操作成功: {Param}", value);
_logger.Error(ex, "操作失败: {Context}", context);
```

### 异常处理
全局异常处理器已自动注册，无需额外配置。

---

## 🧪 编写测试

### 示例：测试服务
```csharp
[Fact]
public async Task MyService_ShouldWork()
{
    // Arrange
    var service = new MyService();
    
    // Act
    var result = await service.DoSomething();
    
    // Assert
    Assert.NotNull(result);
}
```

### 运行特定测试
```bash
# 运行特定类
dotnet test --filter "FullyQualifiedName~SettingsServiceTests"

# 运行特定方法
dotnet test --filter "FullyQualifiedName~SaveSettingsAsync_ValidSettings_ShouldSaveToFile"
```

---

## 📖 更多信息

- **详细实施总结**：查看 [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)
- **测试指南**：查看 [AvaloniaApplication2.Tests/README.md](AvaloniaApplication2.Tests/README.md)
- **项目说明**：查看 [README.md](README.md)

---

## ❓ 常见问题

### Q: 热重载不工作？
A: 确保：
1. 插件已成功加载
2. 文件确实被修改（时间戳变化）
3. 查看日志是否有错误信息

### Q: 测试失败？
A: 运行 `dotnet test --verbosity normal` 查看详细错误信息。

### Q: 日志文件在哪？
A: `AvaloniaApplication2/bin/Debug/net10.0/Logs/` 目录下。

---

祝开发愉快！🎉
