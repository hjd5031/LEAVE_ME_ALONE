using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Check Tomato destroyed or can water", story: "is [EnemyTomato] can water", category: "Conditions", id: "b60feed8e15457c899451e92b3713ae1")]
public partial class CheckTomatoDestroyedOrCanWaterCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> EnemyTomato;

    public override bool IsTrue()
    {
        GameObject targetObject = EnemyTomato.Value;
        var tomatoScript = targetObject.GetComponent<EnemyTomatoCtrl>();
        if (!tomatoScript.CompareTag("UnripeEnemyTomato")||tomatoScript.PlayerUsing)
        {
            return true;
        }
        return false;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
