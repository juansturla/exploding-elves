using Unity.Entities;
using UnityEngine;

public struct Spawner : IComponentData
{
    public Entity Prefab;
    public float TimeBetweenSpawns;
    public float Timer;
}

public class SpawnerAuthoring : MonoBehaviour
{
    [Tooltip("Elf prefab to be spawned")]
    public GameObject Prefab;

    public class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Spawner
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                TimeBetweenSpawns = SpawnSystem.DEFAULT_TIME_BETWEEN_SPAWNS,
                Timer = SpawnSystem.DEFAULT_TIME_BETWEEN_SPAWNS
            });
        }
    }
}
