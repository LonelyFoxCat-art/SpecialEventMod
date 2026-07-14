using System.Xml.Serialization;
using static VersatileSpecialEventAPI.API;

namespace VersatileSpecialEventAPI.CustomRarityUtil.XML;

/// <summary>
/// Mod 书籍 XML 配置文件的根节点数据模型。
/// </summary>
[XmlRoot("BookXmlRoot")]
public class ModBookXmlRoot {
    /// <summary>
    /// 包含在当前根节点下的所有书籍配置信息集合。
    /// </summary>
    [XmlElement("Book")] public List<ModBookXmlInfo> bookXmlList = new List<ModBookXmlInfo>();

    public List<BookXmlInfo> Copy() {
        var copy = new List<BookXmlInfo>();
        foreach (var bookXml in bookXmlList) copy.Add(bookXml.Copy(ModId));
        return copy;
    }
}