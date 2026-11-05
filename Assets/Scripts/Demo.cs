using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour {

    /// <summary>
    /// Number of discrete values on the slider
    /// </summary>
    const int LEGEND_VALUES = 6;

    /// <summary>
    /// Minimum count of the slider
    /// </summary>
    const int MIN_COUNT = 1;

    /// <summary>
    /// Maximum count of the slider
    /// </summary>
    const int MAX_COUNT = 100000;

    /// <summary>
    /// Link to slider legend container
    /// </summary>
    [SerializeField]
    RectTransform _legend = null;

    /// <summary>
    /// Slider for the value
    /// </summary>
    [SerializeField]
    Slider _slider = null;

    /// <summary>
    /// Input field for the value
    /// </summary>
    [SerializeField]
    TMP_InputField _input = null;

    /// <summary>
    /// Current count of the slider
    /// </summary>
    int _count = 0;

    /// <summary>
    /// Legend values for the slider
    /// </summary>
    int[] _values = new int[LEGEND_VALUES] { 1, 10, 100, 1000, 10000, 100000 };

    /// <summary>
    /// Called when the script instance is being loaded
    /// </summary>
    void Start() {
        _count = 10;
        _input.SetTextWithoutNotify($"{_count}");
        UpdateSlider();
        ArrangeLegend();
    }

    /// <summary>
    /// Normalize value from one range to another
    /// </summary>
    /// <param name="value">Value in scaled range</param>
    /// <param name="scaleMin">Minimum value of the scale</param>
    /// <param name="scaleMax">Maximum value of the scale</param>
    /// <param name="rangeMin">Minimum value of the range</param>
    /// <param name="rangeMax">Maximum value of the range</param>
    /// <returns>Normalized value</returns>
    int NormalizeRange(float value, float scaleMin, float scaleMax, int rangeMin, int rangeMax) {
        var min = Mathf.Log(rangeMin);
        var max = Mathf.Log(rangeMax);
        var scale = (max - min) / (scaleMax - scaleMin);
        return Convert.ToInt32(Math.Exp(min + scale * (value - min)));
    }

    /// <summary>
    /// Inverse of NormalizeRange: convert a value in the target range back to the scale value
    /// </summary>
    /// <param name="target">Value in the target range</param>
    /// <param name="scaleMin">Minimum value of the scale</param>
    /// <param name="scaleMax">Maximum value of the scale</param>
    /// <param name="rangeMin">Minimum value of the range</param>
    /// <param name="rangeMax">Maximum value of the range</param>
    /// <returns>Scale value (e.g. slider position)</returns>
    float DenormalizeRange(int target, float scaleMin, float scaleMax, int rangeMin, int rangeMax) {
        var min = Mathf.Log(rangeMin);
        var max = Mathf.Log(rangeMax);
        var scale = (max - min) / (scaleMax - scaleMin);
        return min + (Mathf.Log(target) - min) / scale;
    }

    /// <summary>
    /// Update slider value from count
    /// </summary>
    void UpdateSlider() {
        var denormalized = DenormalizeRange(_count, _slider.minValue, _slider.maxValue, MIN_COUNT, MAX_COUNT);
#if CLIENT_DEBUG
        Debug.Log($"count: {_count}, denormalized: {denormalized}");
#endif        
        _slider.SetValueWithoutNotify(denormalized);
    }

    /// <summary>
    /// Arrange legend items by log values
    /// </summary>
    void ArrangeLegend() {
        var width = _legend.rect.width;
#if CLIENT_DEBUG
        Debug.Log($"legend width: {width}, values: {LEGEND_VALUES}");
#endif

        if (LEGEND_VALUES != _values.Length && LEGEND_VALUES != _legend.childCount) {
#if CLIENT_DEBUG
            Debug.LogError($"legend values invalid: {LEGEND_VALUES}, values: {_values.Length}, legend: {_legend.childCount}");
#endif
            return;
        }
        for (var i = 0; i < _legend.childCount; i++) {
            var child = _legend.GetChild(i) as RectTransform;
            var denormalized = DenormalizeRange(_values[i], _slider.minValue, _slider.maxValue, MIN_COUNT, MAX_COUNT);
            var position = width * denormalized - child.rect.width / 2f;
#if CLIENT_DEBUG
            Debug.Log($"legend #{i} position: {position}, denormalized: {denormalized}");
#endif             
            child.anchoredPosition = new Vector2(position, 0f);
        }
    }

    /// <summary>
    /// Callback on slider change, normalize value and set to input field
    /// </summary>
    /// <param name="value">Slider value</param>
    public void OnSliderChange(float value) {
        var normalized = NormalizeRange(value, _slider.minValue, _slider.maxValue, MIN_COUNT, MAX_COUNT);
#if CLIENT_DEBUG
        Debug.Log($"value: {value}, normalized: {normalized}");
#endif
        _count = normalized;
        _input.SetTextWithoutNotify($"{_count}");
    }

    /// <summary> 
    /// Callback on input change, validate and set to cache
    /// </summary> 
    /// <param name="value">Input value</param>
    public void OnInputChange(string value) {
        if (int.TryParse(value, out var count)) {
            if (count >= MIN_COUNT && count <= MAX_COUNT) {
                _count = count;
            } else {
                if (count < MIN_COUNT) {
                    _count = MIN_COUNT;
                } else if (count > MAX_COUNT) {
                    _count = MAX_COUNT;
                }
                _input.text = $"{_count}";
            }
        }
        UpdateSlider();
    }
}
