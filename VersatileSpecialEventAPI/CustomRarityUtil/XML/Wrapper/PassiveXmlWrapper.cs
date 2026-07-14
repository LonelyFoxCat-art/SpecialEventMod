using VersatileSpecialEventAPI.Utils;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML.Wrapper;

/// <summary>
/// 被动技能 (Passive) XML 数据包装器。
/// <br/>
/// 负责将 Mod 提供的被动技能配置数据桥接并同步至游戏全局数据模型中。
/// </summary>
public class PassiveXmlWrapper {
    /// <summary>
    /// 全局被动技能数据列表的单例实例引用，用于执行实际的数据注册与查询。
    /// </summary>
    public static PassiveXmlList PassiveInstance = Singleton<PassiveXmlList>.Instance;

    /// <summary>
    /// 检查指定的 Mod 被动技能是否已存在于全局数据列表中。
    /// </summary>
    /// <param name="passive">待检查的 Mod 被动技能信息对象。</param>
    /// <returns>若存在匹配的被动技能返回 true，否则返回 false。</returns>
    public static bool IsPassiveExist(ModPassiveXmlInfo passive) =>
        PassiveInstance.GetDataAll().Exists(x => (x._id == passive.Id && x.workshopID == passive.workshopID));

    /// <summary>
    /// 将一组 Mod 被动技能数据批量添加至全局数据模型中。
    /// </summary>
    /// <param name="packageId">Mod 的唯一包标识符，用于溯源和隔离。</param>
    /// <param name="PassiveList">待添加的 Mod 被动技能信息列表。</param>
    public static void AddPassives(string packageId, List<ModPassiveXmlInfo> PassiveList) {
        foreach (var passive in PassiveList) {
            if (IsPassiveExist(passive)) PassiveInstance.AddPassivesByMod([passive.Copy(packageId)]);
        }
    }

    /// <summary>
    /// 从本地 XML 文件读取被动技能配置，解析后批量添加至全局数据模型。
    /// </summary>
    /// <param name="packageId">Mod 的唯一包标识符。</param>
    /// <param name="FilePath">XML 配置文件的物理路径。</param>
    /// <param name="FileName">XML 文件的名称（用于日志或内部标识）。</param>
    /// <exception cref="NullReferenceException">若 XML 解析成功但 <c>root.passiveXmlList</c> 为 null 时抛出。</exception>
    public static void AddPassiveByFile(string packageId, string FilePath, string? FileName = null) {
        XmlHelper.Merge<ModPassiveXmlRoot>(FilePath, FileName, root => AddPassives(packageId, root!.passiveXmlList));
    }
}