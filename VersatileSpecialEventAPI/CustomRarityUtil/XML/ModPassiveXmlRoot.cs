using System.Xml.Serialization;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML;

/// <summary>
/// Mod 被动技能 (Passive) XML 配置文件的根节点数据模型。
/// </summary>
[XmlRoot("PassiveXmlRoot")]
public class ModPassiveXmlRoot {
    /// <summary>
    /// 包含在当前根节点下的所有被动技能配置信息集合。
    /// </summary>
    [XmlElement("Passive")] public List<ModPassiveXmlInfo> passiveXmlList = new List<ModPassiveXmlInfo>();
}