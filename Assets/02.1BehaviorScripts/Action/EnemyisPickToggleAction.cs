using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "EnemyisPickToggle", story: "[enemy] is pick [Value]", category: "Action", id: "0cbf159953febd01844238f3e40bd169")]
public partial class EnemyisPickToggleAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Enemy;
    [SerializeReference] public BlackboardVariable<bool> Value;

    private EnemyCtrl _enemy;
    protected override Status OnStart()
    {
        GameObject targetObj = Enemy?.Value;

        if (targetObj == null)
        {
            Debug.LogError("❌ Blackboard에서 전달된 Target 오브젝트가 null입니다.");
            return Status.Failure;
        }

        _enemy = targetObj.GetComponent<EnemyCtrl>();

        if (_enemy == null)
        {
            Debug.LogError($"❌ Target 오브젝트 '{targetObj.name}'에 EnemyCtrl 없습니다.");
            return Status.Failure;
        }

        _enemy.isPicked = Value;

        Debug.Log($"✅ Target '{targetObj.name}'의 isPicked 값을 {_enemy.isPicked}로 설정했습니다.");
        return Status.Success;
    }
}

