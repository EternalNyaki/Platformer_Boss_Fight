using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damagable : MonoBehaviour
{
    public int maxHealth;

    public int currentHealth { get; protected set; }

    protected Rigidbody2D _rb2d;

    void Start()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        _rb2d = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
    }

    public virtual void Hurt(int damage, Vector2 knockback)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) { Die(); }

        _rb2d.velocity = knockback;
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
