using LOR_DiceSystem;
using LOR_XML;
using static VersatileSpecialEventAPI.API;

namespace VersatileSpecialEventAPI.LocalizationUtil;

/// <summary>
/// Mod 本地化管理器。
/// <br/>
/// 负责从指定的 XML 配置文件中加载多语言文本并热更新游戏内各类实体（卡牌、书籍、角色、舞台、技能等）的显示名称与描述。
/// </summary>
public class LocalizationManager {
    // 存储各 Mod 的通用本地化键值对数据
    public static Dictionary<string, Dictionary<string, string>> LocalizeDict = new Dictionary<string, Dictionary<string, string>>();

    /// <summary>
    /// 从本地化字典中提取卡牌名称，并同步更新至卡牌实例及其关联的 Item 数据中。
    /// </summary>
    public static void GetName(DiceCardXmlInfo card, Dictionary<int, BattleCardDesc> descDict) {
        if (descDict.TryGetValue(card.id.id, out var desc)) {
            card.workshopName = desc.cardName;
            var item = ItemXmlDataList.instance.GetCardItem(card.id, false);
            if (item != null) item.workshopName = card.workshopName;
        }
    }

    /// <summary>
    /// 从本地化字典中提取书籍名称，并同步更新至书籍实例及其关联的全局数据中。
    /// </summary>
    public static void GetName(BookXmlInfo book, Dictionary<int, BookDesc> descDict) {
        if (descDict.TryGetValue(book.id.id, out var desc)) {
            book.InnerName = desc.bookName;
            var data = Singleton<BookXmlList>.Instance.GetData(book.id, true);
            if (data != null) data.InnerName = book.InnerName;
        }
    }

    /// <summary>
    /// 获取当前 Mod 上下文下指定键名的本地化纯文本。
    /// </summary>
    /// <remarks>
    /// 依赖外部上下文中的 <c>ModId</c> 来定位当前 Mod 的翻译字典。
    /// </remarks>
    /// <param name="Name">本地化文本的键名（Key）。</param>
    /// <returns>对应的本地化字符串；若键名为空或 null，则返回字面量 <c>"null"</c> 以避免空引用异常。</returns>
    /// <exception cref="KeyNotFoundException">当 <see cref="LocalizeDict"/> 中不存在当前 <c>ModId</c> 或指定的 <paramref name="Name"/> 时抛出。</exception>
    public static string GetText(string Name) {
        if (string.IsNullOrEmpty(Name)) return "null";
        return LocalizeDict[ModId][Name];
    }

    /// <summary>
    /// 获取当前 Mod 上下文下指定键名的本地化文本，并进行参数格式化。
    /// </summary>
    /// <param name="Name">本地化文本的键名（Key），文本内应包含如 <c>{0}</c> 的格式化占位符。</param>
    /// <param name="args">用于替换占位符的参数数组。</param>
    /// <returns>格式化后的本地化字符串；若键名为空，则返回字面量 <c>"null"</c>。</returns>
    /// <exception cref="KeyNotFoundException">当 <see cref="LocalizeDict"/> 中不存在当前 <c>ModId</c> 或指定的 <paramref name="Name"/> 时抛出。</exception>
    /// <exception cref="System.FormatException">当本地化文本中的占位符索引超出 <paramref name="args"/> 数组范围，或格式不正确时抛出。</exception>
    public static string GetText(string Name, params object[] args) {
        if (string.IsNullOrEmpty(Name)) return "null";
        return string.Format(LocalizeDict[ModId][Name], args);
    }
}