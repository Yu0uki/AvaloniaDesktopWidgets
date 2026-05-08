using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;

namespace NotepadPlugin.ViewModels
{
    /// <summary>
    /// 记事本设置视图模型
    /// </summary>
    public partial class SettingsViewModel : ObservableObject, IDisposable
    {
        private readonly string _settingsFile;
        private bool _isDisposed = false;

        // ===== 可观察属性 =====

        [ObservableProperty]
        private string _notesFolderPath = string.Empty;

        [ObservableProperty]
        private double _defaultFontSize = 14.0;

        [ObservableProperty]
        private string _defaultFontFamily = "Microsoft YaHei";

        [ObservableProperty]
        private bool _autoSave = true;

        [ObservableProperty]
        private int _autoSaveInterval = 5;

        [ObservableProperty]
        private string _statusMessage = "就绪";

        // ===== 字体选项 =====

        public string[] AvailableFonts { get; } = new[]
        {
            "Microsoft YaHei",
            "SimSun",
            "SimHei",
            "KaiTi",
            "FangSong",
            "Arial",
            "Times New Roman",
            "Consolas",
            "Courier New"
        };

        public double[] AvailableFontSizes { get; } = new double[]
        {
            8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72
        };

        // ===== 构造函数 =====

        public SettingsViewModel()
        {
            // 设置文件路径
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NotepadPlugin"
            );

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _settingsFile = Path.Combine(appDataPath, "settings.json");

            // 加载设置
            LoadSettings();
        }

        // ===== 命令 =====

        /// <summary>
        /// 保存设置
        /// </summary>
        [RelayCommand]
        public void SaveSettings()
        {
            try
            {
                // 验证笔记文件夹路径
                if (!Directory.Exists(NotesFolderPath))
                {
                    StatusMessage = "笔记文件夹路径不存在";
                    return;
                }

                // 保存设置到文件
                var settings = new
                {
                    NotesFolderPath = NotesFolderPath,
                    DefaultFontSize = DefaultFontSize,
                    DefaultFontFamily = DefaultFontFamily,
                    AutoSave = AutoSave,
                    AutoSaveInterval = AutoSaveInterval
                };

                var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_settingsFile, json);

                StatusMessage = "设置已保存";
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存设置失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 重置为默认设置
        /// </summary>
        [RelayCommand]
        public void ResetSettings()
        {
            NotesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes");
            DefaultFontSize = 14.0;
            DefaultFontFamily = "Microsoft YaHei";
            AutoSave = true;
            AutoSaveInterval = 5;

            StatusMessage = "已重置为默认设置";
        }

        /// <summary>
        /// 选择笔记文件夹
        /// </summary>
        [RelayCommand]
        public void SelectNotesFolder()
        {
            // 在实际应用中，这里应该打开文件夹选择对话框
            // 由于Avalonia的对话框需要在UI线程中调用，这里简化处理
            StatusMessage = "请使用文件管理器选择文件夹，然后手动输入路径";
        }

        // ===== 方法 =====

        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFile))
                {
                    var json = File.ReadAllText(_settingsFile);
                    var settings = System.Text.Json.JsonSerializer.Deserialize<SettingsData>(json);

                    if (settings != null)
                    {
                        NotesFolderPath = settings.NotesFolderPath ?? 
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes");
                        DefaultFontSize = settings.DefaultFontSize;
                        DefaultFontFamily = settings.DefaultFontFamily ?? "Microsoft YaHei";
                        AutoSave = settings.AutoSave;
                        AutoSaveInterval = settings.AutoSaveInterval;

                        StatusMessage = "设置已加载";
                    }
                }
                else
                {
                    // 使用默认值
                    ResetSettings();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载设置失败: {ex.Message}";
                ResetSettings();
            }
        }

        // ===== 内部类 =====

        private class SettingsData
        {
            public string? NotesFolderPath { get; set; }
            public double DefaultFontSize { get; set; }
            public string? DefaultFontFamily { get; set; }
            public bool AutoSave { get; set; }
            public int AutoSaveInterval { get; set; }
        }

        // ===== IDisposable =====

        public void Dispose()
        {
            if (!_isDisposed)
            {
                // 清理资源
                _isDisposed = true;
            }
        }
    }
}
