using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ElfComponent : IComponentData
{
    public ElfColors ElfColor;
}
public class ElfComponentAuthoring : MonoBehaviour
{
    public ElfColors ElfColor;
}

class ElfComponentAuthoringBaker : Baker<ElfComponentAuthoring>
{
    public override void Bake(ElfComponentAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new ElfComponent()
        {
            ElfColor = authoring.ElfColor
        });
    }
}

public enum ElfColors
{
    RED,
    GREEN,
    BLUE,
    WHITE
}