using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "isWatering", story: "changeWateringBool", category: "Action", id: "7f2f5c5ffc75a6cd1a5d4412f7fe4b22")]
public partial class IsWateringAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> Value;

    private TomatoCtrl _tomato;

    protected override Status OnStart()
    {
        GameObject targetObj = Target?.Value;

        if (targetObj == null)
        {
            Debug.LogError("❌ Blackboard에서 전달된 Target 오브젝트가 null입니다.");
            return Status.Failure;
        }

        _tomato = targetObj.GetComponent<TomatoCtrl>();

        if (_tomato == null)
        {
            Debug.LogError($"❌ Target 오브젝트 '{targetObj.name}'에 TomatoCtrl이 없습니다.");
            return Status.Failure;
        }

        _tomato.isWatering = Value;

        Debug.Log($"✅ Target '{targetObj.name}'의 isWatering 값을 {Value}로 설정했습니다.");
        return Status.Success;
    }
}