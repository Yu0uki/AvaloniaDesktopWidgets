using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NotepadPlugin.ViewModels
{
    /// <summary>
    /// 记事本主视图模型
    /// </summary>
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly string _notesFolder;
        private bool _isDisposed = false;

        // ===== 可观察属性 =====

        [ObservableProperty]
        private ObservableCollection<string> _noteFiles = new();

        [ObservableProperty]
        private string _selectedNoteFile = string.Empty;

        [ObservableProperty]
        private string _noteContent = string.Empty;

        [ObservableProperty]
        private string _currentNoteName = string.Empty;

        [ObservableProperty]
        private double _fontSize = 14.0;

        [ObservableProperty]
        private string _fontFamily = "Microsoft YaHei";

        [ObservableProperty]
        private bool _isSaving = false;

        [ObservableProperty]
        private string _statusMessage = "就绪";

        // ===== 字体选项 =====

        public ObservableCollection<string> AvailableFonts { get; } = new()
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

        public ObservableCollection<double> AvailableFontSizes { get; } = new()
        {
            8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72
        };

        // ===== 构造函数 =====

        public MainViewModel()
        {
            // 默认笔记文件夹路径
            _notesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes");
            
            // 确保文件夹存在
            if (!Directory.Exists(_notesFolder))
            {
                Directory.CreateDirectory(_notesFolder);
            }

            // 加载笔记列表
            LoadNoteFiles();
            
            StatusMessage = $"已加载 {NoteFiles.Count} 个笔记";
        }

        // ===== 命令 =====

        /// <summary>
        /// 新建笔记
        /// </summary>
        [RelayCommand]
        public void NewNote()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var newFileName = $"Note_{timestamp}.txt";
                var filePath = Path.Combine(_notesFolder, newFileName);

                // 创建空文件
                File.WriteAllText(filePath, string.Empty);

                // 刷新列表
                LoadNoteFiles();

                // 选中新创建的笔记
                SelectedNoteFile = newFileName;
                CurrentNoteName = Path.GetFileNameWithoutExtension(newFileName);
                NoteContent = string.Empty;

                StatusMessage = $"已创建新笔记: {CurrentNoteName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"创建笔记失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 保存当前笔记
        /// </summary>
        [RelayCommand]
        public async Task SaveNoteAsync()
        {
            if (string.IsNullOrEmpty(SelectedNoteFile))
            {
                StatusMessage = "请先选择一个笔记";
                return;
            }

            try
            {
                IsSaving = true;
                StatusMessage = "保存中...";

                var filePath = Path.Combine(_notesFolder, SelectedNoteFile);
                await File.WriteAllTextAsync(filePath, NoteContent);

                StatusMessage = $"已保存: {CurrentNoteName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// 删除当前笔记
        /// </summary>
        [RelayCommand]
        public void DeleteNote()
        {
            if (string.IsNullOrEmpty(SelectedNoteFile))
            {
                StatusMessage = "请先选择一个笔记";
                return;
            }

            try
            {
                var filePath = Path.Combine(_notesFolder, SelectedNoteFile);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    LoadNoteFiles();
                    
                    NoteContent = string.Empty;
                    CurrentNoteName = string.Empty;
                    SelectedNoteFile = string.Empty;
                    
                    StatusMessage = "笔记已删除";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"删除失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 重命名当前笔记
        /// </summary>
        [RelayCommand]
        public void RenameNote()
        {
            if (string.IsNullOrEmpty(SelectedNoteFile))
            {
                StatusMessage = "请先选择一个笔记";
                return;
            }

            // 这里可以弹出一个对话框让用户输入新名称
            // 为简化，暂时不做实现
            StatusMessage = "重命名功能待实现";
        }

        // ===== 方法 =====

        /// <summary>
        /// 加载笔记文件列表
        /// </summary>
        private void LoadNoteFiles()
        {
            try
            {
                var files = Directory.GetFiles(_notesFolder, "*.txt")
                    .Select(Path.GetFileName)
                    .Where(f => f != null)
                    .OrderByDescending(f => f)
                    .ToList();

                NoteFiles.Clear();
                foreach (var file in files)
                {
                    NoteFiles.Add(file!);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载笔记列表失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 当选择笔记文件改变时调用
        /// </summary>
        partial void OnSelectedNoteFileChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    var filePath = Path.Combine(_notesFolder, value);
                    if (File.Exists(filePath))
                    {
                        NoteContent = File.ReadAllText(filePath);
                        CurrentNoteName = Path.GetFileNameWithoutExtension(value);
                        StatusMessage = $"已加载: {CurrentNoteName}";
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"加载笔记内容失败: {ex.Message}";
                }
            }
            else
            {
                NoteContent = string.Empty;
                CurrentNoteName = string.Empty;
            }
        }

        /// <summary>
        /// 当字体大小改变时自动保存设置
        /// </summary>
        partial void OnFontSizeChanged(double value)
        {
            // 可以在这里添加自动保存设置的逻辑
            StatusMessage = $"字体大小: {value}";
        }

        /// <summary>
        /// 当字体家族改变时自动保存设置
        /// </summary>
        partial void OnFontFamilyChanged(string value)
        {
            // 可以在这里添加自动保存设置的逻辑
            StatusMessage = $"字体: {value}";
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
