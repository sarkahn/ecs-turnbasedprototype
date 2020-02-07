using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public class ArrowAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    float _bounceHeight = 1;

    [SerializeField]
    float _animSpeed = 1;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData<Arrow>(entity, new Arrow
        {
            bounceHeight = _bounceHeight,
            animSpeed = _animSpeed
        });

        // The arrow system requires the arrow to be disabled to find it in it's initial query
        dstManager.AddComponent<Disabled>(entity);
    }
}
