# 单元测试指南

## 概述

本项目使用 xUnit 作为测试框架，Moq 作为 Mock 框架。

## 运行测试

```bash
# 运行所有测试
dotnet test

# 运行特定测试类
dotnet test --filter "FullyQualifiedName~SettingsServiceTests"

# 运行特定测试方法
dotnet test --filter "FullyQualifiedName~SettingsServiceTests.SettingsService_DefaultSettings_ShouldHaveCorrectValues"

# 查看详细输出
dotnet test --verbosity normal
```

## 测试结构

```
AvaloniaApplication2.Tests/
├── Services/
│   ├── SettingsServiceTests.cs      # 设置服务测试
│   └── PluginManagerTests.cs        # 插件管理器测试
├── ViewModels/                       # ViewModel 测试（待添加）
└── Infrastructure/                   # 基础设施测试（待添加）
```

## 编写测试的最佳实践

### 1. 命名规范

测试方法命名格式：`MethodName_Scenario_ExpectedBehavior`

示例：
- `LoadPluginAsync_InvalidPath_ThrowsFileNotFoundException`
- `SaveSettingsAsync_ValidSettings_ShouldSaveToFile`

### 2. AAA 模式

每个测试应遵循 Arrange-Act-Assert 模式：

```csharp
[Fact]
public void Example_Test()
{
    // Arrange - 准备测试数据
    var service = new SettingsService();
    
    // Act - 执行被测试的操作
    var result = service.Settings;
    
    // Assert - 验证结果
    Assert.NotNull(result);
}
```

### 3. 使用 Mock

对于依赖的服务，使用 Moq 进行模拟：

```csharp
var mockService = new Mock<IService>();
mockService.Setup(s => s.GetData()).Returns(testData);
```

### 4. 清理资源

在 Dispose 方法中清理测试产生的临时文件：

```csharp
public void Dispose()
{
    if (File.Exists(_testFilePath))
    {
        File.Delete(_testFilePath);
    }
}
```

## 覆盖率目标

- **核心服务**: 80%+ 覆盖率
- **ViewModel**: 70%+ 覆盖率
- **UI 组件**: 50%+ 覆盖率（通过集成测试）

## 持续集成

测试将在以下情况自动运行：
- 每次提交到 dev-* 分支
- Pull Request 创建时
- 发布前

## 常见问题

### Q: 测试失败但代码正常工作？
A: 检查是否缺少必要的初始化或 Mock 设置。

### Q: 如何测试异步方法？
A: 使用 `async Task` 返回类型和 `await` 关键字。

### Q: 如何测试私有方法？
A: 不要直接测试私有方法，通过公共接口测试其行为。
