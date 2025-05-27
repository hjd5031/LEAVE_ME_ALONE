using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CheckPlayerTomato", story: "Check if PlayerTomato is Ripen and player not using", category: "Action", id: "76c468585e96b78c67e5c5a455fa57de")]
public partial class CheckPlayerTomatoCondition : Condition
{
    // [SerializeReference] public BlackboardVariable<GameObject> PlayerTomato;
    [SerializeReference] public BlackboardVariable<string> TagValue;
    private PlayerTomatoCtrl _enemyTomato;   
    public override bool IsTrue()
    {
        GameObject[] tagged = GameObject.FindGameObjectsWithTag("RipePlayerTomato");
        foreach (GameObject go in tagged)
        {
            _enemyTomato = go.GetComponent<PlayerTomatoCtrl>();
            if (_enemyTomato.PlayerUsing == false) return true;
        }
        if (tagged == null || tagged.Length == 0)
        {
            return false;
        }
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
