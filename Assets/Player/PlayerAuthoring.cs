using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

using TurnBasedTutorial.Movement;

namespace TurnBasedTutorial.Players
{
    public struct Player : IComponentData
    {
        public int number;
    }

    public class PlayerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        // In case of fast enter play mode, see https://blogs.unity3d.com/2019/11/05/enter-play-mode-faster-in-unity-2019-3/
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init() => _counter = 0;

        static int _counter = 0;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            // Auto assign player number
            dstManager.AddComponentData<Player>(entity, new Player { number = _counter++ });
            dstManager.AddComponent<PlayerActor>(entity);
            dstManager.AddComponent<Collidable>(entity);

        }
    } 
}
