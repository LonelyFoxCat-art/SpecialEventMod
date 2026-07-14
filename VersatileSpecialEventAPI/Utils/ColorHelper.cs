using UnityEngine;

namespace VersatileSpecialEventAPI.Utils;

/// <summary>
/// 提供针对 Unity Color/Color32 的便捷操作、格式转换及 XML 序列化桥接辅助函数
/// </summary>
public static class ColorHelper {
    #region Hex 字符串互转 (支持 #RGB, #RRGGBB, #RRGGBBAA)

    /// <summary>
    /// 从 Hex 字符串解析颜色。自动处理 '#' 前缀，支持 3位、6位、8位(带Alpha) 格式。
    /// </summary>
    public static Color FromHex(this string hex) {
        if (string.IsNullOrEmpty(hex)) return Color.clear;
        hex = hex.TrimStart('#');

        // 兼容 #RGB 格式 (如 #F00 -> #FF0000)
        if (hex.Length == 3) {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }

        // Unity 原生方法支持 #RRGGBB 和 #RRGGBBAA
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color color)) {
            return color;
        }

        return Color.clear;
    }

    /// <summary>
    /// 将颜色转换为 Hex 字符串 (输出格式带有 '#' 前缀)。
    /// </summary>
    /// <param name="includeAlpha">是否包含 Alpha 通道 (输出 #RRGGBBAA)</param>
    public static string ToHex(this Color color, bool includeAlpha = false) {
        string hex = includeAlpha ? ColorUtility.ToHtmlStringRGBA(color) : ColorUtility.ToHtmlStringRGB(color);
        return "#" + hex;
    }

    #endregion

    #region 整数互转与创建

    /// <summary>
    /// 从 0-255 的整数 RGB(A) 值创建 Color。
    /// 相比直接除以 255f，此方法通过 Color32 自动处理字节截断，防止越界导致的颜色异常。
    /// </summary>
    /// <param name="r">红色通道 (0-255)</param>
    /// <param name="g">绿色通道 (0-255)</param>
    /// <param name="b">蓝色通道 (0-255)</param>
    /// <param name="a">透明度通道 (0-255)，默认 255</param>
    public static Color FromIntRGB(int r, int g, int b, int a = 255) {
        // 使用 Mathf.Clamp 确保输入值在 0-255 之间，防止负数或超大值引发异常
        byte safeR = (byte)Mathf.Clamp(r, 0, 255);
        byte safeG = (byte)Mathf.Clamp(g, 0, 255);
        byte safeB = (byte)Mathf.Clamp(b, 0, 255);
        byte safeA = (byte)Mathf.Clamp(a, 0, 255);

        // Color32 会自动隐式转换为 UnityEngine.Color
        return new Color32(safeR, safeG, safeB, safeA);
    }

    /// <summary>
    /// （别名方法）保持与您原有命名习惯兼容，内部调用 FromIntRGB。
    /// </summary
    public static Color ToColor(int r, int g, int b, int a = 255) {
        return FromIntRGB(r, g, b, a);
    }

    /// <summary>
    /// 将 32 位整数 (0xRRGGBBAA) 转换为 Color。
    /// </summary>
    public static Color FromRGBA(int rgba) {
        byte r = (byte)((rgba >> 24) & 0xFF);
        byte g = (byte)((rgba >> 16) & 0xFF);
        byte b = (byte)((rgba >> 8) & 0xFF);
        byte a = (byte)(rgba & 0xFF);
        return new Color32(r, g, b, a);
    }

    /// <summary>
    /// 将 Color 转换为 32 位整数 (0xRRGGBBAA)。
    /// </summary>
    public static int ToRGBA(Color color) {
        Color32 c = color;
        return (c.r << 24) | (c.g << 16) | (c.b << 8) | c.a;
    }


    #endregion

    #region 属性快捷修改 (不修改原对象，返回新 Color)

    /// <summary>
    /// 快速修改颜色的透明度 (0.0f - 1.0f)。
    /// </summary>
    public static Color WithAlpha(Color color, float alpha) {
        color.a = Mathf.Clamp01(alpha);
        return color;
    }

    /// <summary>
    /// 快速修改颜色的透明度 (0 - 255，对 Color32 友好)。
    /// </summary>
    public static Color WithAlpha(Color color, byte alpha) {
        Color32 c = color;
        c.a = alpha;
        return c;
    }

    /// <summary>
    /// 调整颜色亮度 (RGB 乘以 factor，Alpha 保持不变)。
    /// </summary>
    public static Color WithBrightness(Color color, float factor) {
        return new Color(
            Mathf.Clamp01(color.r * factor),
            Mathf.Clamp01(color.g * factor),
            Mathf.Clamp01(color.b * factor),
            color.a
        );
    }

    #endregion

    #region 颜色混合与生成

    /// <summary>
    /// 按权重混合两种颜色 (等同于 Color.Lerp，但语义更清晰)。
    /// </summary>
    public static Color Mix(Color a, Color b, float weight = 0.5f) {
        return Color.Lerp(a, b, Mathf.Clamp01(weight));
    }

    /// <summary>
    /// 生成随机颜色。通过限制 HSV 范围，避免生成过暗或过灰的颜色。
    /// </summary>
    public static Color RandomColor(float minSaturation = 0.5f, float minValue = 0.5f) {
        float h = UnityEngine.Random.Range(0f, 1f);
        float s = UnityEngine.Random.Range(minSaturation, 1f);
        float v = UnityEngine.Random.Range(minValue, 1f);
        return Color.HSVToRGB(h, s, v);
    }

    /// <summary>
    /// 根据索引生成在色环上均匀分布的颜色（适用于为多个玩家/实体分配不同颜色）。
    /// </summary>
    public static Color GetIndexedColor(int index, int total) {
        if (total <= 0) return Color.white;
        float hue = (float)(index % total) / total;
        return Color.HSVToRGB(hue, 0.8f, 0.9f); // 固定饱和度和亮度以保证颜色鲜艳
    }

    #endregion
}