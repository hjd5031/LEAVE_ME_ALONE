using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "isPicked", story: "Change EnemyTomato into isPicked", category: "Action", id: "1f395e416a6420eb9718c1df40893573")]
public partial class IsPickedAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> Value;

    private EnemyTomatoCtrl _enemyTomato;   

    protected override Status OnStart()
    {
        GameObject targetObj = Target?.Value;

        if (targetObj == null)
        {
            Debug.LogError("❌ Blackboard에서 전달된 Target 오브젝트가 null입니다.");
            return Status.Failure;
        }

        _enemyTomato = targetObj.GetComponent<EnemyTomatoCtrl>();

        if (_enemyTomato == null)
        {
            Debug.LogError($"❌ Target 오브젝트 '{targetObj.name}'에 TomatoCtrl이 없습니다.");
            return Status.Failure;
        }

        _enemyTomato.isPicked = Value;

        Debug.Log($"✅ Target '{targetObj.name}'의 isPicking 값을 {Value}로 설정했습니다.");
        return Status.Success;
    }
}

