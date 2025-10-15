using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public partial struct SpawnSystem : ISystem
{
    public const float DEFAULT_TIME_BETWEEN_SPAWNS = 2f;
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // If a SpawnerSettings singleton exists, use it to drive spawn rates
        float timeBetweenSpawns = -1f;
        if (SystemAPI.HasSingleton<SpawnerSettings>())
        {
            timeBetweenSpawns = math.max(0.5f, SystemAPI.GetSingleton<SpawnerSettings>().TimeBetweenSpawns);
        }

        float elfCount = 0;
        if (SystemAPI.HasSingleton<ElfCountData>())
        {
            elfCount = SystemAPI.GetSingleton<ElfCountData>().TotalCount;
        }

        foreach (var (spawner, entity) in SystemAPI.Query<RefRW<Spawner>>().WithEntityAccess())
        {
            if (timeBetweenSpawns > 0f)
            {
                spawner.ValueRW.TimeBetweenSpawns = timeBetweenSpawns;
            }
            spawner.ValueRW.Timer -= SystemAPI.Time.DeltaTime;

            if (spawner.ValueRO.Timer <= 0f && elfCount < NewCollisionSystem.MAX_ENTITIES)
            {
                var targetPosition =
                    LocalTransform.FromPosition(SystemAPI.GetComponent<LocalTransform>(entity).Position);
                ecb.SetComponent(spawner.ValueRO.Prefab, targetPosition);
                spawner.ValueRW.Timer = spawner.ValueRO.TimeBetweenSpawns;
                var newEntity = ecb.Instantiate(spawner.ValueRO.Prefab);
                LifetimeComponent newLifetimeComponent = new LifetimeComponent()
                {
                    Timestamp = SystemAPI.Time.ElapsedTime
                };

                ecb.AddComponent(newEntity, newLifetimeComponent);
                
                // Uncomment if we want to spawn only 1 Elf
                //state.Enabled = false;
            }
        }
        ecb.Playback(state.EntityManager);
    }

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnerSettings>();
        state.RequireForUpdate<ElfCountData>();
    }
    public void OnDestroy(ref SystemState state) { }
}