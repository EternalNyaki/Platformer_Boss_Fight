using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using FacingDirection = PlatformerCharacterController.FacingDirection;

public class PlatformerCharacterVisuals : MonoBehaviour
{
    public Slider healthBar;

    private Damagable _damagable;
    private PlatformerCharacterController _characterController;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private int xMovementHash, yMovementHash, groundedHash, attackHash, hitHash, healthHash;

    void Start()
    {
        //Get component references
        _damagable = GetComponent<Damagable>();
        _characterController = GetComponent<PlatformerCharacterController>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        //Preload animator hashes
        xMovementHash = Animator.StringToHash("xMovement");
        yMovementHash = Animator.StringToHash("yMovement");
        groundedHash = Animator.StringToHash("grounded");
        attackHash = Animator.StringToHash("attack");
        hitHash = Animator.StringToHash("hit");
        healthHash = Animator.StringToHash("health");

        _characterController.attackEvent += TriggerAttack;
    }

    // Update is called once per frame
    void Update()
    {
        AnimUpdate();
    }

    private void AnimUpdate()
    {
        _animator.SetFloat(xMovementHash, Mathf.Abs(_characterController.GetVelocity().x));
        _animator.SetFloat(yMovementHash, _characterController.GetVelocity().y);
        _animator.SetBool(groundedHash, _characterController.IsGrounded());

        _animator.SetInteger(healthHash, _damagable.currentHealth);
        healthBar.value = _damagable.currentHealth;

        //Flip the player sprite based on facing direction
        switch (_characterController.GetFacingDirection())
        {
            case FacingDirection.left:
                _spriteRenderer.flipX = true;
                break;

            case FacingDirection.right:
            default:
                _spriteRenderer.flipX = false;
                break;
        }
    }

    public void TriggerAttack(object sender, System.EventArgs e)
    {
        _animator.SetTrigger(attackHash);
    }
}