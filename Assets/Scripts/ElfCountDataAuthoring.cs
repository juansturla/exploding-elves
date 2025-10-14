using Unity.Entities;
using UnityEngine;

public struct ElfCountData : IComponentData
{
    public int RedCount;
    public int GreenCount;
    public int BlueCount;
    public int WhiteCount;
    public int TotalCount;
}
public class ElfCountDataAuthoring : MonoBehaviour
{
    public class ElfCountDataBaker : Baker<ElfCountDataAuthoring>
    {
        public override void Bake(ElfCountDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ElfCountData
            {
                RedCount = 0,
                GreenCount = 0,
                BlueCount = 0,
                WhiteCount = 0,
                TotalCount = 0,
            });
        }
    }
}