using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "isTheFarmFull", story: "Check the number of isPlantable tag", category: "Conditions", id: "cec9102ab320ba4be74ea740250f965f")]
public partial class IsTheFarmFullCondition : Condition
{

    public override bool IsTrue()
    {
        GameObject[] tagged = GameObject.FindGameObjectsWithTag("EnemyisPlantable");
        if (tagged == null || tagged.Length == 0)
        {
            return true;//Enemies farm is full
        }
        return false;//There is still some space in the farm
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
