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
    public float3 DesiredRotation;
    public float Speed;
    public float TimeUntilNextRotation;
    public float TimeBetweenRotations;
    public float MaxRotationAngle; 
}

public class RandomMotionAuthoring : MonoBehaviour
{
    public float3 Range = new float3(1);

    public float TimeBetweenRotations = 1f;
    public float MaxRotationAngle = 45f;
    public float Speed = 2f;
}

class RandomMotionAuthoringBaker : Baker<RandomMotionAuthoring>
{
    public override void Bake(RandomMotionAuthoring authoring)
    {
        var length = math.length(authoring.Range);
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new RandomMotion
        {
            DesiredRotation = float3.zero,
            Speed = authoring.Speed,
            TimeUntilNextRotation = 0,
            TimeBetweenRotations = authoring.TimeBetweenRotations,
            MaxRotationAngle = authoring.MaxRotationAngle,
        });
    }
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
public partial struct RandomMotionSystem : ISystem
{
    private Random random;

    [BurstCompile]
    public partial struct EntityRandomMotionJob : IJobEntity
    {
        public Random Random;
        public float DeltaTime;
        private const float ROTATION_TOLERANCE = 2f;

        public void Execute(ref RandomMotion motion, ref PhysicsVelocity velocity, ref LocalTransform transform, in PhysicsMass mass)
        {
            motion.CurrentTime += DeltaTime;

            motion.TimeUntilNextRotation -= DeltaTime;
            if (motion.TimeUntilNextRotation < 0)
            {
                var deltaYDegrees = Random.NextFloat(-motion.MaxRotationAngle, motion.MaxRotationAngle);
                motion.TimeUntilNextRotation = motion.TimeBetweenRotations;
                motion.DesiredRotation = transform.RotateY(math.radians(deltaYDegrees)).Forward();
            }
            velocity.Linear = transform.Forward() * motion.Speed;

            var angle = Vector3.Angle(transform.Forward(), motion.DesiredRotation);
            if (angle > ROTATION_TOLERANCE)
            {
                var targetRotation = Quaternion.LookRotation(motion.DesiredRotation, transform.Up());
                transform.Rotation = math.slerp(transform.Rotation, targetRotation, DeltaTime * 5);
            }
        }
    }

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RandomMotion>();
        random = new Random();
        random.InitState();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new EntityRandomMotionJob
        {
            Random = random,
            DeltaTime = SystemAPI.Time.DeltaTime,
        }.Schedule(state.Dependency);
    }
}
