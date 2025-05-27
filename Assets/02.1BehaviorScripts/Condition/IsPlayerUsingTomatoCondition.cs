using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Player Using Tomato", story: "Check Player Using [playertomato]", category: "Action", id: "36b4cec664db1b3d3ef02283c4f8aa1d")]
public partial class IsPlayerUsingTomatoCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Playertomato;

    public override bool IsTrue()
    {
        var tomatoScript = Playertomato.Value.GetComponent<PlayerTomatoCtrl>();
        if(tomatoScript.PlayerUsing)
            return true;

        return false;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
