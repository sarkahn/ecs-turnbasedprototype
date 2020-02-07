using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace TurnBasedTutorial.Players
{
    /// <summary>
    /// Entities with this and a <see cref="TurnAction"/> component will block the game
    /// from simulating until the <see cref="TurnAction"/> component is removed.
    /// </summary>
    public struct PlayerActor : IComponentData
    {
    }
}