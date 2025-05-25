using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CheckPlayerTomato", story: "Check if PlayerTomato is Ripen", category: "Conditions", id: "76c468585e96b78c67e5c5a455fa57de")]
public partial class CheckPlayerTomatoCondition : Condition
{
    // [SerializeReference] public BlackboardVariable<GameObject> PlayerTomato;
    [SerializeReference] public BlackboardVariable<string> TagValue;

    public override bool IsTrue()
    {
        GameObject[] tagged = GameObject.FindGameObjectsWithTag("RipePlayerTomato");
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
