using Unity.Entities;
using UnityEngine;

public struct SpawnerSettings : IComponentData
{
    public float TimeBetweenSpawns; // seconds between spawns
}

public class SpawnerSettingsAuthoring : MonoBehaviour
{
    [Tooltip("Global time between spawns (seconds)")]
    [Range(0.5f, 10f)]
    public float TimeBetweenSpawns = 1f;
}

public class SpawnerSettingsBaker : Baker<SpawnerSettingsAuthoring>
{
    public override void Bake(SpawnerSettingsAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new SpawnerSettings
        {
            TimeBetweenSpawns = authoring.TimeBetweenSpawns
        });
    }
}


