using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct KillingFloorComponent : IComponentData
{
}
public class KillingFloorAuthoring : MonoBehaviour
{
}

class KillingFloorAuthoringBaker : Baker<KillingFloorAuthoring>
{
    public override void Bake(KillingFloorAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new KillingFloorComponent());
    }
}