using System.Xml.Serialization;
using VersatileSpecialEventAPI.CustomRarityUtil.XML.Wrapper;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML;

/// <summary>
/// Mod 被动技能 (Passive) 的 XML 数据模型。
/// <br/>
/// 负责承载从 Mod 配置文件反序列化的原始被动技能属性，并提供向游戏原生运行时模型转换的能力。
/// </summary>
public class ModPassiveXmlInfo {
    [XmlIgnore] public LorId id { get { return new LorId(workshopID, Id); } }
    [XmlIgnore] public string workshopID = "";
    [XmlIgnore] public bool isError;

    [XmlAttribute("ID")] public int Id = -1;
    [XmlElement("Level")] public int level = -1;
    [XmlElement("Negative")] public bool isNegative;
    [XmlElement("IsHide")] public bool isHide;
    [XmlElement("Param")] public List<int> param = new List<int>();
    [XmlElement("Rarity")] public string? rare;
    [XmlElement("Lock")] public bool isLock;
    [XmlElement("CanGivePassive")] public bool CanGivePassive = true;
    [XmlElement("CanReceivePassive")] public bool CanReceivePassive;
    [XmlElement("InnerType")] public int InnerTypeId = -1;
    [XmlElement("Cost")] public int cost = 1;
    [XmlElement("Script")] public string script = "";
    [XmlElement("Name")] public string name = "";
    [XmlElement("Desc")] public string desc = "";

    /// <summary>
    /// 将当前的 Mod XML 数据模型映射并转换为游戏原生的 <see cref="PassiveXmlInfo"/> 运行时实例。
    /// </summary>
    public PassiveXmlInfo Copy(string packageId) {
        Rarity rarity = (Enum.IsDefined(typeof(Rarity), rare) ? ((Rarity)Enum.Parse(typeof(Rarity), rare)) : RarityXmlWrapper.GetRarity(packageId, rare!));
        return new PassiveXmlInfo {
            CanGivePassive = CanGivePassive,
            CanReceivePassive = CanReceivePassive,
            cost = cost,
            desc = desc,
            InnerTypeId = InnerTypeId,
            isError = isError,
            isHide = isHide,
            isLock = isLock,
            isNegative = isNegative,
            _id = Id,
            workshopID = packageId,
            level = level,
            name = name,
            param = param,
            rare = rarity,
            script = script
        };
    }
}