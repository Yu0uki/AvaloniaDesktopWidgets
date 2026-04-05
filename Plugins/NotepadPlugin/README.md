# 记事本插件 (NotepadPlugin)

## 功能特性

- ✅ **笔记管理**：创建、保存、删除和重命名笔记
- ✅ **指定文件夹存储**：所有笔记保存在指定的文件夹中（默认为 `Notes` 文件夹）
- ✅ **字体调节**：支持多种字体选择（微软雅黑、宋体、黑体等）
- ✅ **字号调节**：支持8-72pt的字号选择
- ✅ **实时预览**：字体和字号更改即时生效
- ✅ **设置界面**：可配置默认字体、字号和自动保存选项

## 使用方法

### 1. 加载插件

将 `NotepadPlugin.dll` 放置在项目的 `Plugins` 目录下，启动主程序后会自动加载。

### 2. 主界面功能

#### 工具栏
- **新建**：创建一个新的笔记文件
- **保存**：保存当前编辑的笔记
- **删除**：删除选中的笔记
- **重命名**：重命名当前笔记（待完善）
- **字体选择**：从下拉列表中选择字体
- **字号选择**：从下拉列表中选择字号

#### 左侧面板
- 显示所有笔记文件列表
- 点击文件名即可加载该笔记

#### 右侧编辑器
- 文本编辑区域
- 支持多行文本和Tab键
- 自动换行

#### 状态栏
- 显示当前操作状态
- 显示当前使用的字体和字号

### 3. 设置界面

在插件管理器中点击"设置"按钮可打开设置界面：

#### 笔记存储位置
- 设置笔记文件的保存文件夹
- 默认路径：应用程序目录下的 `Notes` 文件夹

#### 默认字体设置
- 设置新建笔记时使用的默认字体
- 设置新建笔记时使用的默认字号

#### 自动保存
- 启用/禁用自动保存功能
- 设置自动保存间隔（1-60分钟）

## 文件结构

```
NotepadPlugin/
├── ViewModels/
│   ├── MainViewModel.cs        # 主视图模型
│   └── SettingsViewModel.cs    # 设置视图模型
├── Views/
│   ├── MainView.axaml          # 主界面XAML
│   ├── MainView.axaml.cs       # 主界面代码
│   ├── SettingsView.axaml      # 设置界面XAML
│   └── SettingsView.axaml.cs   # 设置界面代码
├── NotepadPlugin.cs            # 插件主类
├── NotepadPlugin.csproj        # 项目文件
└── README.md                   # 说明文档
```

## 技术实现

### MVVM架构
- 使用 `CommunityToolkit.Mvvm` 实现MVVM模式
- `ObservableProperty` 实现属性通知
- `RelayCommand` 实现命令绑定

### 数据存储
- 笔记文件：纯文本格式（.txt）
- 设置文件：JSON格式，存储在 `%APPDATA%\NotepadPlugin\settings.json`

### 字体支持
支持的字体包括：
- Microsoft YaHei（微软雅黑）
- SimSun（宋体）
- SimHei（黑体）
- KaiTi（楷体）
- FangSong（仿宋）
- Arial
- Times New Roman
- Consolas
- Courier New

## 构建和部署

### 构建
```bash
cd Plugins/NotepadPlugin
dotnet build -c Release
```

### 部署
Release构建后，DLL会自动复制到 `Plugins` 目录。如果没有自动复制，可以手动复制：
```bash
copy bin\Release\net10.0\NotepadPlugin.dll ..\..
```

## 注意事项

1. **Visual Tree规则**：每次调用 `GetMainView()` 和 `GetSettingsView()` 都会返回新实例，避免视觉树冲突
2. **文件权限**：确保笔记文件夹有读写权限
3. **编码格式**：笔记文件使用UTF-8编码
4. **自动保存**：目前自动保存功能框架已搭建，完整实现需要在主界面添加定时器

## 未来改进

- [ ] 实现完整的重命名功能（带对话框）
- [ ] 实现文件夹选择对话框
- [ ] 添加搜索功能
- [ ] 添加笔记分类/标签功能
- [ ] 实现自动保存定时器
- [ ] 支持富文本格式
- [ ] 添加笔记预览功能
- [ ] 支持导出为PDF/HTML

## 依赖项

- Avalonia 11.3.11
- CommunityToolkit.Mvvm 8.2.1
- .NET 10.0

## 作者

AI Assistant

## 许可证

本项目遵循主项目的许可证条款。
