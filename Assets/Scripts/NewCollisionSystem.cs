using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct NewCollisionSystem : ISystem
{
    private const int MAX_ENTITIES = 100;
    public static int entitiesCount = 0;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingletonRW<EndInitializationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);

        state.Dependency = new HandleCollisionsJob
        {
            ECB = ecb,
            PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
            ElvesLookup = SystemAPI.GetComponentLookup<ElfComponent>(true),
            LifetimeLookup = SystemAPI.GetComponentLookup<LifetimeComponent>(false),
            currentTimestamp = SystemAPI.Time.ElapsedTime,
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }

    [BurstCompile]
    struct HandleCollisionsJob : ICollisionEventsJob
    {
        public EntityCommandBuffer ECB;
        [ReadOnly] public PhysicsWorld PhysicsWorld;
        [ReadOnly] public ComponentLookup<ElfComponent> ElvesLookup;
        public ComponentLookup<LifetimeComponent> LifetimeLookup;
        public double currentTimestamp;

        public void Execute(CollisionEvent collisionEvent)
        {
            var isElfA = ElvesLookup.HasComponent(collisionEvent.EntityA);
            var isElfB = ElvesLookup.HasComponent(collisionEvent.EntityB);
            if (!isElfA || !isElfB)
                return;

            var details = collisionEvent.CalculateDetails(ref PhysicsWorld);
            if (details.EstimatedImpulse < 1)
            {
                return;
            }

            var elfAEntity = collisionEvent.EntityA;
            var elfBEntity = collisionEvent.EntityB;

            var lifetimeAComponent = LifetimeLookup[elfAEntity];
            var lifetimeBComponent = LifetimeLookup[elfBEntity];

            // entity cannot collide within the first 2 seconds of it's creation
            var timestampThreshold = currentTimestamp - 2;
            if (lifetimeAComponent.Timestamp > timestampThreshold || lifetimeBComponent.Timestamp > timestampThreshold)
            {
                return;
            }
            
            var elfAComponent = ElvesLookup[elfAEntity];
            var elfBComponent = ElvesLookup[elfBEntity];

            if (elfAComponent.ElfColor == elfBComponent.ElfColor)
            {
                
                if (entitiesCount < MAX_ENTITIES)
                {
                    // Update the timestamps to avoid collisioning after colliding
                    lifetimeAComponent.Timestamp = currentTimestamp;
                    lifetimeBComponent.Timestamp = currentTimestamp;
                    entitiesCount++;
                    ECB.Instantiate(elfAEntity);
                }
            } 
            else
            {
                entitiesCount -= 2;
                ECB.DestroyEntity(elfAEntity);
                ECB.DestroyEntity(elfBEntity);
            }
        }
    }
}