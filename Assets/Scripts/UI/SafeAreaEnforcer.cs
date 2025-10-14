using UnityEngine;

public class SafeAreaEnforcer : MonoBehaviour
{
    private RectTransform panelRectTransform;
    private Rect lastSafeArea = Rect.zero;

    void Awake()
    {
        panelRectTransform = GetComponent<RectTransform>();
        RefreshSafeArea();
    }

    void Update()
    {
        // Check if safe area has changed (e.g., device rotation)
        if (Screen.safeArea != lastSafeArea)
        {
            RefreshSafeArea();
        }
    }

    private void RefreshSafeArea()
    {
        lastSafeArea = Screen.safeArea;

        // Convert safe area from screen coordinates to Canvas local coordinates
        Vector2 anchorMin = lastSafeArea.position;
        Vector2 anchorMax = lastSafeArea.position + lastSafeArea.size;

        // Convert to normalized coordinates relative to the screen size
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Apply the normalized safe area to the RectTransform
        panelRectTransform.anchorMin = anchorMin;
        panelRectTransform.anchorMax = anchorMax;
    }
}