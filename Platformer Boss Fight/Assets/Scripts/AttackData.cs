using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackData
{
    public Dictionary<float, AttackHitbox> data;
}

[System.Serializable]
public struct AttackFrame
{
    public AttackHitbox[] hitboxes;
}

[System.Serializable]
public struct AttackHitbox
{
    public Rect rect;
    public float rotation;
    public int damage;
    public Vector2 knockback;
}