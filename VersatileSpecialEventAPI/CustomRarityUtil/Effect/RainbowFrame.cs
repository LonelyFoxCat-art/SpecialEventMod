using UnityEngine;
using UnityEngine.UI;

namespace VersatileSpecialEventAPI.CustomRarityUtil.Effect;

public class RainbowFrame : MonoBehaviour {
    private Image? _image;
    private float _elapsed;

    private void Start() {
        _image = GetComponent<Image>();
        _elapsed = RandomUtil.RangeFloat(0f, 3f);

        if (_image == null) {
            Debug.LogWarning("[RainbowFrame] The Image component is required for this component to work.");
            Destroy(this);
            return;
        }
    }

    private void Update() {
        _elapsed += Time.deltaTime;

        if (_elapsed > 3f) _elapsed = 0f;

        Color.RGBToHSV(_image!.color, out float h, out float s, out float v);
        Color newColor = Color.HSVToRGB(_elapsed / 3f, s, v);
        newColor.a = _image.color.a;

        _image.color = newColor;
    }
}