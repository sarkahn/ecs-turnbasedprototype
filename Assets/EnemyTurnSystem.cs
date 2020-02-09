using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace TurnBasedTutorial.Enemies
{
    public struct EnemyTurn : IComponentData { }

    /// <summary>
    /// A game turn that hands out actions to all enemies at once.
    /// </summary>
    public class EnemyTurnSystem : GameTurnSystem<EnemyTurn, Enemy>
    {
        protected override void OnTurnBegin()
        {
            // Assign an action to all enemies. Not concerned with ordering.
            AssignAction(_actorsWithoutActions);
        }

        protected override JobHandle OnTurnUpdate(JobHandle inputDeps)
        {
            EndTurn();

            return inputDeps;
        }

    }
}