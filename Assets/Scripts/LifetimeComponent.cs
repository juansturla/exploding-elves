using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct LifetimeComponent : IComponentData
{
    public double Timestamp;
}