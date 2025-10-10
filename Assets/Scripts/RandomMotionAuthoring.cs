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

// This behavior will set a dynamic body's linear velocity to get to randomly selected
// point in space. When the body gets with a specified tolerance of the random position,
// a new random position is chosen and the body starts header there instead.
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
    [BurstCompile]
    public partial struct EntityRandomMotionJob : IJobEntity
    {
        public Random Random;
        public PhysicsStep StepComponent;
        public float DeltaTime;

        public void Execute(ref RandomMotion motion, ref PhysicsVelocity velocity, in LocalTransform transform, in PhysicsMass mass)
        {
            motion.CurrentTime += DeltaTime;

            Random.InitState((uint)(motion.CurrentTime * 1000));

            var desiredOffset = motion.DesiredPosition - transform.Position;
            // If we are close enough to the destination pick a new destination
            if (motion.CurrentTime != DeltaTime)
            {
                Debug.Log("Remaining distance for " + motion.Name + " distance: "
                          + math.lengthsq(transform.Position - motion.DesiredPosition));
            }
            if (motion.CurrentTime == DeltaTime ||
                math.lengthsq(transform.Position - motion.DesiredPosition) < motion.Tolerance)
            {
                var min = new float3(-math.abs(motion.Range));
                var max = new float3(math.abs(motion.Range));
                desiredOffset = Random.NextFloat3(min, max);
                motion.DesiredPosition = desiredOffset + transform.Position;
                Debug.Log("Point reached, changing direction. Current position " + transform.Position 
                    + " Offset: " + desiredOffset);
                velocity.Linear = math.normalize((motion.DesiredPosition - transform.Position)) * 1;
            }
            // Smoothly change the linear velocity
            if (mass.InverseMass != 0)
            {
                velocity.Linear -= StepComponent.Gravity * DeltaTime;
            }
        }
    }

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RandomMotion>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<PhysicsStep>(out var stepComponent))
            stepComponent = PhysicsStep.Default;

        state.Dependency = new EntityRandomMotionJob
        {
            Random = new Random(),
            DeltaTime = SystemAPI.Time.DeltaTime,
            StepComponent = stepComponent
        }.Schedule(state.Dependency);
    }
}
