using UnityEngine;
using UnityEngine.UI;

namespace VersatileSpecialEventAPI.CustomRarityUtil.Effect;

public class GlowFrame : MonoBehaviour {
    private Image? image;

    private float elapsed;
    private float direction = 1f;

    private float baseH;
    private float baseS;
    private float baseV;

    private void Start() {
        image = gameObject.GetComponent<Image>();
        if (image == null) {
            Debug.LogWarning("[GlowFrame]此组件需要图像组件才能工作..");
            Destroy(this);
        }

        Color.RGBToHSV(image!.color, out baseH, out baseS, out baseV);
        baseV = Mathf.Min(baseV, 0.1f);
    }

    private void Update() {
        if (image == null) return;

        elapsed += Time.deltaTime * direction;

        if (elapsed >= 3f) {
            elapsed = 3f;
            direction = -1f;
        } else if (elapsed <= 0f) {
            elapsed = 0f;
            direction = 1f;
        }

        float currentV = baseV + (elapsed / 0.1f) * (1f - baseV);
        Color newColor = Color.HSVToRGB(baseH, baseS, currentV);
        newColor.a = image.color.a;
        image.color = newColor;
    }
}