using System.Collections;
using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.UI;

public class DamagableBTCharacter : DamagableObject
{
    public Slider hpBar;
    public Slider mpBar;

    private BehaviourTree _behaviourTree;
    private Blackboard _blackboard;

    protected override void Initialize()
    {
        _behaviourTree = GetComponent<BehaviourTreeOwner>().behaviour;
        _blackboard = GetComponent<Blackboard>();

        base.Initialize();
    }

    void Update()
    {
        hpBar.value = _blackboard.GetVariableValue<int>("hp");
        mpBar.value = _blackboard.GetVariableValue<float>("mp");
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