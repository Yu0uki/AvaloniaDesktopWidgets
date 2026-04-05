namespace AvaloniaApplication2.Models
{
    /// <summary>
    /// 插件清单模型 - 从插件DLL中读取的元数据
    /// </summary>
    public class PluginManifest
    {
        /// <summary>
        /// 插件唯一标识符
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 插件显示名称
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 插件版本号
        /// </summary>
        public string Version { get; set; } = "";

        /// <summary>
        /// 插件描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 插件作者
        /// </summary>
        public string Author { get; set; } = "";

        /// <summary>
        /// 实现IPlugin接口的完整类型名
        /// </summary>
        public string MainType { get; set; } = "";
    }
}
