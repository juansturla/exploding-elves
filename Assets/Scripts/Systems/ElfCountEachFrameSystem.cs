using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;


[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(NewCollisionSystem))]
public partial struct ElfCountEachFrameSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<ElfCountData>())
            return;

        var countData = SystemAPI.GetSingletonRW<ElfCountData>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        countData.ValueRW.RedCount = 0;
        countData.ValueRW.GreenCount = 0;
        countData.ValueRW.BlueCount = 0;
        countData.ValueRW.WhiteCount = 0;
        // Process spawn events
        foreach (var (elfComponent, entity) in SystemAPI.Query<RefRO<ElfComponent>>().WithEntityAccess())
        {
            switch (elfComponent.ValueRO.ElfColor)
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
        }

        countData.ValueRW.TotalCount = countData.ValueRO.RedCount
                                       + countData.ValueRO.GreenCount
                                       + countData.ValueRO.WhiteCount
                                       + countData.ValueRO.BlueCount;
                                       
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ElfCountData>();
    }

    public void OnDestroy(ref SystemState state) { }
}
