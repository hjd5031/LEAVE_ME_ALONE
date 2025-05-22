using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "EnemyMoveBackward", story: "[Target] Moves Backward", category: "Action", id: "069f49dcf845f15d567ccf3a348e5f9c")]
public partial class EnemyMoveBackwardAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    private EnemyCtrl _enemy;

    private class MoveBackHelper : MonoBehaviour {}

    protected override Status OnStart()
    {
        GameObject targetObj = Target?.Value;

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

        // 이동 헬퍼 추가
        var helper = targetObj.GetComponent<MoveBackHelper>();
        if (helper == null)
        {
            helper = targetObj.AddComponent<MoveBackHelper>();
        }

        helper.StartCoroutine(SmoothMoveBack(targetObj.transform, 1f, 2f));

        Debug.Log($"✅ Target '{targetObj.name}'가 부드럽게 뒤로 이동합니다.");
        return Status.Success;
    }

    private IEnumerator SmoothMoveBack(Transform target, float distance, float duration)
    {
        Vector3 start = target.position;
        Vector3 end = start - target.forward * distance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            target.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.position = end;
    }
}