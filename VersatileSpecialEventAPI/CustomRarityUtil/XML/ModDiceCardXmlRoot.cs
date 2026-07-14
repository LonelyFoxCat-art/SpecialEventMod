using System.Xml.Serialization;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML;

/// <summary>
/// Mod 骰子卡牌 XML 配置文件的根节点数据模型。
/// </summary>
[XmlRoot("DiceCardXmlRoot")]
public class ModDiceCardXmlRoot {
    /// <summary>
    /// 包含在当前根节点下的所有骰子卡牌配置信息集合。
    /// </summary>
    [XmlElement("Card")] public List<ModDiceCardXmlInfo> diceXmlList = new List<ModDiceCardXmlInfo>();
}