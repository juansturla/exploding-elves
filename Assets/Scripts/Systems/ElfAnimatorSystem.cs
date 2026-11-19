using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
public partial struct ElfAnimatorSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (elfGameObjectPrefab, entity) in SystemAPI.Query<ElfGameObjectComponent>()
                     .WithNone<ElfAnimatorReference>()
                     .WithEntityAccess())
        {
            var newGameObject = Object.Instantiate(elfGameObjectPrefab.ElfGameObject);
            var newAnimatorReference = new ElfAnimatorReference()
            {
                ElfAnimator = newGameObject.GetComponent<Animator>()
            };
            ecb.AddComponent(entity, newAnimatorReference);
            // The animated model replaces the placeholder cube
            ecb.AddComponent<DisableRendering>(entity);
        }

        foreach (var (transform, animatorReference) in
                 SystemAPI.Query<LocalTransform, ElfAnimatorReference>())
        {
            animatorReference.ElfAnimator.transform.position = transform.Position;
            animatorReference.ElfAnimator.transform.rotation = transform.Rotation;
        }

        foreach (var (animatorReference, entity) in SystemAPI.Query<ElfAnimatorReference>()
                     .WithNone<ElfGameObjectComponent, LocalTransform>().WithEntityAccess())
        {
            Object.Destroy(animatorReference.ElfAnimator.gameObject);
            ecb.RemoveComponent<ElfAnimatorReference>(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}