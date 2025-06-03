using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CustomRotate", story: "Rotate [Transform] by [Rotation]", category: "Action", id: "9e1385d061e07f088a3f125c6f179041")]
public partial class CustomRotateAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Transform;
    [SerializeReference] public BlackboardVariable<Vector3> Rotation;
    [SerializeReference] public BlackboardVariable<float> Duration;

    [CreateProperty] private float m_Progress;
    [CreateProperty] private Quaternion m_StartRotation;
    private Quaternion m_EndRotation;

    protected override Status OnStart()
    {
        if (Transform.Value == null)
        {
            LogFailure("No Target set.");
            return Status.Failure;
        }

        if (Duration.Value <= 0.0f)
        {
            Transform.Value.rotation = Quaternion.Euler(m_StartRotation.eulerAngles + Rotation.Value);
            return Status.Success;
        }

        m_StartRotation = Transform.Value.rotation;
        m_EndRotation = Quaternion.Euler(m_StartRotation.eulerAngles + Rotation.Value);
        m_Progress = 0.0f;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        foreach(var tomato in GameManager.Instance.enemyTomatoes)
        {
            if(tomato.tag == "RipeEnemyTomato")
                return Status.Success;
        }
        float normalizedProgress = Mathf.Min(m_Progress / Duration.Value, 1f);
        Transform.Value.rotation = Quaternion.Lerp(m_StartRotation, m_EndRotation, normalizedProgress);
        m_Progress += Time.deltaTime;

        return normalizedProgress == 1 ? Status.Success : Status.Running;
    }

    protected override void OnDeserialize()
    {
        // Only target to reduce serialization size.
        m_EndRotation = Quaternion.Euler(m_StartRotation.eulerAngles + Rotation.Value);
    }
}

