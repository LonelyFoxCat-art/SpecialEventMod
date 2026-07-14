using LOR_DiceSystem;
using VersatileSpecialEventAPI.Utils;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML.Wrapper;

/// <summary>
/// 骰子卡牌 (Dice Card) XML 数据包装器。
/// <br/>
/// 负责将 Mod 提供的骰子卡牌配置数据桥接并同步至游戏全局数据模型中。
/// </summary>
public class DiceCardXmlWrapper : DiceCardXmlInfo {
    /// <summary>
    /// 全局卡牌数据列表的实例引用，用于执行实际的卡牌数据注册与查询。
    /// </summary>
    public static ItemXmlDataList DiceInstance = ItemXmlDataList.instance;

    /// <summary>
    /// 检查指定的 Mod 骰子卡牌是否已存在于全局数据列表中。
    /// </summary>
    /// <param name="dice">待检查的 Mod 骰子卡牌信息对象。</param>
    /// <returns>若存在匹配的卡牌返回 true，否则返回 false。</returns>
    public static bool IsDiceExist(ModDiceCardXmlInfo dice) => DiceInstance.GetCardList().Exists(x => (x._id == dice.Id && x.workshopID == dice.workshopID));

    /// <summary>
    /// 将一组 Mod 骰子卡牌数据批量添加至全局数据模型中。
    /// </summary>
    /// <param name="packageId">Mod 的唯一包标识符，用于溯源和隔离。</param>
    /// <param name="DiceList">待添加的 Mod 骰子卡牌信息列表。</param>
    public static void AddDice(string packageId, List<ModDiceCardXmlInfo> DiceList) {
        foreach (var dice in DiceList) DiceInstance.AddCardInfoByMod(packageId, [dice.Copy(packageId)]);
    }

    /// <summary>
    /// 从本地 XML 文件读取骰子卡牌配置，解析后批量添加至全局数据模型。
    /// </summary>
    /// <param name="packageId">Mod 的唯一包标识符。</param>
    /// <param name="FilePath">XML 配置文件的物理路径。</param>
    /// <param name="FileName">XML 文件的名称（用于日志或内部标识）。</param>
    /// <exception cref="NullReferenceException">若 XML 解析成功但 <c>root.diceXmlList</c> 为 null 时抛出。</exception>
    public static void AddDiceByFile(string packageId, string FilePath, string? FileName = null) {
        XmlHelper.Merge<ModDiceCardXmlRoot>(FilePath, FileName, root => AddDice(packageId, root.diceXmlList!));
    }
}