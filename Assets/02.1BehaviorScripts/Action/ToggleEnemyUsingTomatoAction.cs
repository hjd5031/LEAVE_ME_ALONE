using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ToggleEnemyUsingEnemyTomato", story: "Enemy Using [EnemyTomato] to [value]", category: "Action", id: "48755d4d732254d76f98acf0e521752f")]
public partial class ToggleEnemyUsingTomatoAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> EnemyTomato;
    [SerializeReference] public BlackboardVariable<bool> value;

    protected override Status OnStart()
    {
        var tomatoScript = EnemyTomato.Value.GetComponent<EnemyTomatoCtrl>();
        tomatoScript.EnemyUsing = value;
        return Status.Success;
    }

    // protected override Status OnUpdate()
    // {
    //     return Status.Success;
    // }
    //
    // protected override void OnEnd()
    // {
    // }
}

