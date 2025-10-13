using Unity.Entities;
using UnityEngine;

public struct SpawnerSettings : IComponentData
{
    public float SpawnRate; // seconds between spawns
}

public class SpawnerSettingsAuthoring : MonoBehaviour
{
    [Tooltip("Global time between spawns (seconds)")]
    [Range(0.1f, 10f)]
    public float SpawnRate = 1f;
}

public class SpawnerSettingsBaker : Baker<SpawnerSettingsAuthoring>
{
    public override void Bake(SpawnerSettingsAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new SpawnerSettings
        {
            SpawnRate = authoring.SpawnRate
        });
    }
}


