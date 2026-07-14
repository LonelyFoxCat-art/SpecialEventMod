using LOR_DiceSystem;
using System.Xml.Serialization;
using VersatileSpecialEventAPI.CustomRarityUtil.XML.Wrapper;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML;

/// <summary>
/// Mod 骰子卡牌 (Dice Card) 的 XML 数据模型。
/// <br/>
/// 负责承载从 Mod 配置文件反序列化的原始卡牌属性，并提供向游戏原生运行时模型转换的能力。
/// </summary>
public class ModDiceCardXmlInfo {
    [XmlIgnore] public LorId id => new LorId(workshopID, Id);
    [XmlIgnore] public string workshopID = "";
    [XmlIgnore] public bool isError;

    [XmlAttribute("ID")] public int Id = -1;

    [XmlElement("Name")] public string workshopName = "";
    [XmlElement("TextId")] public int _textId = -1;
    [XmlElement("Artwork")] public string Artwork = "";
    [XmlElement("Rarity")] public string Rarity = "";

    [XmlElement("Option")] public List<CardOption> optionList = new List<CardOption>();
    [XmlElement("Keyword")] public List<string> Keywords = new List<string>();
    [XmlElement("Spec")] public DiceCardSpec Spec = new DiceCardSpec();

    [XmlElement("Script")] public string Script = "";
    [XmlElement("ScriptDesc")] public string ScriptDesc = "";

    [XmlArray("BehaviourList"), XmlArrayItem("Behaviour")]
    public List<DiceBehaviour> DiceBehaviourList = new List<DiceBehaviour>();

    [XmlElement("Chapter")] public int Chapter;
    [XmlElement("SpecialEffect")] public string SpecialEffect = "";
    [XmlElement("SkinChange")] public string SkinChange = "";
    [XmlElement("SkinChangeType")] public CardSkinType SkinChangeType;
    [XmlElement("SkinHeight")] public int SkinHeight;
    [XmlElement("MapChange")] public string MapChange = "";

    [XmlElement("Priority")] public int Priority;
    [XmlElement("PriorityScript")] public string PriorityScript = "";

    [XmlElement("Category")] public BookCategory category;
    [XmlElement("MaxCooltimeForEgo")] public int EgoMaxCooltimeValue = 9;
    [XmlElement("MaxNum")] public int MaxNum = 150;

    /// <summary>
    /// 定义卡牌的附加选项或标签，用于控制卡牌的获取、使用及显示逻辑。
    /// </summary>
    public enum CardOption {
        Basic,
        OnlyPage,
        EGO,
        EgoPersonal,
        Personal,
        NoInventory,
        ExhaustOnUse,
        EgoChange,
        MultipleCard = 327196
    }

    /// <summary>
    /// 将当前的 Mod XML 数据模型映射并转换为游戏原生的 <see cref="DiceCardXmlInfo"/> 运行时实例。
    /// </summary>
    /// <param name="packageId">当前 Mod 的包标识符，用于解析自定义稀有度及绑定相对资源 ID。</param>
    /// <param name="deepCopy">
    /// 是否对 <see cref="DiceBehaviourList"/> 执行深拷贝。
    /// 默认为 <c>false</c>（浅拷贝），以节省内存；若运行时逻辑会修改行为列表，请设为 <c>true</c>。
    /// </param>
    /// <returns>包含完整运行时数据的 <see cref="DiceCardXmlInfo"/> 实例。</returns>
    public DiceCardXmlInfo Copy(string packageId, bool deepCopy = false) {
        List<DiceBehaviour> behaviourList = deepCopy ? DiceBehaviourList?.Select(b => b.Copy()).ToList() ?? new List<DiceBehaviour>() : DiceBehaviourList;
        List<LOR_DiceSystem.CardOption> convertedOptions = optionList?.Select(o => (LOR_DiceSystem.CardOption)o).ToList() ?? new List<LOR_DiceSystem.CardOption>();
        Rarity rarity = (Enum.IsDefined(typeof(Rarity), Rarity) ? ((Rarity)Enum.Parse(typeof(Rarity), Rarity)) : RarityXmlWrapper.GetRarity(packageId, Rarity!));

        return new DiceCardXmlInfo {
            _id = Id,
            workshopID = packageId,
            workshopName = workshopName,
            _textId = _textId,
            Artwork = Artwork,
            Rarity = rarity,
            optionList = convertedOptions,
            Keywords = Keywords,
            Spec = Spec,
            Script = Script,
            ScriptDesc = ScriptDesc,
            DiceBehaviourList = behaviourList,
            Chapter = Chapter,
            SpecialEffect = SpecialEffect,
            SkinChange = SkinChange,
            SkinChangeType = SkinChangeType,
            SkinHeight = SkinHeight,
            MapChange = MapChange,
            Priority = Priority,
            PriorityScript = PriorityScript,
            category = category,
            EgoMaxCooltimeValue = EgoMaxCooltimeValue,
            MaxNum = MaxNum
        };
    }
}