using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public struct Spawner : IComponentData
{
    public Entity Prefab;
    public float SpawnRate;
    public float Timer;
}

public class SpawnerAuthoring : MonoBehaviour
{
    [Tooltip("Elf prefab to be spawned")]
    public GameObject Prefab;
    [Tooltip("Time between spawns (in seconds)")]
    [Range(1,10)]
    public float SpawnRate = 1f;

    public class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Spawner
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                SpawnRate = authoring.SpawnRate,
                Timer = authoring.SpawnRate
            });
        }
    }
}

[BurstCompile]
public partial struct SpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // If a SpawnerSettings singleton exists, use it to drive spawn rates
        float settingsSpawnRate = -1f;
        if (SystemAPI.HasSingleton<SpawnerSettings>())
        {
            settingsSpawnRate = math.max(0.01f, SystemAPI.GetSingleton<SpawnerSettings>().SpawnRate);
        }

        foreach (var (spawner, entity) in SystemAPI.Query<RefRW<Spawner>>().WithEntityAccess())
        {
            if (settingsSpawnRate > 0f)
            {
                spawner.ValueRW.SpawnRate = settingsSpawnRate;
            }
            spawner.ValueRW.Timer -= SystemAPI.Time.DeltaTime;

            if (spawner.ValueRO.Timer <= 0f && NewCollisionSystem.EntitiesCount.Data < NewCollisionSystem.MAX_ENTITIES)
            {
                var targetPosition =
                    LocalTransform.FromPosition(SystemAPI.GetComponent<LocalTransform>(entity).Position);
                ecb.SetComponent(spawner.ValueRO.Prefab, targetPosition);
                spawner.ValueRW.Timer = spawner.ValueRO.SpawnRate;
                var newEntity = ecb.Instantiate(spawner.ValueRO.Prefab);
                LifetimeComponent newLifetimeComponent = new LifetimeComponent()
                {
                    Timestamp = SystemAPI.Time.ElapsedTime
                };

                ecb.AddComponent(newEntity, newLifetimeComponent);
                
                // Get elf color from the prefab's ElfComponent
                if (SystemAPI.HasComponent<ElfComponent>(spawner.ValueRO.Prefab))
                {
                    var elfColor = SystemAPI.GetComponent<ElfComponent>(spawner.ValueRO.Prefab).ElfColor;
                    
                    // Send spawn event
                    var spawnEventEntity = ecb.CreateEntity();
                    ecb.AddComponent(spawnEventEntity, new ElfSpawnedEvent { ElfColor = elfColor });
                }
                //state.Enabled = false;
            }
        }
        ecb.Playback(state.EntityManager);
    }

    public void OnCreate(ref SystemState state) { }
    public void OnDestroy(ref SystemState state) { }
}