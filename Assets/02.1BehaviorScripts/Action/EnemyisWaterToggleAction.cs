using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "EnemyisWaterToggle", story: "[Enemy] isWatering [Value]", category: "Action", id: "4432bd71f741ed508271e18069331f11")]
public partial class EnemyisWaterToggleAction : Action
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

        _enemy.isWatering = Value;

        Debug.Log($"✅ Target '{targetObj.name}'의 isWatering 값을 {_enemy.isWatering}로 설정했습니다.");
        return Status.Success;
    }
}

