using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Water until Tomato Ripe", story: "Ripening [Target]", category: "Action", id: "a5ce47d85af49fae68e762f2de062bcd")]
public partial class WaterUntilTomatoRipeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    private GameObject tomato;
    protected override Status OnStart()
    {
        tomato = Target?.Value;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if(tomato.tag == "EnemyisSunning")
            return Status.Success;
        else
        {
            return Status.Running;
        }
    }

    protected override void OnEnd()
    {
    }
}

