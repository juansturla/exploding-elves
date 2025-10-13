using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using Unity.Mathematics;

public class SpawnRateUI : MonoBehaviour
{
    public Button increaseButton;
    public Button decreaseButton;
    public TMP_Text valueLabel;
    public float stepSeconds = 0.5f;
    public float minSeconds = 1f;
    public float maxSeconds = 10f;

    private EntityManager entityManager;
    private Entity settingsEntity;
    private bool hasSettings;

    private void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        hasSettings = TryGetSettingsEntity(out settingsEntity);

        if (increaseButton != null) 
            increaseButton.onClick.AddListener(OnIncrease);
        if (decreaseButton != null) 
            decreaseButton.onClick.AddListener(OnDecrease);
        RefreshLabel();
    }

    private bool TryGetSettingsEntity(out Entity entity)
    {
        var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<SpawnerSettings>());
        if (!query.IsEmpty)
        {
            entity = query.GetSingletonEntity();
            return true;
        }
        entity = Entity.Null;
        return false;
    }

    private float GetCurrent()
    {
        if (!hasSettings)
        {
            hasSettings = TryGetSettingsEntity(out settingsEntity);
            if (!hasSettings) return 1f;
        }
        return entityManager.GetComponentData<SpawnerSettings>(settingsEntity).SpawnRate;
    }

    private void SetCurrent(float value)
    {
        if (!hasSettings)
        {
            hasSettings = TryGetSettingsEntity(out settingsEntity);
            if (!hasSettings) return;
        }
        var data = entityManager.GetComponentData<SpawnerSettings>(settingsEntity);
        data.SpawnRate = math.clamp(value, minSeconds, maxSeconds);
        entityManager.SetComponentData(settingsEntity, data);
        RefreshLabel();
    }

    private void OnIncrease()
    {
        SetCurrent(GetCurrent() + stepSeconds);
    }

    private void OnDecrease()
    {
        SetCurrent(GetCurrent() - stepSeconds);
    }

    private void RefreshLabel()
    {
        if (valueLabel != null)
        {
            valueLabel.text = $"Spawn: {GetCurrent():0.00}s";
        }
    }
}


