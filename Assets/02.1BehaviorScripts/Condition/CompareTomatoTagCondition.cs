using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CompareTomatoTag", story: "Check number of [value] Tomatoes", category: "Conditions", id: "cf806cdc64dec2cd835fe3650fa11a4a")]
internal class CompareTomatoTagCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<string> Value;
    private int usableTomatoCount = 0;
    private EnemyTomatoCtrl _enemyTomato;
    public override bool IsTrue()
    {
        usableTomatoCount = 0;
        GameObject[] tagged = GameObject.FindGameObjectsWithTag(Value);

        foreach (GameObject go in tagged)
        {
            _enemyTomato = go.GetComponent<EnemyTomatoCtrl>();
            if(!_enemyTomato.playerUsing)
                usableTomatoCount++;
        }
        // int randint = UnityEngine.Random.Range(1, 3);
        if (usableTomatoCount >= 1)
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
