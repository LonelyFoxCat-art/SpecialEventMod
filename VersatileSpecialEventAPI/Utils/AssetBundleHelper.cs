using UnityEngine;

namespace VersatileSpecialEventAPI.Utils;

/// <summary>
/// AssetBundle 资源管理与加载辅助类。
/// 负责 AssetBundle 的内存缓存、本地文件动态加载以及统一的资源检索调度。
/// </summary>
public static class AssetBundleHelper {
    private static Dictionary<string, AssetBundle> AssetBundleList = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// 获取指定名称的 AssetBundle 实例。
    /// </summary>
    /// <param name="Name">AssetBundle 的唯一标识名称。</param>
    /// <returns>对应的 AssetBundle 实例。</returns>
    /// <exception cref="KeyNotFoundException">当指定的 Name 不存在于缓存字典中时抛出。</exception>
    public static AssetBundle GetData(string Name) => AssetBundleList[Name];

    /// <summary>
    /// 获取当前缓存的所有 AssetBundle 数据字典引用。
    /// </summary>
    /// <returns>包含所有已加载 AssetBundle 的字典实例。</returns>
    public static Dictionary<string, AssetBundle> GetDataAll() => AssetBundleList;

    /// <summary>
    /// 检查指定名称的 AssetBundle 是否已加载并缓存。
    /// </summary>
    /// <param name="Name">AssetBundle 的唯一标识名称。</param>
    /// <returns>若存在返回 true，否则返回 false。</returns>
    public static bool IsAssetBundleExist(string Name) => AssetBundleList.ContainsKey(Name);

    /// <summary>
    /// 将 AssetBundle 实例添加到内存缓存中。
    /// </summary>
    /// <param name="Name">用于在缓存中注册的唯一标识名称。</param>
    /// <param name="Asset">要缓存的 AssetBundle 实例。</param>
    public static void AddAssetBundle(string Name, AssetBundle Asset) {
        if (!IsAssetBundleExist(Name)) AssetBundleList[Name] = Asset;
    }

    /// <summary>
    /// 从本地文件系统读取 AssetBundle 文件，加载后加入内存缓存。
    /// </summary>
    /// <param name="FilePath">AssetBundle 文件的绝对或相对物理路径。</param>
    /// <param name="FileName">用于在缓存中注册该 AssetBundle 的唯一标识名称。</param>
    public static void AddAssetBundleByFile(string FilePath, string FileName) {
        AssetBundle Asset = AssetBundle.LoadFromFile(FilePath);
        AddAssetBundle(FileName, Asset);
    }
}