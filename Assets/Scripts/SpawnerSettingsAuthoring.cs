using Unity.Entities;
using UnityEngine;

public struct SpawnerSettings : IComponentData
{
    public float TimeBetweenSpawns; // seconds between spawns
}

public class SpawnerSettingsAuthoring : MonoBehaviour
{
}

public class SpawnerSettingsBaker : Baker<SpawnerSettingsAuthoring>
{
    public override void Bake(SpawnerSettingsAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new SpawnerSettings
        {
            TimeBetweenSpawns = SpawnSystem.DEFAULT_TIME_BETWEEN_SPAWNS
        });
    }
}


