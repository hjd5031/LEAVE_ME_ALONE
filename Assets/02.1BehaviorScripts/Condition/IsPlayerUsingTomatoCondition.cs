using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Player Using Tomato", story: "Check Player Using [playertomato]", category: "Action", id: "36b4cec664db1b3d3ef02283c4f8aa1d")]
public partial class IsPlayerUsingTomatoCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Playertomato;

    public override bool IsTrue()
    {
        var tomatoScript = Playertomato.Value.GetComponent<PlayerTomatoCtrl>();
        if (tomatoScript.PlayerUsing || !tomatoScript.CompareTag("RipePlayerTomato"))
        {
            SoundManager.Instance.PlaySfx(SoundManager.Sfx.EnemyFrustrated,false, 1f);
            return true;
        }

        SoundManager.Instance.PlaySfx(SoundManager.Sfx.EnemyLaugh3,false,1f);
        return false;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
