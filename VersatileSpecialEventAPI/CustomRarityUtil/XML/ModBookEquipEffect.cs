using System.Xml.Serialization;
using VersatileSpecialEventAPI.CustomRarityUtil.XML.Wrapper;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML;

/// <summary>
/// Mod 书籍装备效果 (Book Equip Effect) 的 XML 数据模型。
/// 负责承载从配置文件反序列化的原始属性，并提供向运行时包装器 (<see cref="BookEquipEffectWrapper"/>) 转换的能力。
/// </summary>
public class ModBookEquipEffect {
    [XmlIgnore] public int MaxPlayPoint = 3;
    [XmlIgnore] public List<LorId> PassiveList = new List<LorId>();
    [XmlIgnore] public int PassiveCost = 10;

    [XmlElement("HpReduction")] public int HpReduction;
    [XmlElement("HP")] public int Hp;
    [XmlElement("DeadLine")] public int DeadLine;
    [XmlElement] public int Break;
    [XmlElement("SpeedMin")] public int SpeedMin;
    [XmlElement] public int Speed;
    [XmlElement] public int SpeedDiceNum;

    [XmlElement] public AtkResist SResist = AtkResist.Normal;
    [XmlElement] public AtkResist PResist = AtkResist.Normal;
    [XmlElement] public AtkResist HResist = AtkResist.Normal;
    [XmlElement] public AtkResist SBResist = AtkResist.Normal;
    [XmlElement] public AtkResist PBResist = AtkResist.Normal;
    [XmlElement] public AtkResist HBResist = AtkResist.Normal;


    [XmlElement("StartPlayPoint")] public int StartPlayPoint = 3;
    [XmlElement("AddedStartDraw")] public int AddedStartDraw;

    [XmlElement("OnlyCard")] public List<LorIdXml> OnlyCard = new List<LorIdXml>();
    [XmlElement("Card")] public List<BookSoulCardInfo> CardList = new List<BookSoulCardInfo>();
    [XmlElement("Passive")] public List<LorIdXml> _PassiveList = new List<LorIdXml>();

    /// <summary>
    /// 将当前的 XML 数据模型转换并映射为运行时使用的 <see cref="BookEquipEffectWrapper"/> 实例。
    /// </summary>
    /// <param name="packageId">当前 Mod 的包标识符，用于解析和绑定相对资源 ID。</param>
    /// <returns>包含完整运行时数据的 <see cref="BookEquipEffectWrapper"/> 实例。</returns>
    public BookEquipEffectWrapper Copy(string packageId) {
        return new BookEquipEffectWrapper {
            HpReduction = HpReduction,
            Hp = Hp,
            DeadLine = DeadLine,
            Break = Break,
            SpeedMin = SpeedMin,
            Speed = Speed,
            SpeedDiceNum = SpeedDiceNum,
            SResist = SResist,
            PResist = PResist,
            HResist = HResist,
            SBResist = SBResist,
            PBResist = PBResist,
            HBResist = HBResist,
            MaxPlayPoint = MaxPlayPoint,
            StartPlayPoint = StartPlayPoint,
            AddedStartDraw = AddedStartDraw,
            PassiveCost = PassiveCost,
            OnlyCard = (from x in OnlyCard select x.xmlId).ToList(),
            OnlyCardPackageId = (from x in OnlyCard select x.pid).ToList(),
            CardList = CardList,
            _PassiveList = _PassiveList,
            PassiveList = (from x in _PassiveList select LorId.MakeLorId(x, packageId)).ToList()
        };
    }
}