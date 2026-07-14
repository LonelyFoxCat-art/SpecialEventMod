using System.Xml.Serialization;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML;

/// <summary>
/// Mod 稀有度 (Rarity) XML 配置文件的根节点数据模型。
/// </summary>
[XmlRoot("RarityXmlRoot")]
public class RarityXmlRoot {
    /// <summary>
    /// 包含在当前根节点下的所有稀有度配置信息集合。
    /// </summary>
    [XmlElement("Rarity")] public List<RarityXmlInfo> rarityXmlList = new List<RarityXmlInfo>();
}