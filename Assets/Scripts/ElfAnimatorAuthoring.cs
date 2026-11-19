using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ElfGameObjectComponent : IComponentData
{
    public GameObject ElfGameObject;
}
public class ElfAnimatorReference : ICleanupComponentData
{
    public Animator ElfAnimator;
}

public class ElfAnimatorAuthoring : MonoBehaviour
{
    public GameObject ElfGameObjectPrefab;
    public class ElfGameObjectBaker : Baker<ElfAnimatorAuthoring>
    {
        public override void Bake(ElfAnimatorAuthoring authoring)
        {
            // The model comes from an Asset Store pack that isn't in the repo.
            // Without it the elf keeps its colored cube.
            if (authoring.ElfGameObjectPrefab == null)
                return;

            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new ElfGameObjectComponent()
            {
                ElfGameObject = authoring.ElfGameObjectPrefab
            });
        }
    }
}
