using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

using TurnBasedTutorial.Movement;

namespace TurnBasedTutorial.Enemies
{
    public struct Enemy : IComponentData
    { }

    public class EnemyAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Enemy>(entity);
            dstManager.AddComponent<Collidable>(entity);
        }
    } 
}
