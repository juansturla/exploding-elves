using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeScaleUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private TMP_Text valueLabel;

    private readonly float[] cycleSpeeds = new float[] { 1f, 2f, 3f };
    private int currentIndex = 0;

    private void Awake()
    {
        // Initialize index to the closest defined speed to current timescale
        currentIndex = GetClosestIndex(Time.timeScale);
        ApplyCurrentSpeed();

        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(CycleSpeed);
        }
    }

    private void CycleSpeed()
    {
        currentIndex = (currentIndex + 1) % cycleSpeeds.Length;
        ApplyCurrentSpeed();
    }

    private void ApplyCurrentSpeed()
    {
        Time.timeScale = cycleSpeeds[currentIndex];
        if (valueLabel != null)
        {
            valueLabel.text = $"{cycleSpeeds[currentIndex]:0}x";
        }
    }

    private int GetClosestIndex(float value)
    {
        int best = 0;
        float bestDiff = Mathf.Abs(cycleSpeeds[0] - value);
        for (int i = 1; i < cycleSpeeds.Length; i++)
        {
            float d = Mathf.Abs(cycleSpeeds[i] - value);
            if (d < bestDiff)
            {
                best = i;
                bestDiff = d;
            }
        }
        return best;
    }
}


