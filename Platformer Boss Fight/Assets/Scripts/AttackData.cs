using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackData
{
    public AttackHitboxWithTiming[] data;
}

[System.Serializable]
public struct AttackFrame
{
    public AttackHitbox[] hitboxes;
}

[System.Serializable]
public struct AttackHitboxWithTiming
{
    public float timing;
    public AttackHitbox hitbox;

    public static implicit operator AttackHitbox(AttackHitboxWithTiming h)
    {
        return h.hitbox;
    }
}

[System.Serializable]
public struct AttackHitbox
{
    public Rect rect;
    public float rotation;
    public int damage;
    public Vector2 knockback;
}