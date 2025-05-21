using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CompareTomatoTag", story: "Check number of [value] Tomatoes", category: "Conditions", id: "cf806cdc64dec2cd835fe3650fa11a4a")]
internal class CompareTomatoTagCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<string> Value;

    public override bool IsTrue()
    {
        GameObject[] tagged = GameObject.FindGameObjectsWithTag(Value);
        int randint = UnityEngine.Random.Range(1, 3);
        if (tagged.Length >= randint)
        {
            return true;           
        }
        return false;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
