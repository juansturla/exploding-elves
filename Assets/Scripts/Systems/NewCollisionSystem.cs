using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct NewCollisionSystem : ISystem
{
    public const int MAX_ENTITIES = 100;
    public static readonly SharedStatic<int> EntitiesCount = SharedStatic<int>.GetOrCreate<NewCollisionSystem>();
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);
        var processedSpawns = new NativeParallelHashSet<Entity>(1024, Allocator.TempJob);
        var processedDestroys = new NativeParallelHashSet<Entity>(1024, Allocator.TempJob);

        state.Dependency = new HandleCollisionsJob
        {
            ECB = ecb,
            PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
            ElvesLookup = SystemAPI.GetComponentLookup<ElfComponent>(true),
            LifetimeLookup = SystemAPI.GetComponentLookup<LifetimeComponent>(false),
            KillingFloorLookup = SystemAPI.GetComponentLookup<KillingFloorComponent>(true),
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(false),
            RandomMotionLookup = SystemAPI.GetComponentLookup<RandomMotion>(false),
            CurrentTimestamp = SystemAPI.Time.ElapsedTime,
            ProcessedSpawns = processedSpawns,
            ProcessedDestroys = processedDestroys,
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

        state.Dependency.Complete();
        processedSpawns.Dispose();
        processedDestroys.Dispose();
    }

    [BurstCompile]
    struct HandleCollisionsJob : ICollisionEventsJob
    {
        [ReadOnly] public PhysicsWorld PhysicsWorld;
        [ReadOnly] public ComponentLookup<ElfComponent> ElvesLookup;
        [ReadOnly] public ComponentLookup<KillingFloorComponent> KillingFloorLookup;
        public ComponentLookup<LocalTransform> TransformLookup;
        public ComponentLookup<RandomMotion> RandomMotionLookup;
        public EntityCommandBuffer ECB;
        public ComponentLookup<LifetimeComponent> LifetimeLookup;
        public double CurrentTimestamp;
        public NativeParallelHashSet<Entity> ProcessedSpawns;
        public NativeParallelHashSet<Entity> ProcessedDestroys;

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
                MarkElfForDestroy(entityB);
                return;
            }
            if(isKillingFloorB && isElfA)
            {
                MarkElfForDestroy(entityA);
                return;
            }

            if (!isElfA || !isElfB)
                return;

            var details = collisionEvent.CalculateDetails(ref PhysicsWorld);
            if (details.EstimatedImpulse < 1)
                return;

            var lifetimeAComponent = LifetimeLookup[entityA];
            var lifetimeBComponent = LifetimeLookup[entityB];

            // entity cannot collide within the first 0.5 seconds of it's creation
            var timestampThreshold = CurrentTimestamp - 0.5f;
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
                    // Avoid setting components if either is already marked for destroy
                    if (!ProcessedDestroys.Contains(entityA))
                        ECB.SetComponent(entityA, lifetimeAComponent);
                    if (!ProcessedDestroys.Contains(entityB))
                        ECB.SetComponent(entityB, lifetimeBComponent);
                    
                    // Rotate transforms to look away from each other; update RandomMotion.DesiredRotation
                    var transformRotated = new LocalTransform();
                    if (TransformLookup.HasComponent(entityA))
                    {
                        transformRotated = TransformLookup[entityA];
                    }
                    if (TransformLookup.HasComponent(entityB))
                    {
                        transformRotated = transformRotated.RotateY(math.radians(120));
                        var tB = TransformLookup[entityB];
                        tB.Rotation = transformRotated.Rotation;
                        TransformLookup[entityB] = tB;
                        if (RandomMotionLookup.HasComponent(entityB))
                        {
                            var rmB = RandomMotionLookup[entityB];
                            rmB.DesiredRotation = transformRotated.Forward();
                            RandomMotionLookup[entityB] = rmB;
                        }
                    }

                    // Deduplicate per-frame spawn from this source entity
                    if (ProcessedSpawns.Add(entityA))
                    {
                        // Send spawn event for the new elf
                        var spawnEvent = ECB.CreateEntity();
                        ECB.AddComponent(spawnEvent, new ElfSpawnedEvent { ElfColor = elfAComponent.ElfColor });
                        
                        // TODO: Find another way of referencing this entityCount without the static field
                        EntitiesCount.Data++;
                        var newEntity = ECB.Instantiate(entityA);
                        
                        // Set transform and desired rotation via ECB (deferred entity)
                        ECB.SetComponent(newEntity, transformRotated);
                        var rmN = RandomMotionLookup[entityA];
                        rmN.DesiredRotation = transformRotated.Forward();
                        ECB.SetComponent(newEntity, rmN);
                    }
                }
            } 
            else
            {
                MarkElfForDestroy(entityA);
                MarkElfForDestroy(entityB);
            }
        }

        private void MarkElfForDestroy(Entity entity)
        {
            // If entity was already destroyed earlier in the frame, skip
            if (!ElvesLookup.HasComponent(entity))
            {
                return;
            }
            if (!ProcessedDestroys.Add(entity))
            {
                return;
            }
            var elfBColor = ElvesLookup[entity].ElfColor;
                
            var destroyEvent = ECB.CreateEntity();
            ECB.AddComponent(destroyEvent, new ElfDestroyedEvent { ElfColor = elfBColor });
                
            // TODO: Find another way of referencing this entityCount without the static field
            EntitiesCount.Data -= 1;
            // Mark for destruction; actual destruction happens in cleanup system
            ECB.AddComponent<PendingDestroy>(entity);
        }
    }
}