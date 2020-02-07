using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Stage : IComponentData
{
    public int2 size;
}

public class StageAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    public int2 _size = new int2(20,20);

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Stage { size = _size });
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var s = transform.localScale;
        s.x = _size.x;
        s.y = 1;
        s.z = _size.y;
        transform.localScale = s;
    }
#endif
}
