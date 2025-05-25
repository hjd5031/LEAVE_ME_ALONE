using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Random Path for Vehicle", story: "Set random [StartPoint] and [EndPoint] from [StartPoints] and [EndPoints]", category: "Action", id: "0d3064b023f0255f16fe8ba48257cb29")]
public partial class SetRandomPathForVehicleAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> StartPoint;
    [SerializeReference] public BlackboardVariable<GameObject> EndPoint;
    [SerializeReference] public BlackboardVariable<GameObject> StartPoints;
    [SerializeReference] public BlackboardVariable<GameObject> EndPoints;
    private int _randint;
    protected override Status OnStart()
    {
        _randint = UnityEngine.Random.Range(0, 5);
        StartPoint.Value = StartPoints.Value.transform.GetChild(_randint).gameObject;
        EndPoint.Value = EndPoints.Value.transform.GetChild(_randint).gameObject;
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

