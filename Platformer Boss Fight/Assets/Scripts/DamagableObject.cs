using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagableObject : MonoBehaviour
{
    public int maxHealth;

    protected int _health;

    protected Rigidbody2D _rb2d;

    void Start()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        _rb2d = GetComponent<Rigidbody2D>();

        _health = maxHealth;
    }

    public virtual void Hurt(int damage, Vector2 knockback)
    {
        _health -= damage;
        if (_health <= 0) { Die(); }

        _rb2d.velocity = knockback;
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
