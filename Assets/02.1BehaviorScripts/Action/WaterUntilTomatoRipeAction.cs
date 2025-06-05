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
    private string PlayerDigSoundID;
    private GameObject player;
    protected override Status OnStart()
    {
        tomato = Target?.Value;
        player = GameObject.FindGameObjectWithTag("Enemy");
        PlayerDigSoundID = SoundManager.Instance.Play3DSfx(SoundManager.Sfx.WaterTomato, player.transform,0.6f);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {

        if (tomato.tag == "EnemyisSunning")
        {
            SoundManager.Instance.StopSfx(PlayerDigSoundID);
            return Status.Success;
        }
        foreach(var tomatoes in GameManager.Instance.enemyTomatoes)
        {
            if (tomatoes.tag == "RipeEnemyTomato")
            {
                SoundManager.Instance.StopSfx(PlayerDigSoundID);
                return Status.Success;
            }
        }
        return Status.Running;
    }
    protected override void OnEnd()
    {
    }
}

