using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Change Player Tomato into isPicked", story: "Change Player Tomato into isPicked", category: "Action", id: "99332588e42d14acc5b43ebdf86fc8c7")]
public partial class ChangePlayerTomatoIntoIsPickedAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> Value;

    private PlayerTomatoCtrl _playerTomato;
    private EnemyCtrl enemy;

    protected override Status OnStart()
    {
        enemy = GameObject.FindFirstObjectByType<EnemyCtrl>();
        GameObject targetObj = Target.Value;

        if (targetObj == null)
        {
            Debug.LogError("❌ Blackboard에서 전달된 Target 오브젝트가 null입니다.");
            return Status.Failure;
        }

        _playerTomato = targetObj.GetComponent<PlayerTomatoCtrl>();

        if (_playerTomato == null)
        {
            Debug.LogError($"❌ Target 오브젝트 '{targetObj.name}'에 TomatoCtrlPlayer이 없습니다.");
            return Status.Failure;
        }

        if (_playerTomato.CompareTag("RipePlayerTomato"))
        {
            enemy.EnemyScore += 4;
            _playerTomato.isPicked = Value;
            Debug.Log($"✅ Target '{targetObj.name}'의 isPicked 값을 {Value}로 설정했습니다.");    
            // return Status.Success;
        }
        return Status.Success;
    }
}

