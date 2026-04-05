using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace PluginTemplate.ViewModels
{
    /// <summary>
    /// 主视图 ViewModel 模板
    /// </summary>
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        // ===== Observable 属性 =====
        
        [ObservableProperty]
        private string message = "欢迎使用插件模板！";

        [ObservableProperty]
        private int counter = 0;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private string errorMessage = "";

        // ===== 命令 =====

        /// <summary>
        /// 示例命令：执行异步操作
        /// </summary>
        [RelayCommand]
        private async Task DoWorkAsync()
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = "";

            try
            {
                // TODO: 替换为实际的业务逻辑
                await Task.Delay(1000); // 模拟异步操作
                
                Counter++;
                Message = $"操作完成！计数：{Counter}";
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"操作失败：{ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 示例命令：重置状态
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanReset))]
        private void Reset()
        {
            Counter = 0;
            Message = "已重置";
            HasError = false;
            ErrorMessage = "";
        }

        private bool CanReset() => Counter > 0;

        /// <summary>
        /// 示例命令：带参数的命令
        /// </summary>
        [RelayCommand]
        private void ShowMessage(string text)
        {
            Message = $"收到消息：{text}";
        }

        // ===== 资源清理 =====

        public void Dispose()
        {
            // TODO: 清理资源（定时器、订阅等）
        }
    }
}
