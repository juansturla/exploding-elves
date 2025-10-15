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
    [Tooltip("Time between spawns (in seconds). Can be changed in the SpawnRateGroup GameObject or in the player UI")]
    private readonly float timeBetweenSpawns = 1f;

    public class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Spawner
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                TimeBetweenSpawns = authoring.timeBetweenSpawns,
                Timer = authoring.timeBetweenSpawns
            });
        }
    }
}
