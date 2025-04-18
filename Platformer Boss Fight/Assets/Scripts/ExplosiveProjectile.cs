using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveProjectile : Projectile
{
    public float explosionTimer = 5f;

    private bool _exploding = false;

    private int _explodeHash;
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();

        _explodeHash = Animator.StringToHash("explode");
    }

    protected override void Move()
    {
        if (explosionTimer > 0f)
        {
            base.Move();
        }
        else if (!_exploding)
        {
            _exploding = true;
            StartCoroutine(Explode());
        }

        if (explosionTimer <= -5f)
        {
            Destroy(gameObject);
        }

        explosionTimer -= Time.deltaTime;
    }

    protected virtual IEnumerator Explode()
    {
        _animator.SetTrigger(_explodeHash);

        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName("Exit"));

        Destroy(gameObject);
    }
}
