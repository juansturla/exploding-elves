using Unity.Burst;
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
    public float TimeBetweenRotations = 1f;
    public float MaxRotationAngle = 60f;
    public float Speed = 2f;
}

class RandomMotionAuthoringBaker : Baker<RandomMotionAuthoring>
{
    public override void Bake(RandomMotionAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new RandomMotion
        {
            DesiredRotation = float3.zero,
            Speed = authoring.Speed,
            TimeUntilNextRotation = 0,
            TimeBetweenRotations = authoring.TimeBetweenRotations,
            MaxRotationAngle = authoring.MaxRotationAngle,
            CurrentTime = 0,
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
            if (motion.CurrentTime == 0)
            {
                var deltaYDegrees = Random.NextFloat(-180, 180);
                transform = transform.RotateY(deltaYDegrees);
            }
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
            motion.CurrentTime += DeltaTime;
        }
    }

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RandomMotion>();
        random = new Random();
        random.InitState((uint) System.DateTime.Now.Ticks);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        state.Dependency = new EntityRandomMotionJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            Random = random,
        }.Schedule(state.Dependency);
    }
}
