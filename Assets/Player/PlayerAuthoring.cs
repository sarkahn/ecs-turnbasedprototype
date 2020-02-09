using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

using TurnBasedTutorial.Movement;
using System;

namespace TurnBasedTutorial.Players
{
    public struct Player : IComponentData, IComparable<Player>
    {
        public int number;

        public int CompareTo(Player other)
        {
            Debug.Log("COMPARING PLAYERS");
            return number.CompareTo(other.number);
        }
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
            dstManager.AddComponent<Collidable>(entity);

#if UNITY_EDITOR
            dstManager.SetName(entity, "Player " + (_counter - 1));
#endif

        }
    } 
}
