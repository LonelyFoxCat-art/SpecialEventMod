using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace VersatileSpecialEventAPI.CustomRarityUtil.Effect;

public class EmissionFrame : MonoBehaviour {
    public Image? image;
    public _2dxFX_BurningFX? FX;

    public void Start() {
        image = gameObject.GetComponent<Image>();
        if (image == null) {
            Debug.LogWarning("[EmissionFrame]此组件需要图像组件才能工作.");
            Destroy(this);
        }

        Material material = image!.material;
        FX = gameObject.AddComponent<_2dxFX_BurningFX>();
        PropertyInfo propInfo = typeof(_2dxFX_BurningFX).GetProperty("defaultMaterial", AccessTools.all);
        propInfo.SetValue(FX, material);
    }

    public void OnDestroy() {
        Destroy(FX);
    }
}