using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeScaleUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider timeScaleSlider;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private TMP_Text valueLabel;

    [Header("Settings")]
    [SerializeField] private float minScale = 0.5f;   // paused
    [SerializeField] private float maxScale = 3.0f;   // 3x speed
    [SerializeField] private float step = 0.5f;

    private void Awake()
    {
        if (timeScaleSlider != null)
        {
            timeScaleSlider.minValue = minScale;
            timeScaleSlider.maxValue = maxScale;
            timeScaleSlider.value = Time.timeScale;
            timeScaleSlider.onValueChanged.AddListener(OnSliderChanged);
        }
        if (increaseButton != null) increaseButton.onClick.AddListener(OnIncrease);
        if (decreaseButton != null) decreaseButton.onClick.AddListener(OnDecrease);
        RefreshLabel();
    }

    private void OnSliderChanged(float value)
    {
        SetTimeScale(value);
    }

    private void OnIncrease()
    {
        var next = Mathf.Clamp(Time.timeScale + step, minScale, maxScale);
        SetTimeScale(next);
        if (timeScaleSlider != null) timeScaleSlider.value = next;
    }

    private void OnDecrease()
    {
        var next = Mathf.Clamp(Time.timeScale - step, minScale, maxScale);
        SetTimeScale(next);
        if (timeScaleSlider != null) timeScaleSlider.value = next;
    }

    private void SetTimeScale(float value)
    {
        Time.timeScale = value;
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (valueLabel != null)
        {
            valueLabel.text = $"Time Scale: {Time.timeScale:0.00}x";
        }
    }
}


