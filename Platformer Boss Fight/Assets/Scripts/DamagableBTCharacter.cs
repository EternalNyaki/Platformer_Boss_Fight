using System.Collections;
using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using UnityEngine;

public class DamagableBTCharacter : DamagableObject
{
    private BehaviourTree _behaviourTree;

    protected override void Initialize()
    {
        _behaviourTree = GetComponent<BehaviourTreeOwner>().behaviour;

        base.Initialize();
    }

    public override void Hurt(int damage, Vector2 knockback)
    {
        BTCharacterHurtEventArgs eventArgs = new BTCharacterHurtEventArgs
        {
            damage = damage,
            knockback = knockback
        };

        _behaviourTree.SendEvent("Hurt", eventArgs, this);
    }
}

[System.Serializable]
public struct BTCharacterHurtEventArgs
{
    public int damage;
    public Vector2 knockback;
}