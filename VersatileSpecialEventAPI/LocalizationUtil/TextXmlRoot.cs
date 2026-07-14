using System.Xml.Serialization;

namespace VersatileSpecialEventAPI.LocalizationUtil;

public class TextXmlRoot {
    [XmlElement("Text")] public List<TextXmlInfo> TextXmlList = new List<TextXmlInfo>();
}