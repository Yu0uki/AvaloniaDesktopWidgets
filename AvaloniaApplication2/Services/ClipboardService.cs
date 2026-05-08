using AvaloniaApplication2.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace AvaloniaApplication2.Services
{
    /// <summary>
    /// 剪贴板历史服务 — 运行期间持久化，不受页面导航影响
    /// </summary>
    public class ClipboardService
    {
        public ObservableCollection<ClipboardItem> History { get; } = new();

        private const int MaxItems = 50;

        public void AddItem(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;
            if (History.Count > 0 && History[0].Content == content) return; // 去重

            History.Insert(0, new ClipboardItem { Content = content, Time = System.DateTime.Now });

            while (History.Count > MaxItems)
                History.RemoveAt(History.Count - 1);
        }

        public void Clear()
        {
            History.Clear();
        }
    }
}
