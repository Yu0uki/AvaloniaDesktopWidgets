using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JsonToolboxPlugin.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly string _settingsFile;
        private bool _isDisposed = false;

        // ===== 可观察属性 =====

        [ObservableProperty]
        private string _inputJson = string.Empty;

        [ObservableProperty]
        private string _outputText = string.Empty;

        [ObservableProperty]
        private string _statusMessage = "就绪";

        [ObservableProperty]
        private int _indentSize = 2;

        [ObservableProperty]
        private double _editorFontSize = 13.0;

        [ObservableProperty]
        private bool _wordWrap = false;

        // 输入字符统计
        [ObservableProperty]
        private int _inputCharCount = 0;

        [ObservableProperty]
        private int _inputLineCount = 0;

        // 输出字符统计
        [ObservableProperty]
        private string _outputStats = "0 字符";

        // ===== 构造函数 =====

        public MainViewModel()
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

        // ===== 核心命令 =====

        /// <summary>
        /// JSON 格式化（美化输出）
        /// </summary>
        [RelayCommand]
        public void FormatJson()
        {
            if (string.IsNullOrWhiteSpace(InputJson))
            {
                StatusMessage = "请输入 JSON 内容";
                return;
            }

            try
            {
                var options = new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                };

                using var doc = JsonDocument.Parse(InputJson, options);
                var writerOptions = new JsonWriterOptions
                {
                    Indented = true,
                    IndentSize = IndentSize
                };

                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream, writerOptions);
                doc.WriteTo(writer);
                writer.Flush();

                OutputText = Encoding.UTF8.GetString(stream.ToArray());
                UpdateOutputStats();
                StatusMessage = $"✅ 格式化完成，共 {OutputText.Length} 个字符";
            }
            catch (JsonException ex)
            {
                OutputText = string.Empty;
                StatusMessage = $"❌ JSON 格式错误: {ex.Message}";
            }
        }

        /// <summary>
        /// JSON 压缩（移除空白）
        /// </summary>
        [RelayCommand]
        public void MinifyJson()
        {
            if (string.IsNullOrWhiteSpace(InputJson))
            {
                StatusMessage = "请输入 JSON 内容";
                return;
            }

            try
            {
                var options = new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                };

                using var doc = JsonDocument.Parse(InputJson, options);
                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream);
                doc.WriteTo(writer);
                writer.Flush();

                OutputText = Encoding.UTF8.GetString(stream.ToArray());
                UpdateOutputStats();
                StatusMessage = $"✅ 压缩完成，共 {OutputText.Length} 个字符（压缩比 {GetCompressionRatio()}%）";
            }
            catch (JsonException ex)
            {
                OutputText = string.Empty;
                StatusMessage = $"❌ JSON 格式错误: {ex.Message}";
            }
        }

        /// <summary>
        /// JSON 验证
        /// </summary>
        [RelayCommand]
        public void ValidateJson()
        {
            if (string.IsNullOrWhiteSpace(InputJson))
            {
                StatusMessage = "请输入 JSON 内容";
                return;
            }

            try
            {
                var options = new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                };

                using var doc = JsonDocument.Parse(InputJson, options);
                var rootElement = doc.RootElement;

                var typeName = rootElement.ValueKind switch
                {
                    JsonValueKind.Object => "对象 (Object)",
                    JsonValueKind.Array => "数组 (Array)",
                    JsonValueKind.String => "字符串 (String)",
                    JsonValueKind.Number => "数字 (Number)",
                    JsonValueKind.True or JsonValueKind.False => "布尔值 (Boolean)",
                    JsonValueKind.Null => "空值 (Null)",
                    _ => "未知"
                };

                var depth = GetMaxDepth(rootElement);
                var propertyCount = CountProperties(rootElement);

                OutputText = $@"✅ JSON 验证通过

类型: {typeName}
最大深度: {depth}
属性总数: {propertyCount}
总字符数: {InputJson.Length}

解析耗时: < 1ms";
                UpdateOutputStats();
                StatusMessage = $"✅ JSON 有效 - {typeName}，{propertyCount} 个属性";
            }
            catch (JsonException ex)
            {
                var position = GetErrorPosition(ex);
                OutputText = $"❌ JSON 无效\n\n错误位置 (行 {position.line}, 列 {position.column}):\n{ex.Message}";
                UpdateOutputStats();
                StatusMessage = $"❌ JSON 无效: {ex.Message}";
            }
        }

        /// <summary>
        /// JSON 转义（将 JSON 字符串转义为可嵌入代码的格式）
        /// </summary>
        [RelayCommand]
        public void EscapeJson()
        {
            if (string.IsNullOrWhiteSpace(InputJson))
            {
                StatusMessage = "请输入 JSON 内容";
                return;
            }

            var escaped = InputJson
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");

            OutputText = escaped;
            UpdateOutputStats();
            StatusMessage = $"✅ 转义完成，共 {OutputText.Length} 个字符";
        }

        /// <summary>
        /// JSON 反转义
        /// </summary>
        [RelayCommand]
        public void UnescapeJson()
        {
            if (string.IsNullOrWhiteSpace(InputJson))
            {
                StatusMessage = "请输入 JSON 内容";
                return;
            }

            try
            {
                var unescaped = System.Text.RegularExpressions.Regex.Unescape(InputJson);
                OutputText = unescaped;
                UpdateOutputStats();
                StatusMessage = $"✅ 反转义完成，共 {OutputText.Length} 个字符";
            }
            catch (Exception ex)
            {
                OutputText = string.Empty;
                StatusMessage = $"❌ 反转义失败: {ex.Message}";
            }
        }

        // ===== 辅助命令 =====

        /// <summary>
        /// 复制输出到剪贴板
        /// </summary>
        [RelayCommand]
        public async Task CopyOutputAsync()
        {
            if (string.IsNullOrEmpty(OutputText))
            {
                StatusMessage = "没有可复制的内容";
                return;
            }

            try
            {
                var topLevel = Avalonia.Application.Current?.ApplicationLifetime
                    as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
                if (topLevel?.MainWindow?.Clipboard != null)
                {
                    await topLevel.MainWindow.Clipboard.SetTextAsync(OutputText);
                    StatusMessage = $"✅ 已复制到剪贴板（{OutputText.Length} 个字符）";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ 复制失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 清空输入输出
        /// </summary>
        [RelayCommand]
        public void ClearAll()
        {
            InputJson = string.Empty;
            OutputText = string.Empty;
            InputCharCount = 0;
            InputLineCount = 0;
            StatusMessage = "已清空";
        }

        /// <summary>
        /// 交换输入和输出
        /// </summary>
        [RelayCommand]
        public void SwapInputOutput()
        {
            if (string.IsNullOrEmpty(OutputText))
                return;

            var temp = InputJson;
            InputJson = OutputText;
            OutputText = string.Empty;
            StatusMessage = "已交换，可继续处理";
        }

        /// <summary>
        /// 从剪贴板粘贴到输入
        /// </summary>
        [RelayCommand]
        public async Task PasteFromClipboardAsync()
        {
            try
            {
                var topLevel = Avalonia.Application.Current?.ApplicationLifetime
                    as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
                if (topLevel?.MainWindow?.Clipboard != null)
                {
                    var text = await topLevel.MainWindow.Clipboard.GetTextAsync();
                    if (!string.IsNullOrEmpty(text))
                    {
                        InputJson = text;
                        UpdateInputStats();
                        StatusMessage = "已从剪贴板粘贴";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ 粘贴失败: {ex.Message}";
            }
        }

        // ===== 属性变化处理 =====

        partial void OnInputJsonChanged(string value)
        {
            UpdateInputStats();
        }

        partial void OnOutputTextChanged(string value)
        {
            OutputStats = string.IsNullOrEmpty(value) ? "0 字符" : $"{value.Length} 字符";
        }

        // ===== 统计方法 =====

        private void UpdateInputStats()
        {
            InputCharCount = InputJson.Length;
            InputLineCount = string.IsNullOrEmpty(InputJson) ? 0 : InputJson.Split('\n').Length;
        }

        private void UpdateOutputStats()
        {
            // 统计信息通过 StatusMessage 展示
        }

        private int GetMaxDepth(JsonElement element, int depth = 0)
        {
            int maxDepth = depth;

            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        maxDepth = Math.Max(maxDepth, GetMaxDepth(property.Value, depth + 1));
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        maxDepth = Math.Max(maxDepth, GetMaxDepth(item, depth + 1));
                    }
                }
            }

            return maxDepth;
        }

        private int CountProperties(JsonElement element)
        {
            int count = 0;

            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    count++;
                    if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        count += CountProperties(property.Value);
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        count += CountProperties(item);
                    }
                }
            }

            return count;
        }

        private (int line, int column) GetErrorPosition(JsonException ex)
        {
            if (ex is null) return (0, 0);

            // 从异常消息中尝试提取行号/列号
            var match = Regex.Match(ex.Message, @"\((\d+),\s*(\d+)\)");
            if (match.Success && match.Groups.Count >= 3)
            {
                return (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
            }

            return (0, 0);
        }

        private int GetCompressionRatio()
        {
            if (string.IsNullOrEmpty(InputJson) || string.IsNullOrEmpty(OutputText))
                return 100;

            var inputLen = InputJson.Length;
            var outputLen = OutputText.Length;
            if (inputLen == 0) return 100;

            return (int)((double)outputLen / inputLen * 100);
        }

        // ===== 设置持久化 =====

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
                    }
                }
            }
            catch
            {
                // 使用默认值
            }
        }

        public void SaveSettings()
        {
            try
            {
                var data = new SettingsData
                {
                    IndentSize = IndentSize,
                    EditorFontSize = EditorFontSize,
                    WordWrap = WordWrap
                };
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFile, json);
            }
            catch
            {
                // 静默失败
            }
        }

        private class SettingsData
        {
            public int IndentSize { get; set; } = 2;
            public double EditorFontSize { get; set; } = 13.0;
            public bool WordWrap { get; set; } = false;
        }

        // ===== IDisposable =====

        public void Dispose()
        {
            if (!_isDisposed)
            {
                SaveSettings();
                _isDisposed = true;
            }
        }
    }
}
