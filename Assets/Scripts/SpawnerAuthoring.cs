using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;

public struct Spawner : IComponentData
{
    public Entity Prefab;
    public float SpawnRate;
    public float Timer;
}

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
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
                Timer = authoring.SpawnRate // Initialize timer to spawn immediately
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

        foreach (var (spawner, entity) in SystemAPI.Query<RefRW<Spawner>>().WithEntityAccess())
        {
            spawner.ValueRW.Timer -= SystemAPI.Time.DeltaTime;

            if (spawner.ValueRO.Timer <= 0f)
            {
                ecb.Instantiate(spawner.ValueRO.Prefab);
                ecb.SetComponent(spawner.ValueRO.Prefab, LocalTransform.FromPosition(SystemAPI.GetComponent<LocalTransform>(entity).Position)); // Example: spawn at spawner's position
                spawner.ValueRW.Timer = spawner.ValueRO.SpawnRate;
            }
        }
        ecb.Playback(state.EntityManager);
    }

    public void OnCreate(ref SystemState state) { }
    public void OnDestroy(ref SystemState state) { }
}