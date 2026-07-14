using System.Xml.Serialization;

namespace VersatileSpecialEventAPI.LocalizationUtil;

public class TextXmlInfo {
    [XmlAttribute("Name")] public string Name = string.Empty;
    [XmlText] public string Text = string.Empty;
}