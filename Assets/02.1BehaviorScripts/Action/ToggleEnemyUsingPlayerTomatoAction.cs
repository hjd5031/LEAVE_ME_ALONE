using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ToggleEnemyUsingPlayerTomato", story: "Enemy Using [playerTomato] to [value]", category: "Action", id: "fee92b4c7abb3185feb65bc80bb3007b")]
public partial class ToggleEnemyUsingPlayerTomatoAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> PlayerTomato;
    [SerializeReference] public BlackboardVariable<bool> Value;

    protected override Status OnStart()
    {
        var tomatoScript = PlayerTomato.Value.GetComponent<PlayerTomatoCtrl>();
        tomatoScript.EnemyUsing = Value;
        return Status.Success;
    }
}

