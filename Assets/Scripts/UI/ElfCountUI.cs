using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using Unity.Mathematics;

public class ElfCountUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text redCountText;
    [SerializeField] private TMP_Text greenCountText;
    [SerializeField] private TMP_Text blueCountText;
    [SerializeField] private TMP_Text whiteCountText;
    [SerializeField] private TMP_Text totalCountText;
    
    [Header("Color Settings")]
    [SerializeField] private Color redColor = Color.red;
    [SerializeField] private Color greenColor = Color.green;
    [SerializeField] private Color blueColor = Color.blue;
    [SerializeField] private Color whiteColor = Color.white;
    
    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.1f; // Update UI every 0.1 seconds
    
    private EntityManager entityManager;
    private Entity countDataEntity;
    private bool hasCountData;
    private float timeSinceLastUpdate;

    private void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        hasCountData = TryGetCountDataEntity(out countDataEntity);
        
        // Set initial colors
        if (redCountText != null) redCountText.color = redColor;
        if (greenCountText != null) greenCountText.color = greenColor;
        if (blueCountText != null) blueCountText.color = blueColor;
        if (whiteCountText != null) whiteCountText.color = whiteColor;
        
        UpdateUI();
    }

    private void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            UpdateUI();
            timeSinceLastUpdate = 0f;
        }
    }

    private bool TryGetCountDataEntity(out Entity entity)
    {
        var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<ElfCountData>());
        if (!query.IsEmpty)
        {
            entity = query.GetSingletonEntity();
            return true;
        }
        entity = Entity.Null;
        return false;
    }

    private void UpdateUI()
    {
        if (!hasCountData)
        {
            hasCountData = TryGetCountDataEntity(out countDataEntity);
            if (!hasCountData) return;
        }

        var countData = entityManager.GetComponentData<ElfCountData>(countDataEntity);
        
        // Update individual color counts
        if (redCountText != null)
            redCountText.text = $"Red: {countData.RedCount}";
            
        if (greenCountText != null)
            greenCountText.text = $"Green: {countData.GreenCount}";
            
        if (blueCountText != null)
            blueCountText.text = $"Blue: {countData.BlueCount}";
            
        if (whiteCountText != null)
            whiteCountText.text = $"White: {countData.WhiteCount}";
        
        // Update total count
        if (totalCountText != null)
        {
            totalCountText.text = $"Total: {countData.TotalCount}";
        }
    }
}
