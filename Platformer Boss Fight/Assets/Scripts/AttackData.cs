using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackData
{
    public AttackHitbox[] data;
}

[System.Serializable]
public struct AttackHitbox
{
    public float timing;
    public Rect hitboxRect;
    public float hitboxRotation;
}