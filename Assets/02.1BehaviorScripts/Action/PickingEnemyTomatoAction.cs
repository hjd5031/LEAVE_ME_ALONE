using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
// using UnityEngine;
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Picking Enemy Tomato", story: "Pick [Target] after [Wait_Time]", category: "Action", id: "2be8bbf98b51b3f688cc453893b966a8")]
public partial class PickingEnemyTomatoAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Wait_Time;
    [CreateProperty] private float m_Timer = 0.0f;
    private GameObject tomato;
    private EnemyTomatoCtrl _enemyTomato;
    private EnemyCtrl enemy;
    protected override Status OnStart()
    {
        m_Timer = Wait_Time;
        tomato = Target?.Value;
        enemy = GameObject.FindFirstObjectByType<EnemyCtrl>();
        _enemyTomato = tomato.GetComponent<EnemyTomatoCtrl>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        m_Timer -= Time.deltaTime;
        if (m_Timer <= 0.0f)
        {
            ChangeTomatoIntoIsPicked();
            
        }

        if (tomato.tag == "PickedEnemyTomato")
        {
            enemy.EnemyScore += 6;
            return Status.Success;
        }
        if(tomato.tag == "EnemyisPlantable")return Status.Success;
        return Status.Running;
        
    }

    void ChangeTomatoIntoIsPicked()
    {
        tomato.tag = "PickedEnemyTomato";
        _enemyTomato.isPicked = true;
    }
    protected override void OnEnd()
    {
    }
}

