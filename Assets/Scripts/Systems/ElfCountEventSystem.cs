using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;

public struct ElfSpawnedEvent : IComponentData
{
    public ElfColors ElfColor;
}

public struct ElfDestroyedEvent : IComponentData
{
    public ElfColors ElfColor;
}

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(NewCollisionSystem))]
public partial struct ElfCountEventSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<ElfCountData>())
            return;

        var countData = SystemAPI.GetSingletonRW<ElfCountData>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Process spawn events
        foreach (var (spawnEvent, entity) in SystemAPI.Query<RefRO<ElfSpawnedEvent>>().WithEntityAccess())
        {
            switch (spawnEvent.ValueRO.ElfColor)
            {
                case ElfColors.RED:
                    countData.ValueRW.RedCount++;
                    break;
                case ElfColors.GREEN:
                    countData.ValueRW.GreenCount++;
                    break;
                case ElfColors.BLUE:
                    countData.ValueRW.BlueCount++;
                    break;
                case ElfColors.WHITE:
                    countData.ValueRW.WhiteCount++;
                    break;
            }
            ecb.DestroyEntity(entity);
        }

        // Process destroy events
        foreach (var (destroyEvent, entity) in SystemAPI.Query<RefRO<ElfDestroyedEvent>>().WithEntityAccess())
        {
            switch (destroyEvent.ValueRO.ElfColor)
            {
                case ElfColors.RED:
                    countData.ValueRW.RedCount = math.max(0, countData.ValueRW.RedCount - 1);
                    break;
                case ElfColors.GREEN:
                    countData.ValueRW.GreenCount = math.max(0, countData.ValueRW.GreenCount - 1);
                    break;
                case ElfColors.BLUE:
                    countData.ValueRW.BlueCount = math.max(0, countData.ValueRW.BlueCount - 1);
                    break;
                case ElfColors.WHITE:
                    countData.ValueRW.WhiteCount = math.max(0, countData.ValueRW.WhiteCount - 1);
                    break;
            }
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ElfCountData>();
    }

    public void OnDestroy(ref SystemState state) { }
}
