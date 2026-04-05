using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WorldClockPlugin.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string timeFormat = "yyyy-MM-dd HH:mm:ss";

        [ObservableProperty]
        private bool isIndependentWindow = false;

        [ObservableProperty]
        private bool showSeconds = true;

        [ObservableProperty]
        private bool use24HourFormat = true;

        public SettingsViewModel()
        {
            // 加载保存的设置（如果有）
            LoadSettings();
        }

        private void LoadSettings()
        {
            // 这里可以从配置文件或主应用设置中加载
            // 暂时使用默认值
        }

        public void SaveSettings()
        {
            // 这里可以保存设置到配置文件
            // 暂时只保留在内存中
        }

        [RelayCommand]
        private void SaveSettingsCommand()
        {
            SaveSettings();
        }
    }
}
