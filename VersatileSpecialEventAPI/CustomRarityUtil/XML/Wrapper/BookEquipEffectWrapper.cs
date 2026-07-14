namespace VersatileSpecialEventAPI.CustomRarityUtil.XML.Wrapper;

/// <summary>
/// 书籍装备效果的运行时包装器。
/// <br/>
/// 继承自 <see cref="BookEquipEffect"/>，用于在内存中存储经过解析、扁平化或动态推导后的运行时数据。
/// </summary>
public class BookEquipEffectWrapper : BookEquipEffect {
    /// <summary>
    /// 限定可用卡牌所属的 Mod 包标识符 (Package ID) 列表。
    /// </summary>
    public List<string> OnlyCardPackageId = new List<string>();
}