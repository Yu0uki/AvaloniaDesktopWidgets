using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using AvaloniaApplication2.Core;
using AvaloniaApplication2.Services;
using AvaloniaApplication2.DependencyInjection;

namespace AvaloniaApplication2.ViewModels
{
    public partial class PluginWrapperViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string pluginName;

        [ObservableProperty]
        private Control? pluginContent;

        [ObservableProperty]
        private bool hasSettingsView;

        private readonly string pluginId;
        private readonly IPlugin? plugin;
        private readonly MainWindowViewModel mainWindowVM;
        private bool _disposed;

        public PluginWrapperViewModel(string pluginId, string pluginName, Control pluginContent, IPlugin? plugin, MainWindowViewModel mainWindowVM)
        {
            this.pluginId = pluginId;
            this.PluginName = pluginName;
            this.PluginContent = pluginContent;
            this.plugin = plugin;
            this.mainWindowVM = mainWindowVM;
            this.HasSettingsView = plugin?.GetSettingsView() != null;
        }

        [RelayCommand]
        private async Task OpenAsWindowAsync()
        {
            await mainWindowVM.OpenPluginAsWindowAsync(pluginId);
        }

        [RelayCommand]
        private void OpenSettings()
        {
            if (plugin == null) return;

            try
            {
                var settingsView = plugin.GetSettingsView();
                if (settingsView != null)
                {
                    // 尝试从 SettingsService 加载已保存的插件设置并注入到视图
                    TryLoadAndInjectSettings(settingsView);
                    mainWindowVM.ShowPluginSettingsView(pluginId, plugin.Name, settingsView);
                }
            }
            catch (Exception)
            {
                // 插件设置加载失败时静默处理，避免崩溃
            }
        }

        /// <summary>
        /// 尝试从 SettingsService 加载保存的设置并注入到插件设置视图中。
        /// 通过反射匹配属性名进行注入，失败时静默跳过。
        /// </summary>
        private async void TryLoadAndInjectSettings(Control settingsView)
        {
            try
            {
                var settingsService = ServiceContainer.GetService<SettingsService>();
                if (settingsService == null) return;

                var saved = await settingsService.GetPluginSettingsRawAsync(pluginId);
                if (string.IsNullOrEmpty(saved)) return;

                var vm = settingsView.DataContext;
                if (vm == null) return;

                var vmType = vm.GetType();
                using var doc = System.Text.Json.JsonDocument.Parse(saved);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    var targetProp = vmType.GetProperty(prop.Name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (targetProp == null || !targetProp.CanWrite) continue;

                    try
                    {
                        var val = System.Text.Json.JsonSerializer.Deserialize(
                            prop.Value.GetRawText(), targetProp.PropertyType);
                        if (val != null)
                            targetProp.SetValue(vm, val);
                    }
                    catch
                    {
                        // 类型不匹配，跳过
                    }
                }
            }
            catch
            {
                // 加载失败静默处理
            }
        }

        /// <summary>
        /// 保存插件设置到 SettingsService
        /// </summary>
        public async Task SavePluginSettingsAsync(object settingsViewModel)
        {
            if (settingsViewModel == null || plugin == null) return;

            try
            {
                var settingsService = ServiceContainer.GetService<SettingsService>();
                if (settingsService == null) return;

                var json = System.Text.Json.JsonSerializer.Serialize(settingsViewModel,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                await settingsService.SavePluginSettingsRawAsync(pluginId, json);
            }
            catch
            {
                // 保存失败静默处理
            }
        }

        [RelayCommand]
        private void ClosePlugin()
        {
            Dispose();
            mainWindowVM.ClosePluginView(pluginId);
        }

        /// <summary>
        /// P0: 断开 Visual Tree 引用，防止内存泄漏
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // 先触发设置保存
            if (HasSettingsView && plugin != null)
            {
                try
                {
                    var settingsView = plugin.GetSettingsView();
                    if (settingsView?.DataContext != null)
                    {
                        _ = SavePluginSettingsAsync(settingsView.DataContext);
                    }
                }
                catch { }
            }

            PluginContent = null;
        }
    }
}
