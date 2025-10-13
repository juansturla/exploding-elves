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
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new ElfGameObjectComponent()
            {
                ElfGameObject = authoring.ElfGameObjectPrefab
            });
        }
    }
}
