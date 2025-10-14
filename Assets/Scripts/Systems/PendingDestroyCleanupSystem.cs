using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics.Systems;

public struct PendingDestroy : IComponentData
{
    public ElfColors ElfColor;
}

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(NewCollisionSystem))]
public partial struct PendingDestroyCleanupSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (tag, entity) in SystemAPI.Query<RefRO<PendingDestroy>>().WithEntityAccess())
        {

            ecb.DestroyEntity(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}


