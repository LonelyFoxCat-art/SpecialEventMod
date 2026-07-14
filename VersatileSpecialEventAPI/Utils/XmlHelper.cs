using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static VersatileSpecialEventAPI.API;

namespace VersatileSpecialEventAPI.Utils;

/// <summary>
/// 提供 XML 序列化、反序列化、文件读写及多文件合并功能的静态工具类。
/// <br/>
/// 内部通过缓存 <see cref="XmlSerializer"/> 和反射元数据来优化性能并避免内存泄漏。
/// </summary>
public static class XmlHelper {
    // OPTIMIZED: 使用 ConcurrentDictionary 替代 Dictionary，确保多线程环境下的缓存读写安全
    private static readonly ConcurrentDictionary<Type, XmlSerializer> _serializerCache = new();
    private static readonly ConcurrentDictionary<Type, MemberInfo[]> _listMembersCache = new();

    /// <summary>
    /// 获取指定类型的 <see cref="XmlSerializer"/> 实例（带线程安全缓存）。
    /// </summary>
    private static XmlSerializer GetSerializer<T>() {
        return _serializerCache.GetOrAdd(typeof(T), type => new XmlSerializer(type));
    }

    #region 反射与合并辅助逻辑

    /// <summary>
    /// 获取类型中所有可写的 <see cref="IList"/> 属性或字段。
    /// <br/>
    /// 主要用于 <see cref="Merge{T}"/> 方法中，决定哪些数据集合需要被合并。
    /// </summary>
    private static MemberInfo[] GetListMembers<T>() {
        return _listMembersCache.GetOrAdd(typeof(T), type => {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.CanWrite && typeof(IList).IsAssignableFrom(p.PropertyType))
                            .Cast<MemberInfo>();

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                             .Where(f => !f.IsInitOnly && typeof(IList).IsAssignableFrom(f.FieldType))
                             .Cast<MemberInfo>();

            return props.Concat(fields).ToArray();
        });
    }

    /// <summary>
    /// 通过反射安全地获取对象中指定成员（属性或字段）的 <see cref="IList"/> 值。
    /// </summary>
    private static IList? GetListValue(MemberInfo member, object obj) => member switch {
        PropertyInfo prop => prop.GetValue(obj) as IList,
        FieldInfo field => field.GetValue(obj) as IList,
        _ => null
    };

    /// <summary>
    /// 通过反射安全地设置对象中指定成员（属性或字段）的 <see cref="IList"/> 值。
    /// </summary>
    private static void SetListValue(MemberInfo member, object obj, IList value) {
        switch (member) {
            case PropertyInfo prop when prop.CanWrite:
                prop.SetValue(obj, value);
                break;
            case FieldInfo field when !field.IsInitOnly:
                field.SetValue(obj, value);
                break;
        }
    }

    #endregion

    #region 基础读写

    /// <summary>
    /// 从指定的 XML 文件加载并反序列化对象。
    /// <br/>
    /// 若文件不存在或解析失败，将记录错误日志并返回默认实例，不会抛出异常。
    /// </summary>
    public static T Load<T>(string filePath) where T : new() {
        if (!File.Exists(filePath)) return new T();
        try {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (T)GetSerializer<T>().Deserialize(fs)! ?? new T();
        } catch (Exception ex) {
            APILogger.LogError($"[XmlHelper] 加载失败 {filePath}: {ex.Message}");
            return new T();
        }
    }

    /// <summary>
    /// 将对象序列化并保存到指定的 XML 文件。
    /// </summary>
    public static void Save<T>(string filePath) {
        T? obj = default;
        try {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var settings = new XmlWriterSettings {
                Indent = true,
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
            };
            using var writer = XmlWriter.Create(filePath, settings);

            var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            GetSerializer<T>().Serialize(writer, obj, ns);
        } catch (Exception ex) {
            APILogger.LogError($"[XmlHelper] 保存失败 {filePath}: {ex.Message}");
        }
    }

    #endregion

    #region 批量合并

    /// <summary>
    /// 批量加载并合并多个 XML 文件。
    /// </summary>
    public static T? Merge<T>(string basePath, string? folderName = null, Action<T>? processAction = null) where T : class {
        var targetPath = Path.Combine(basePath, folderName ?? "");
        var filesToProcess = CollectXmlFiles(targetPath);
        if (filesToProcess == null || filesToProcess.Count == 0) return null;

        T? mergedResult = null;
        var listMembers = GetListMembers<T>();

        foreach (var file in filesToProcess) {
            try {
                using var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (GetSerializer<T>().Deserialize(fs) is not T currentData) continue;

                try { processAction?.Invoke(currentData); } catch (Exception ex) {
                    APILogger.LogError($"[XmlHelper] 文件 {file.Name} 的 processAction 执行失败: {ex.Message}\n{ex.StackTrace}");
                }

                if (mergedResult == null) {
                    mergedResult = currentData;
                } else {
                    MergeListMembers(mergedResult, currentData, listMembers);
                }

                APILogger.LogInfo($"[XmlHelper] 成功加载并合并: {file.Name}");
            } catch (Exception ex) {
                APILogger.LogError($"[XmlHelper] {file.Name} 加载失败: {ex.Message}\n{ex.StackTrace}");
            }
        }
        return mergedResult;
    }

    /// <summary>
    /// 收集目标路径下的 XML 文件。
    /// </summary>
    private static List<FileInfo>? CollectXmlFiles(string targetPath) {
        string Extension = Path.GetExtension(targetPath);
        if (File.Exists(targetPath)) {
            if (string.Equals(Path.GetExtension(targetPath), ".xml", StringComparison.OrdinalIgnoreCase)) {
                return new List<FileInfo> { new FileInfo(targetPath) };
            }
            APILogger.LogWarn($"[XmlHelper] 路径指向非 XML 文件，已跳过: {targetPath}");
            return null;
        }
        if (Extension == null) return null;
        if (Directory.Exists(targetPath)) {
            var files = new DirectoryInfo(targetPath).GetFiles("*.xml", SearchOption.AllDirectories);
            return files.Length > 0 ? new List<FileInfo>(files) : null;
        }

        Directory.CreateDirectory(targetPath);
        return null;
    }

    /// <summary>
    /// 执行核心的列表成员合并逻辑。
    /// </summary>
    private static void MergeListMembers<T>(T mergedResult, T currentData, MemberInfo[] listMembers) where T : class {
        foreach (var member in listMembers) {
            var sourceList = GetListValue(member, currentData);
            if (sourceList == null || sourceList.Count == 0) continue;

            var targetList = GetListValue(member, mergedResult);
            if (targetList == null) {
                var listType = member switch {
                    PropertyInfo prop => prop.PropertyType,
                    FieldInfo field => field.FieldType,
                    _ => null
                };

                if (listType != null && Activator.CreateInstance(listType) is IList newList) {
                    SetListValue(member, mergedResult, newList);
                    targetList = newList;
                }
            }

            if (targetList != null) {
                foreach (var item in sourceList) targetList.Add(item);
            }
        }
    }

    #endregion

    #region 字符串

    /// <summary>
    /// 将对象序列化为格式化的 XML 字符串。
    /// </summary>
    public static string ToXml<T>(T obj) {
        if (obj == null) return string.Empty;
        using var sw = new StringWriter();
        var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        GetSerializer<T>().Serialize(sw, obj, ns);
        return sw.ToString();
    }

    /// <summary>
    /// 从 XML 字符串反序列化对象。若字符串为空或解析失败，返回默认实例。
    /// </summary>
    public static T FromXml<T>(string xml) where T : new() {
        if (string.IsNullOrWhiteSpace(xml)) return new T();
        using var sr = new StringReader(xml);
        return (T)GetSerializer<T>().Deserialize(sr)! ?? new T();
    }

    #endregion
}
