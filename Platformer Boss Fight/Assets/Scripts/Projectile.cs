using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    public Vector2 knockback;

    public float moveSpeed;

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    protected virtual void Move()
    {
        transform.position += transform.right * moveSpeed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        DamagableObject dobj = collision.GetComponent<DamagableObject>();

        if (dobj != null)
        {
            OnHitCharacter(dobj);
        }

        DestroySelf();
    }

    protected virtual void OnHitCharacter(DamagableObject dobj)
    {
        if (dobj.gameObject.name == "Player")
        {
            dobj.Hurt(damage, new Vector2(knockback.x * Mathf.Sign(transform.right.x), knockback.y));
        }
    }

    protected virtual void DestroySelf()
    {
        Destroy(gameObject);
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
