using UnityEngine;
using static VersatileSpecialEventAPI.API;

namespace VersatileSpecialEventAPI.Utils;

public static class SpriteHelper {
    private static readonly Dictionary<string, Sprite> _artWorksList = new Dictionary<string, Sprite>();

    /// <summary>
    /// 获取指定名称的 Sprite 实例。
    /// </summary>
    /// <param name="name">Sprite 的唯一标识名称。</param>
    /// <returns>对应的 Sprite 实例。</returns>
    /// <exception cref="KeyNotFoundException">当指定的 name 不存在于缓存字典中时抛出。</exception>
    public static Sprite GetData(string name) => _artWorksList[name];

    /// <summary>
    /// 获取当前缓存的所有 Sprite 数据字典引用。
    /// </summary>
    /// <returns>包含所有已加载 Sprite 的字典实例。</returns>
    /// <remarks>建议后续版本将返回类型升级为 IReadOnlyDictionary&lt;string, Sprite&gt; 以防止外部意外修改缓存。</remarks>
    public static Dictionary<string, Sprite> GetDataAll() => _artWorksList;

    /// <summary>
    /// 获取指定名称 Sprite 关联的底层 Texture2D 纹理。
    /// </summary>
    /// <param name="name">Sprite 的唯一标识名称。</param>
    /// <returns>对应的 Texture2D 实例。</returns>
    /// <exception cref="KeyNotFoundException">当指定的 name 不存在于缓存中时抛出。</exception>
    /// <exception cref="NullReferenceException">当缓存中的 Sprite 实例为 null 时抛出。</exception>
    public static Texture2D GetTexture(string name) {
        var sprite = GetData(name);
        return sprite?.texture ?? throw new NullReferenceException($"The cached Sprite for '{name}' is null.");
    }

    /// <summary>
    /// 检查指定名称的 Sprite 是否已加载并缓存。
    /// </summary>
    /// <param name="name">Sprite 的唯一标识名称。</param>
    /// <returns>若存在返回 true，否则返回 false。</returns>
    public static bool IsSpriteExist(string name) => _artWorksList.ContainsKey(name);

    /// <summary>
    /// 将 Sprite 实例添加到内存缓存中。
    /// </summary>
    /// <param name="name">用于在缓存中注册的唯一标识名称。</param>
    /// <param name="sprite">要缓存的 Sprite 实例。</param>
    public static void AddSprite(string name, Sprite sprite) {
        if (sprite == null) return;

        if (!_artWorksList.ContainsKey(name)) {
            _artWorksList[name] = sprite;
            APILogger.LogInfo($"SpriteHelper -> 添加图片: {name}[Sprite]");
        }
    }

    /// <summary>
    /// 从本地文件系统读取图片文件，动态解析为 Texture2D 并构建 Sprite 后加入缓存。
    /// </summary>
    /// <param name="filePath">图片文件的绝对或相对物理路径 (支持 PNG, JPG 等 Unity 支持的格式)。</param>
    /// <param name="fileName">用于在缓存中注册该 Sprite 的唯一标识名称。</param>
    public static void AddSpriteByFile(string filePath, string fileName) {
        Texture2D texture = new Texture2D(2, 2);
        var loadSuccess = false;

        try {
            byte[] fileData = File.ReadAllBytes(filePath);
            if (ImageConversion.LoadImage(texture, fileData)) {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                AddSprite(fileName, sprite);
                loadSuccess = true;
            }
        } finally {
            if (!loadSuccess) UnityEngine.Object.Destroy(texture);
        }
    }
}