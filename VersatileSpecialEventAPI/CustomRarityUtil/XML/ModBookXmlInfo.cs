using CustomInvitation;
using System.Xml.Serialization;
using VersatileSpecialEventAPI.CustomRarityUtil.XML.Wrapper;
using VersatileSpecialEventAPI.Utils;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML;

/// <summary>
/// Mod 书籍 (Book) 的 XML 数据模型。
/// <br/>
/// 负责承载从 Mod 配置文件反序列化的原始书籍属性，并提供向游戏原生运行时模型转换的能力。
/// </summary>
public class ModBookXmlInfo {
    [XmlIgnore] public LorId id { get { return new LorId(workshopID, Id); } }
    [XmlIgnore] public string workshopID = string.Empty;

    [XmlAttribute("ID")] public int Id = -1;

    [XmlElement("Name")] public string InnerName = "";
    [XmlElement("TextId")] public int TextId = -1;
    [XmlElement("BookIcon")] public string _bookIcon = "";

    [XmlElement("Option")] public List<BookOption> optionList = new List<BookOption>();
    [XmlElement("Category")] public List<BookCategory> categoryList = new List<BookCategory>();

    [XmlElement("EquipEffect")] public ModBookEquipEffect EquipEffect = new ModBookEquipEffect();

    [XmlElement("Rarity")] public string Rarity = "";

    [XmlElement("CharacterSkin")] public List<string> CharacterSkin = new List<string>();
    [XmlElement("CharacterSkinType")] public string skinType = "Lor";
    [XmlElement("SkinGender")] public Gender gender = Gender.N;

    [XmlElement("Chapter")] public int Chapter = 1;
    [XmlElement("Episode")] public int episode = -1;

    [XmlElement("RangeType")] public EquipRangeType RangeType;
    [XmlElement("NotEquip")] public bool canNotEquip;
    [XmlElement("RandomFace")] public bool RandomFace = false;

    [XmlElement("SpeedDice")] public string SpeedDice = string.Empty;
    [XmlElement("SpeedDiceNum")] public int speedDiceNumber = 1;
    [XmlElement("SpeedDiceColor")] public string SpeedDiceColor = string.Empty;
    [XmlElement("SuccessionPossibleNumber")] public int SuccessionPossibleNumber = 9;

    [XmlElement("SoundInfo")] public List<BookSoundInfo> motionSoundList = new List<BookSoundInfo>();

    /// <summary>
    /// 将当前的 Mod XML 数据模型映射并转换为游戏原生的 <see cref="BookXmlInfo"/> 运行时实例。
    /// </summary>
    /// <param name="packageId">当前 Mod 的包标识符，用于解析自定义稀有度及绑定相对资源 ID。</param>
    /// <returns>包含完整运行时数据的 <see cref="BookXmlInfo"/> 实例。</returns>
    public BookXmlWrapper Copy(string packageId) {
        Rarity rarity = (Enum.IsDefined(typeof(Rarity), Rarity) ? ((Rarity)Enum.Parse(typeof(Rarity), Rarity)) : RarityXmlWrapper.GetRarity(packageId, Rarity!));
        var EquipEffectList = new BookEquipEffectWrapper();
        return new BookXmlWrapper {
            _id = Id,
            workshopID = packageId,
            InnerName = InnerName,
            TextId = TextId,
            _bookIcon = _bookIcon,
            optionList = optionList,
            categoryList = categoryList,
            EquipEffect = EquipEffect.Copy(packageId),
            canNotEquip = this.canNotEquip,
            Chapter = this.Chapter,
            CharacterSkin = this.CharacterSkin,
            episode = this.episode,
            gender = this.gender,
            motionSoundList = this.motionSoundList,
            RandomFace = this.RandomFace,
            RangeType = this.RangeType,
            Rarity = rarity,
            skinType = this.skinType,
            speedDice = this.SpeedDice,
            speedDiceNumber = this.speedDiceNumber,
            speedDiceColor = ColorHelper.FromHex(this.SpeedDiceColor),
            SuccessionPossibleNumber = this.SuccessionPossibleNumber
        };
    }
}