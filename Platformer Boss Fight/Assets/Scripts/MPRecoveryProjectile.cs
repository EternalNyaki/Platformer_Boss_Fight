using System.Collections;
using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using UnityEngine;

public class MPRecoveryProjectile : Projectile
{
    protected override void OnHitCharacter(DamagableObject dobj)
    {
        base.OnHitCharacter(dobj);

        BehaviourTreeOwner owner = dobj.gameObject.GetComponent<BehaviourTreeOwner>();
        if (owner != null)
        {
            dobj.GetComponent<Blackboard>().SetVariableValue("mp", dobj.GetComponent<Blackboard>().GetVariableValue<float>("mp") + damage);
        }
    }
}
