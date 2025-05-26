using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Check Tomato Destroyed", story: "Is [Target] Destroyed?", category: "Conditions", id: "a085b86f2a423d052b9b7110995637e5")]
public partial class CheckTomatoDestroyedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    public override bool IsTrue()
    {
        GameObject targetObject = Target.Value;
        if (targetObject.tag == "EnemyisPlantable")
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
