using LOR_DiceSystem;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using VersatileSpecialEventAPI.CustomRarityUtil.Effect;
using VersatileSpecialEventAPI.Utils;
using static VersatileSpecialEventAPI.API;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML;

#region 枚举定义

public enum RarityArtworkType {
    LeftFrame,
    RightFrame,
    FrontFrame,
    PassiveIcon,
    RangeIcon
}

public enum RarityColorType {
    Frame,
    FrameLinear,
    RangeIcon,
    AbilityDesc,
    AbilityKeyword
}

/// <summary>
/// 稀有度掉落配置的类型枚举。
/// </summary>
public enum RarityDropType {
    Card,
    Equip
}

#endregion

/// <summary>
/// 稀有度 (Rarity) 配置的 XML 数据模型。
/// <br/>
/// 负责承载稀有度的视觉表现（颜色、图标）、掉落规则及卡组限制等配置，并提供运行时所需的类型转换与查询能力。
/// </summary>
public class RarityXmlInfo {
    #region 内部数据模型 (嵌套类)

    /// <summary>
    /// 稀有度相关的单个颜色配置。
    /// </summary>
    public class RarityColor {
        [XmlAttribute("Type")] public RarityColorType type;
        [XmlAttribute("Value")] public string Value = string.Empty;
    }

    /// <summary>
    /// 稀有度相关的单个视觉素材（边框、图标）路径配置。
    /// </summary>
    public class RarityArtwork {
        [XmlAttribute("Type")] public RarityArtworkType type;
        [XmlAttribute("Name")] public string ArtworkName = string.Empty;

        /// <summary>
        /// 仅当 Type 为 RangeIcon 时生效，用于区分不同的攻击范围。
        /// </summary>
        [XmlAttribute("Range")] public CardRange range;
    }

    /// <summary>
    /// 稀有度相关的单个掉落概率与数量配置。
    /// </summary>
    public class RarityDropInfo {
        [XmlAttribute("Type")] public RarityDropType type;
        [XmlAttribute("DropRate")] public float DropRate = 0.4f;
        [XmlAttribute("DropMax")] public int DropMax = 1;
    }

    #endregion

    #region XML 序列化字段

    [XmlAttribute("ID")] public int Id = -1;
    [XmlElement("Name")] public string RarityName = "";

    [XmlElement("FrameEffect")] public FrameEffect FrameEffect = FrameEffect.None;

    [XmlElement("RarityColor")] public List<RarityColor> RarityColors = new List<RarityColor>();
    [XmlElement("RarityArtwork")] public List<RarityArtwork> RarityArtworks = new List<RarityArtwork>();
    [XmlElement("DropInfo")] public List<RarityDropInfo> DropInfos = new List<RarityDropInfo>();

    [XmlElement("DeckLimit")] public int DeckLimit = 3;
    [XmlElement("FrontFrameApplyColor")] public bool frontFrameApplyColor = false;

    #endregion

    #region 运行时推导属性

    private Color GetColor(RarityColorType type) {
        var found = RarityColors.Find(x => x.type == type);
        return ColorHelper.FromHex(found.Value);
    }

    public RarityDropInfo GetDropInfo(RarityDropType type) {
        return DropInfos.Find(x => x.type == type) ?? null!;
    }

    [XmlIgnore] public Rarity Rarity => (Rarity)(Rarity.Special + Id);

    [XmlIgnore] public Color FrameColor => GetColor(RarityColorType.Frame);
    [XmlIgnore] public Color FrameLinearColor => GetColor(RarityColorType.FrameLinear);
    [XmlIgnore] public Color RangeIconColor => GetColor(RarityColorType.RangeIcon);
    [XmlIgnore] public Color AbilityDescColor => GetColor(RarityColorType.AbilityDesc);
    [XmlIgnore] public Color AbilityKeywordColor => GetColor(RarityColorType.AbilityKeyword);
    [XmlIgnore] public RarityDropInfo CardDropInfo => GetDropInfo(RarityDropType.Card);
    [XmlIgnore] public RarityDropInfo EquipDropInfo => GetDropInfo(RarityDropType.Equip);

    [XmlIgnore]
    public Vector3 RangeIconColorHsv {
        get {
            Color.RGBToHSV(RangeIconColor, out var x, out var y, out var z);
            return new Vector3(x, y, z);
        }
    }

    #endregion

    #region 公共方法

    public Sprite GetArtwork(RarityArtworkType type) {
        var found = RarityArtworks.Find(x => x.type == type);
        if (found == null || string.IsNullOrEmpty(found.ArtworkName) || !SpriteHelper.IsSpriteExist(found.ArtworkName)) return null!;
        return SpriteHelper.GetData(found.ArtworkName);
    }

    public string GetArtworkName(RarityArtworkType type) {
        if (RarityArtworks == null) return string.Empty;
        var found = RarityArtworks.Find(x => x.type == type);
        if (found == null || string.IsNullOrEmpty(found.ArtworkName)) return string.Empty;
        return found.ArtworkName;
    }

    public string GetRangeIconName(CardRange cardRange) {
        if (RarityArtworks == null) return string.Empty;
        var found = RarityArtworks.Find(x => x.type == RarityArtworkType.RangeIcon && x.range == cardRange);
        if (found == null || string.IsNullOrEmpty(found.ArtworkName)) return string.Empty;
        return found.ArtworkName;
    }

    public Sprite GetRangeIcon(CardRange cardRange) {
        string iconName = GetRangeIconName(cardRange);
        if (string.IsNullOrEmpty(iconName)) return null!;

        if (SpriteHelper.GetDataAll().TryGetValue(ModId, out var Artworks)) return Artworks;
        return null!;
    }

    public override string ToString() {
        var sb = new StringBuilder();

        // 基础信息
        sb.AppendLine($"[RarityXmlInfo] ID: {Id}, Name: {RarityName}, Enum: {Rarity}");
        sb.AppendLine($"  FrameEffect: {FrameEffect} | DeckLimit: {DeckLimit} | ApplyColor: {frontFrameApplyColor}");

        // 颜色配置
        sb.AppendLine("  [Colors]");
        if (RarityColors != null && RarityColors.Count > 0) {
            foreach (var color in RarityColors) {
                sb.AppendLine($"    {color.type}: {color.Value}");
            }
        } else {
            sb.AppendLine("    (Empty)");
        }

        // 素材配置
        sb.AppendLine("  [Artworks]");
        if (RarityArtworks != null && RarityArtworks.Count > 0) {
            foreach (var artwork in RarityArtworks) {
                string rangeStr = Enum.IsDefined(typeof(CardRange), artwork.range) ? $" [Range: {artwork.range}]" : string.Empty;
                sb.AppendLine($"    {artwork.type}: {artwork.ArtworkName}{rangeStr}");
            }
        } else {
            sb.AppendLine("    (Empty)");
        }

        // 掉落配置
        sb.AppendLine("  [DropInfos]");
        if (DropInfos != null && DropInfos.Count > 0) {
            foreach (var drop in DropInfos) {
                sb.AppendLine($"    {drop.type}: Rate={drop.DropRate}, Max={drop.DropMax}");
            }
        } else {
            sb.AppendLine("    (Empty)");
        }

        return sb.ToString().TrimEnd();
    }
    #endregion
}