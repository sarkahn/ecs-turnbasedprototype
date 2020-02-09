# Unity ECS Turn Based Prototype

This is a simple example of a turn based game using Unity's ECS. This was developed in Unity 2019.3.06f using Entities v 0.5.1.

This example is based off an [excellent session from Unite Copenhagen 2019](https://www.youtube.com/watch?v=mL4qrt-15TE). While it's a fantastic demo it seemed like the speaker did not yet have a firm grasp on Unity's ECS API (which is extremely fair, it's very new and was changing a lot last year), so I figured I would try to create a more up-to-date example.

![](images/demo.gif)
 
*Players move, then enemies move*

In every aspect of this prototype I tried to follow the "expected" workflow when working with Unity's ECS system (as of this writing and as far as I understand it).

All work takes place in jobs. It uses the conversion system to allow you to modify entity properties and converts them to "pure" entities. There are no gameobjects at runtime (aside from the UI - as of this writing there is no built-in UI for ECS).

------

I've built it to be as simple as possible while still being extensible. The concept is fairly straightforward - you define a `GameTurnSystem<TurnType,ActorType>` like this:

##### [EnemyTurnSystem.cs](Assets/EnemyTurnSystem.cs)
```
    public struct EnemyTurn : IComponentData { }

    public class EnemyTurnSystem : GameTurnSystem<EnemyTurn, Enemy>
    {
        ...
    }

```

`GameTurnSystem` takes two generic parameters - `TurnType` and `ActorType`. As shown above, the first parameter should be a unique tag component used only for the purpose of defining the turn. It's used internally to manage turn order.

The second parameter is the `ActorType` - this gives an easy way to reference the entities you plan to add actions to inside your system. Any entities that are going to act during this turn should have this component.

There's no behaviour happening here, the job of a "game turn" is to hand out actions and call `EndTurn` when all actions are complete. The base class provides some utility functions to make things nice and readable:

##### [EnemyTurnSystem.cs](Assets/EnemyTurnSystem.cs)
```
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

```

The above system assigns an action to all enemies.  In this case we know that enemy actions will only take a single frame, so we immediately end turn in Update.

With the above class defined we can add our game turn to the loop by calling `AddTurn` in `GameTurnManager`. You can call  this from anywhere, but the order matters, so you should call it for all your defined turns in the same place, wherever you decide to do it:

##### [GameTurnManager.cs](Assets/GameTurnManager.cs)
```
    public class GameTurnManager : JobComponentSystem
    {        
        protected override void OnCreate()
        {
            ...

            AddGameTurn<PlayerTurn>();
            AddGameTurn<EnemyTurn>();
        }
    }

```

Notice we're we use the same `EnemyTurn` type used in the `EnemyTurnSystem`. With the turn defined and added to the turn loop, any entities with an `Enemy` component will receive a `TurnAction` component when it's the enemies' turn. In the "acting" system we need to make sure to query for our `TurnAction` components - this gives us the "turn based" behaviour:

##### [EnemyAISystem.cs](Assets/Enemies/EnemyAISystem.cs)
```
    inputDeps = Entities
        .WithAll<Enemy>()
        // Only act when we have a turn
        .WithAll<TurnAction>()
        .ForEach((int entityInQueryIndex, Entity e) =>
        {
            buffer.AddComponent<TryMove>(entityInQueryIndex, e, directions[entityInQueryIndex]);
        }).Schedule(inputDeps);

```

[More coming]

-------

This project was developed for free in my off time, and quite a lot of work went into it. If you find it useful, please consider donating. Any amount you could spare would really help me out a great deal. Thank you!

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=Y54CX7AXFKQXG)