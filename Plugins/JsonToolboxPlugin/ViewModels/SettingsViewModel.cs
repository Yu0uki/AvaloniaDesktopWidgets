using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Text.Json;

namespace JsonToolboxPlugin.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly string _settingsFile;

        // ===== 可观察属性 =====

        [ObservableProperty]
        private int _indentSize = 2;

        [ObservableProperty]
        private double _editorFontSize = 13.0;

        [ObservableProperty]
        private bool _wordWrap = false;

        [ObservableProperty]
        private bool _autoValidate = false;

        [ObservableProperty]
        private string _statusMessage = "就绪";

        // 缩进选项
        public int[] IndentSizes { get; } = { 2, 4, 6, 8 };

        // 字号选项
        public double[] FontSizes { get; } = { 10, 11, 12, 13, 14, 15, 16, 18, 20, 22, 24 };

        // ===== 构造函数 =====

        public SettingsViewModel()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JsonToolboxPlugin"
            );
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            _settingsFile = Path.Combine(appDataPath, "settings.json");
            LoadSettings();
        }

        // ===== 命令 =====

        [RelayCommand]
        private void SaveSettings()
        {
            try
            {
                var data = new SettingsData
                {
                    IndentSize = IndentSize,
                    EditorFontSize = EditorFontSize,
                    WordWrap = WordWrap,
                    AutoValidate = AutoValidate
                };

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_settingsFile, json);
                StatusMessage = "✅ 设置已保存";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ 保存失败: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ResetSettings()
        {
            IndentSize = 2;
            EditorFontSize = 13.0;
            WordWrap = false;
            AutoValidate = false;
            StatusMessage = "已重置为默认值（未保存）";
        }

        // ===== 方法 =====

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFile))
                {
                    var json = File.ReadAllText(_settingsFile);
                    var data = JsonSerializer.Deserialize<SettingsData>(json);
                    if (data != null)
                    {
                        IndentSize = data.IndentSize;
                        EditorFontSize = data.EditorFontSize;
                        WordWrap = data.WordWrap;
                        AutoValidate = data.AutoValidate;
                        StatusMessage = "设置已加载";
                        return;
                    }
                }

                ResetSettings();
                StatusMessage = "使用默认设置";
            }
            catch
            {
                ResetSettings();
                StatusMessage = "加载设置失败，使用默认值";
            }
        }

        // ===== 内部类 =====

        private class SettingsData
        {
            public int IndentSize { get; set; } = 2;
            public double EditorFontSize { get; set; } = 13.0;
            public bool WordWrap { get; set; } = false;
            public bool AutoValidate { get; set; } = false;
        }
    }
}
