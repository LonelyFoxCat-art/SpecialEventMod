using VersatileSpecialEventAPI.Utils;
using static VersatileSpecialEventAPI.API;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML.Wrapper;

/// <summary>
/// 稀有度 (Rarity) XML 数据包装器。
/// <br/>
/// 负责管理 Mod 自定义稀有度配置的加载、缓存与查询，提供字符串/枚举与内部数据模型之间的双向映射。
/// </summary>
public class RarityXmlWrapper {
    public static Dictionary<string, RarityXmlRoot> RarityDict = new Dictionary<string, RarityXmlRoot>();

    /// <summary>
    /// 获取当前已加载的所有 Mod 稀有度配置字典。
    /// </summary>
    /// <returns>包含所有已加载稀有度根节点的字典引用。</returns>
    public static Dictionary<string, RarityXmlRoot> GetDataAll() => RarityDict;

    /// <summary>
    /// 检查指定 Mod 的稀有度配置是否已加载到内存中。
    /// </summary>
    /// <param name="packageId">Mod 的唯一包标识符。</param>
    /// <returns>若已加载返回 true，否则返回 false。</returns>
    public static bool IsRarityLoaded(string packageId) => RarityDict.ContainsKey(packageId);

    /// <summary>
    /// 根据稀有度名称获取对应的 XML 配置信息。
    /// </summary>
    /// <param name="packageId">Mod 的唯一包标识符。</param>
    /// <param name="rarity">稀有度的显示名称 (RarityName)。</param>
    /// <returns>匹配的 <see cref="RarityXmlInfo"/> 实例；若未找到或 Mod 未加载，则返回 null。</returns>
    public static RarityXmlInfo GetRarityXmlInfo(string packageId, string rarity) {
        if (RarityDict.TryGetValue(packageId, out var rarityInfo)) {
            return rarityInfo.rarityXmlList.Find(x => rarity == x.RarityName);
        }
        return null!;
    }

    /// <summary>
    /// 根据游戏内部的 <see cref="Rarity"/> 枚举获取对应的 XML 配置信息。
    /// </summary>
    /// <param name="packageId">Mod 的唯一包标识符。</param>
    /// <param name="rarity">游戏内部的稀有度枚举值。</param>
    /// <returns>匹配的 <see cref="RarityXmlInfo"/> 实例；若未找到或 Mod 未加载，则返回 null。</returns>
    public static RarityXmlInfo GetRarityXmlInfo(string packageId, Rarity rarity) {
        if (RarityDict.TryGetValue(packageId, out var rarityInfo)) {
            return rarityInfo.rarityXmlList.Find(x => rarity == x.Rarity);
        }
        return null!;
    }

    /// <summary>
    /// 将稀有度名称反向解析为游戏内部的 <see cref="Rarity"/> 枚举值。
    /// </summary>
    /// <param name="packageId">Mod 的唯一包标识符。</param>
    /// <param name="rarityName">稀有度的显示名称。</param>
    /// <returns>对应的 <see cref="Rarity"/> 枚举值；若未找到匹配项，则回退返回默认值 <see cref="Rarity.Common"/>。</returns>
    public static Rarity GetRarity(string packageId, string rarityName) {
        RarityXmlInfo? info = GetRarityXmlInfo(packageId, rarityName);
        return info != null ? info.Rarity : Rarity.Common;
    }

    /// <summary>
    /// 将一组稀有度配置数据添加或合并到指定 Mod 的缓存中。
    /// </summary>
    /// <param name="packageId">Mod 的唯一包标识符。</param>
    /// <param name="rarityList">待添加的稀有度配置列表。</param>
    /// <exception cref="ArgumentNullException">若 <paramref name="rarityList"/> 为 null，或目标 <c>rarityXmlList</c> 未初始化时抛出。</exception>
    public static void AddRarity(string packageId, List<RarityXmlInfo> rarityList) {
        if (!RarityDict.TryGetValue(packageId, out var root)) {
            root = new RarityXmlRoot();
            RarityDict[packageId] = root;
        }

        if (rarityList != null) {
            root.rarityXmlList?.AddRange(rarityList);
        }
    }

    /// <summary>
    /// 从本地 XML 文件读取稀有度配置，解析后合并至全局数据模型。
    /// </summary>
    /// <param name="packageId">Mod 的唯一包标识符。</param>
    /// <param name="filePath">XML 配置文件的物理路径。</param>
    /// <param name="fileName">XML 文件的名称（用于日志或内部标识）。</param>
    public static void AddRarityByFile(string packageId, string filePath, string? FileName = null) {
        XmlHelper.Merge<RarityXmlRoot>(filePath, FileName, root => {
            AddRarity(packageId, root.rarityXmlList);
            foreach (var rarity in root.rarityXmlList) {
                APILogger.LogInfo(rarity.ToString());
            }
        });
    }
}