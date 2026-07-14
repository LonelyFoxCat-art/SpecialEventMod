using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace VersatileSpecialEventAPI.CustomRarityUtil.Effect;

public class ShinyFrame : MonoBehaviour {
    public Image? image;
    public _2dxFX_Shiny_Reflect? Fx;

    public void Start() {
        image = gameObject.GetComponent<Image>();
        if (image == null) {
            Debug.LogWarning("[ShinyFrame]此组件需要图像组件才能工作.");
            Destroy(this);
        }

        Material material = image!.material;
        Fx = gameObject.AddComponent<_2dxFX_Shiny_Reflect>();
        Fx.AnimationSpeedReduction = 1f;
        PropertyInfo propInfo = typeof(_2dxFX_Shiny_Reflect).GetProperty("defaultMaterial", AccessTools.all);
        propInfo.SetValue(Fx, material);
    }

    public void OnDestroy() {
        Destroy(Fx);
    }
}