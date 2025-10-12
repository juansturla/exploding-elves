using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public struct RandomMotion : IComponentData
{
    public float CurrentTime;
    public float3 InitialPosition;
    public float3 DesiredPosition;
    public float Speed;
    public float Tolerance;
    public float3 Range;
    public FixedString64Bytes Name;
}

public class RandomMotionAuthoring : MonoBehaviour
{
    public float3 Range = new float3(1);
}

class RandomMotionAuthoringBaker : Baker<RandomMotionAuthoring>
{
    public override void Bake(RandomMotionAuthoring authoring)
    {
        var length = math.length(authoring.Range);
        var transform = GetComponent<Transform>();
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new RandomMotion
        {
            InitialPosition = transform.position,
            DesiredPosition = float3.zero,
            Speed = length * 0.01f,
            Tolerance = 0.5f,
            Range = authoring.Range,
            Name = this.GetName()
        });
    }
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
public partial struct RandomMotionSystem : ISystem
{
    public Random Random;

    [BurstCompile]
    public partial struct EntityRandomMotionJob : IJobEntity
    {
        public Random Random;
        public float DeltaTime;

        public void Execute(ref RandomMotion motion, ref PhysicsVelocity velocity, in LocalTransform transform, in PhysicsMass mass)
        {
            motion.CurrentTime += DeltaTime;

            // If we are close enough to the destination pick a new destination
            
            if (motion.CurrentTime == DeltaTime ||
                math.lengthsq(transform.Position - motion.DesiredPosition) < motion.Tolerance)
            {
                var min = new float3(-math.abs(motion.Range));
                var max = new float3(math.abs(motion.Range));
                var desiredOffset = Random.NextFloat3(min, max);
                motion.DesiredPosition = desiredOffset + transform.Position;
                velocity.Linear = math.normalize((motion.DesiredPosition - transform.Position)) * 5;
            }
        }
    }

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RandomMotion>();
        Random = new Random();
        Random.InitState();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new EntityRandomMotionJob
        {
            Random = Random,
            DeltaTime = SystemAPI.Time.DeltaTime,
        }.Schedule(state.Dependency);
    }
}
