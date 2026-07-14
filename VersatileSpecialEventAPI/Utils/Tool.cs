using UnityEngine;
using VersatileSpecialEventAPI.CustomRarityUtil.Effect;

namespace VersatileSpecialEventAPI.Utils;

public static class Tool {
    public static Type GetFrameEffect(this FrameEffect frameEffects) => frameEffects switch {
        FrameEffect.Rainbow => typeof(RainbowFrame),
        FrameEffect.Glow => typeof(GlowFrame),
        FrameEffect.Fire => typeof(FireFrame),
        FrameEffect.Emission => typeof(EmissionFrame),
        FrameEffect.Shiny => typeof(ShinyFrame),
        _ => null!
    };

    public static void SafeDestroyAll<T>(this GameObject go) where T : MonoBehaviour {
        if (go != null) Array.ForEach(go.GetComponents<T>(), UnityEngine.Object.DestroyImmediate);
    }
    public static void SafeDestroyAll(this GameObject go, Type type) {
        if (type != null && typeof(MonoBehaviour).IsAssignableFrom(type)) Array.ForEach(go.GetComponents(type), UnityEngine.Object.DestroyImmediate);
    }

    public static bool IsCustomRarity(this Rarity rarity) => !Enum.IsDefined(typeof(Rarity), rarity);

    public static bool TryGet<T>(this List<T> list, int index, out T value) {
        if (list != null && index >= 0 && index < list.Count) {
            value = list[index];
            return true;
        }
        value = default!;
        return false;
    }
    public static void Add<T>(this List<T> list, params T[] items) {
        if (list == null || items == null) return;
        for (int i = 0; i < items.Length; i++) {
            list.Add(items[i]);
        }
    }
    public static void AddRangeDistinct<T>(this List<T> list, IEnumerable<T> items) {
        if (list == null || items == null) return;
        HashSet<T> existingItems = new HashSet<T>(list);

        foreach (var item in items) {
            if (existingItems.Add(item)) list.Add(item);
        }
    }
}