using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace TurnBasedTutorial.Enemies
{
    /// <summary>
    /// Entities with this comoponent will have the opportunity to act after the player has taken their turn.
    /// </summary>
    public struct AIActor : IComponentData
    {
    }
}

