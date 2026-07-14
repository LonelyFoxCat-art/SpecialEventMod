using UnityEngine;

namespace VersatileSpecialEventAPI;

// 添加注释后
/// <summary>
/// Mod 日志记录器，提供线程安全的文件日志写入功能。
/// 支持多级别日志（Debug/Info/Warn/Error）和参数化消息格式化。
/// </summary>
/// <remarks>
/// <para>线程安全：通过 <see cref="_lockObj"/> 确保多线程环境下的并发写入安全。</para>
/// <para>异常处理：格式化失败和 IO 异常均会降级处理，不会中断主程序流程。</para>
/// </remarks>
public class ModLogger {
    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogType {
        /// <summary>调试信息</summary>
        Debug,
        /// <summary>一般信息</summary>
        Info,
        /// <summary>警告信息</summary>
        Warn,
        /// <summary>错误信息</summary>
        Error
    }

    /// <summary>
    /// 日志记录器名称，用于标识日志来源
    /// </summary>
    public string Name { get; set; } = "VersatileSpecialEventAPI";

    /// <summary>
    /// 日志文件完整路径，包含文件名
    /// </summary>
    public string LogPath { get; set; } = "VersatileSpecialEventAPI.log";

    /// <summary>
    /// 线程同步锁对象，确保日志写入的原子性
    /// </summary>
    private readonly object _lockObj = new object();

    /// <summary>
    /// 初始化日志记录器实例
    /// </summary>
    /// <param name="Name">日志记录器名称，将用于日志条目标识</param>
    /// <param name="LogPath">日志文件根目录路径，实际日志将写入 {LogPath}/Logs/{Name}.log</param>
    public ModLogger(string Name, string LogPath) {
        this.Name = Name;
        this.LogPath = Path.Combine(LogPath, "Logs", $"{Name}.log");
        if (File.Exists(this.LogPath)) File.Delete(this.LogPath);
    }

    /// <summary>
    /// 记录指定级别的日志信息
    /// </summary>
    /// <param name="type">日志级别</param>
    /// <param name="message">日志消息模板，支持 {0}, {1} 等占位符</param>
    /// <param name="parameters">用于填充消息模板的参数数组</param>
    /// <remarks>
    /// <para>格式化容错：当参数与模板不匹配时，会附加错误标记而非抛出异常。</para>
    /// <para>自动创建目录：如果日志文件所在目录不存在，会自动创建。</para>
    /// <para>IO 异常处理：文件写入失败时会输出到调试控制台，不会中断程序。</para>
    /// </remarks>
    public void Log(LogType type, string message, params object[] parameters) {
        string formattedMessage = message;

        if (parameters != null && parameters.Length > 0) {
            try {
                formattedMessage = string.Format(message, parameters);
            } catch (FormatException) {
                formattedMessage = $"{message} [Log Format Error: 参数不匹配]";
            } catch (Exception) {
                formattedMessage = $"{message} [Log Format Error: 未知格式化异常]";
            }
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string logEntry = $"[{timestamp} | {Name} | {type}]: {formattedMessage}";

        lock (_lockObj) {
            try {
                if (!string.IsNullOrEmpty(LogPath)) {
                    string directory = Path.GetDirectoryName(LogPath)!;

                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
                        Directory.CreateDirectory(directory);
                    }

                    File.AppendAllText(LogPath, logEntry + Environment.NewLine);
                }
            } catch (Exception ioEx) {
                Debug.LogError($"[CRITICAL] ModLogger 写入失败: {ioEx.Message}");
            }
        }
    }

    /// <summary>
    /// 记录 Info 级别日志的便捷方法
    /// </summary>
    /// <param name="message">日志消息模板</param>
    /// <param name="parameters">格式化参数</param>
    public void LogInfo(string message, params object[] parameters) => Log(LogType.Info, message, parameters);

    /// <summary>
    /// 记录 Debug 级别日志的便捷方法
    /// </summary>
    /// <param name="message">日志消息模板</param>
    /// <param name="parameters">格式化参数</param>
    public void LogDebug(string message, params object[] parameters) => Log(LogType.Debug, message, parameters);

    /// <summary>
    /// 记录 Warn 级别日志的便捷方法
    /// </summary>
    /// <param name="message">日志消息模板</param>
    /// <param name="parameters">格式化参数</param>
    public void LogWarn(string message, params object[] parameters) => Log(LogType.Warn, message, parameters);

    /// <summary>
    /// 记录 Error 级别日志的便捷方法
    /// </summary>
    /// <param name="message">日志消息模板</param>
    /// <param name="parameters">格式化参数</param>
    public void LogError(string message, params object[] parameters) => Log(LogType.Error, message, parameters);
}