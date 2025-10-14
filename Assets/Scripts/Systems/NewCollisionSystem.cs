using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct NewCollisionSystem : ISystem
{
    private const int MAX_ENTITIES = 100;
    private static readonly SharedStatic<int> EntitiesCount = SharedStatic<int>.GetOrCreate<NewCollisionSystem>();
    
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
            KillingFloorLookup = SystemAPI.GetComponentLookup<KillingFloorComponent>(true),
            CurrentTimestamp = SystemAPI.Time.ElapsedTime,
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }

    [BurstCompile]
    struct HandleCollisionsJob : ICollisionEventsJob
    {
        [ReadOnly] public PhysicsWorld PhysicsWorld;
        [ReadOnly] public ComponentLookup<ElfComponent> ElvesLookup;
        [ReadOnly] public ComponentLookup<KillingFloorComponent> KillingFloorLookup;
        public EntityCommandBuffer ECB;
        public ComponentLookup<LifetimeComponent> LifetimeLookup;
        public double CurrentTimestamp;

        public void Execute(CollisionEvent collisionEvent)
        {
            var entityA = collisionEvent.EntityA;
            var entityB = collisionEvent.EntityB;
            
            var isElfA = ElvesLookup.HasComponent(entityA);
            var isElfB = ElvesLookup.HasComponent(entityB);
            
            var isKillingFloorA = KillingFloorLookup.HasComponent(entityA);
            var isKillingFloorB = KillingFloorLookup.HasComponent(entityB);

            if (isKillingFloorA && isElfB)
            {
                DestroyElfEntity(entityB);
                return;
            }
            if(isKillingFloorB && isElfA)
            {
                DestroyElfEntity(entityA);
                return;
            }

            if (!isElfA || !isElfB)
                return;

            var details = collisionEvent.CalculateDetails(ref PhysicsWorld);
            if (details.EstimatedImpulse < 1)
                return;

            var lifetimeAComponent = LifetimeLookup[entityA];
            var lifetimeBComponent = LifetimeLookup[entityB];

            // entity cannot collide within the first 2 seconds of it's creation
            var timestampThreshold = CurrentTimestamp - 2;
            if (lifetimeAComponent.Timestamp > timestampThreshold || lifetimeBComponent.Timestamp > timestampThreshold)
            {
                return;
            }
            
            var elfAComponent = ElvesLookup[entityA];
            var elfBComponent = ElvesLookup[entityB];

            if (elfAComponent.ElfColor == elfBComponent.ElfColor)
            {
                
                if (EntitiesCount.Data < MAX_ENTITIES)
                {
                    // Update the timestamps to avoid collisioning after colliding
                    lifetimeAComponent.Timestamp = CurrentTimestamp;
                    lifetimeBComponent.Timestamp = CurrentTimestamp;
                    ECB.SetComponent(entityA, lifetimeAComponent);
                    ECB.SetComponent(entityB, lifetimeBComponent);
                    
                    // Send spawn event for the new elf
                    var spawnEvent = ECB.CreateEntity();
                    ECB.AddComponent(spawnEvent, new ElfSpawnedEvent { ElfColor = elfAComponent.ElfColor });
                    
                    // TODO: Find another way of referencing this entityCount without the static field
                    EntitiesCount.Data++;
                    ECB.Instantiate(entityA);
                }
            } 
            else
            {
                DestroyElfEntity(entityA);
                DestroyElfEntity(entityB);
            }
        }

        private void DestroyElfEntity(Entity entityB)
        {
            var elfBColor = ElvesLookup[entityB].ElfColor;
                
            var destroyEventB = ECB.CreateEntity();
            ECB.AddComponent(destroyEventB, new ElfDestroyedEvent { ElfColor = elfBColor });
                
            // TODO: Find another way of referencing this entityCount without the static field
            EntitiesCount.Data -= 1;
            ECB.DestroyEntity(entityB);
        }
    }
}