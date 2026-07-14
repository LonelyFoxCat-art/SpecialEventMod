using UnityEngine;
using VersatileSpecialEventAPI.Utils;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML.Wrapper;

/// <summary>
/// 书籍 XML 数据包装器。
/// <br/>
/// 负责将 Mod 提供的书籍配置数据桥接并同步至游戏全局单例数据模型中。
/// </summary>
public class BookXmlWrapper : BookXmlInfo {
    public string speedDice = string.Empty;
    public Color speedDiceColor = Color.white;

    /// <summary>
    /// 全局书籍数据列表的单例实例引用。
    /// </summary>
    public static BookXmlList BookInstance = Singleton<BookXmlList>.Instance;

    public string SpeedDice => speedDice;
    public Color SpeedDiceColor => speedDiceColor;

    /// <summary>
    /// 检查指定的 Mod 书籍是否已存在于全局数据列表中。
    /// </summary>
    /// <param name="book">待检查的 Mod 书籍信息对象。</param>
    /// <returns>若存在匹配的书籍返回 true，否则返回 false。</returns>
    public static bool IsBookExist(ModBookXmlInfo book) => BookInstance.GetList().Exists(x => (x._id == book.Id && x.workshopID == book.workshopID));

    /// <summary>
    /// 将一组 Mod 书籍数据批量添加至全局数据模型中。
    /// </summary>
    /// <param name="packageId">Mod 的唯一包标识符，用于溯源和隔离。</param>
    /// <param name="BookList">待添加的 Mod 书籍信息列表。</param>
    public static void AddBookByFile(string packageId, string FilePath, string? FileName = null) {
        XmlHelper.Merge<ModBookXmlRoot>(FilePath, FileName, root => BookInstance.AddEquipPageByMod(packageId, root.Copy()));
    }
}