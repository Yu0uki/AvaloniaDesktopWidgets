using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PluginTemplate.ViewModels
{
    /// <summary>
    /// 设置视图 ViewModel 模板
    /// </summary>
    public partial class SettingsViewModel : ObservableObject
    {
        // ===== 设置属性 =====
        
        [ObservableProperty]
        private string setting1 = "默认值";

        [ObservableProperty]
        private bool enableFeature = true;

        [ObservableProperty]
        private int maxItems = 100;

        [ObservableProperty]
        private string selectedTheme = "Light";

        // ===== 下拉选项 =====
        
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

        /// <summary>
        /// 从配置文件加载设置
        /// </summary>
        private void LoadSettings()
        {
            // TODO: 实现设置加载逻辑
            // 示例：
            // if (File.Exists("settings.json"))
            // {
            //     var json = File.ReadAllText("settings.json");
            //     var settings = JsonSerializer.Deserialize<MySettings>(json);
            //     if (settings != null)
            //     {
            //         Setting1 = settings.Setting1;
            //         EnableFeature = settings.EnableFeature;
            //         // ...
            //     }
            // }
        }

        /// <summary>
        /// 保存设置到配置文件
        /// </summary>
        private void SaveSettings()
        {
            // TODO: 实现设置保存逻辑
            // 示例：
            // var settings = new MySettings
            // {
            //     Setting1 = this.Setting1,
            //     EnableFeature = this.EnableFeature,
            //     MaxItems = this.MaxItems,
            //     SelectedTheme = this.SelectedTheme
            // };
            // var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            // File.WriteAllText("settings.json", json);
        }

        /// <summary>
        /// 保存设置命令
        /// </summary>
        [RelayCommand]
        private void SaveSettingsCommand()
        {
            SaveSettings();
            // TODO: 显示保存成功的通知
        }

        /// <summary>
        /// 重置为默认值
        /// </summary>
        [RelayCommand]
        private void ResetToDefaults()
        {
            Setting1 = "默认值";
            EnableFeature = true;
            MaxItems = 100;
            SelectedTheme = "Light";
        }
    }
}
