using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UI;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerController : Damagable
{
    /// <summary>
    /// Describes the direction the player is facing
    /// </summary>
    public enum FacingDirection
    {
        left = -1, right = 1
    }

    public bool disableControls = false;

    public PlatformerControllerParams movementParams;

#if UNITY_EDITOR
    public Sprite[] attack1Sprites;
    public Sprite[] attack2Sprites;
#endif

    public AttackFrame[] attack1Hitboxes;
    public AttackFrame[] attack2Hitboxes;
    public LayerMask enemyMask;

    public Slider healthBar;

    //Vector for storing directional input
    private Vector2 _playerInput;
    //The direction the player is facing
    private FacingDirection _direction = FacingDirection.right;

    //Booleans from storing player input between Update() and FixedUpdate()
    private bool _jumpTrigger, _jumpReleaseTrigger;
    private bool _attackInput;

    //Amount of time since the player was last grounded
    //Used for coyote time
    private float _timeSinceLastGrounded = 0;

    //The player's previous falling speed
    //Used to calculate camera shake intensity when landing
    private float _prevFallingSpeed = 0f;

    private Coroutine _motionOverrideCoroutine = null;
    private bool _endOverrideCoroutineTrigger = false;

    private List<GameObject> attackHits;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private int xMovementHash, yMovementHash, groundedHash, attackHash, hitHash, healthHash;

    protected override void Initialize()
    {
        base.Initialize();

        //Get component references
        _rb2d = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        xMovementHash = Animator.StringToHash("xMovement");
        yMovementHash = Animator.StringToHash("yMovement");
        groundedHash = Animator.StringToHash("grounded");
        attackHash = Animator.StringToHash("attack");
        hitHash = Animator.StringToHash("hit");
        healthHash = Animator.StringToHash("health");

        attackHits = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!disableControls) { GetInputs(); }

        //Set previous falling speed
        if (_rb2d.velocity.y < 0)
        {
            _prevFallingSpeed = _rb2d.velocity.y;
        }

        //Increment coyote time timer
        if (!IsGrounded())
        {
            _timeSinceLastGrounded += Time.deltaTime;
        }
        else
        {
            _timeSinceLastGrounded = 0;
        }

        healthBar.value = currentHealth;

        AnimUpdate();
    }

    private void GetInputs()
    {
        //Get directional input
        _playerInput.x = Input.GetAxisRaw("Horizontal");
        _playerInput.y = Input.GetAxisRaw("Vertical");
        _playerInput.Normalize();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C))
        {
            _jumpTrigger = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C))
        {
            _jumpReleaseTrigger = true;
        }

        _attackInput = (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.X)) && IsGrounded();
    }

    private void AnimUpdate()
    {
        _animator.SetFloat(xMovementHash, Mathf.Abs(_rb2d.velocity.x));
        _animator.SetFloat(yMovementHash, _rb2d.velocity.y);
        _animator.SetBool(groundedHash, IsGrounded());
        if (_attackInput) { _animator.SetTrigger(attackHash); }
        _animator.SetInteger(healthHash, currentHealth);

        //Flip the player sprite based on facing direction
        switch (GetFacingDirection())
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

    void FixedUpdate()
    {
        MovementUpdate(_playerInput);

        //Reset input triggers
        _jumpTrigger = false;
        _jumpReleaseTrigger = false;
    }

    //Update movement and apply physics (called in FixedUpdate())
    private void MovementUpdate(Vector2 playerInput)
    {
        if (_motionOverrideCoroutine != null) { return; }

        Vector2 velocity = _rb2d.velocity;

        //Calculate movement
        HorizontalMovement(playerInput.x, ref velocity.x);
        VerticalMovement(playerInput.y, ref velocity.y);

        //Apply movement
        _rb2d.velocity = velocity;

        ApplyGravity();
    }

    //Calculate horizontal movement (horizontal input)
    private void HorizontalMovement(float horizontalInput, ref float xVelocity)
    {
        if (horizontalInput == 0)
        {
            if (xVelocity < movementParams.minMovementTolerance && xVelocity > -movementParams.minMovementTolerance)
            {
                //Do nothing
                xVelocity = 0f;
            }
            else
            {
                //Decelerate
                xVelocity = Mathf.Clamp(-Mathf.Sign(xVelocity) * movementParams.acceleration * Time.deltaTime, -movementParams.maxSpeed, movementParams.maxSpeed);
            }
        }
        else
        {
            //Accelerate
            xVelocity = Mathf.Clamp(xVelocity + horizontalInput * movementParams.acceleration * Time.deltaTime, -movementParams.maxSpeed, movementParams.maxSpeed);

            //Change facing direction
            if (horizontalInput > 0)
            {
                _direction = FacingDirection.right;
            }
            else if (horizontalInput < 0)
            {
                _direction = FacingDirection.left;
            }
        }
    }

    //Calculate vertical movement (gravity and jumping)
    private void VerticalMovement(float verticalInput, ref float yVelocity)
    {
        if (_jumpTrigger && (IsGrounded() || _timeSinceLastGrounded < movementParams.coyoteTime))
        {
            //Jump
            Jump(ref yVelocity);
        }
        if (_jumpReleaseTrigger && yVelocity > 0f)
        {
            //Shorten jump (for dynamic jump height)
            yVelocity /= 2;
        }
    }

    private void Jump(ref float yVelocity)
    {
        yVelocity = movementParams.jumpVelocity;
    }

    private void ApplyGravity()
    {
        //Calculate gravity
        _rb2d.velocity = new Vector2(_rb2d.velocity.x, Mathf.Clamp(_rb2d.velocity.y + movementParams.gravity * Time.deltaTime, -movementParams.terminalVelocity, float.PositiveInfinity));
    }

    public void EndOverrideAction()
    {
        _endOverrideCoroutineTrigger = true;
        _motionOverrideCoroutine = null;
    }

    private bool GetEndOverrideCoroutine()
    {
        bool output = _endOverrideCoroutineTrigger;
        _endOverrideCoroutineTrigger = false;
        return output;
    }

    private IEnumerator AttackRoutine()
    {
        _rb2d.velocity = new Vector2(0f, _rb2d.velocity.y);
        while (!GetEndOverrideCoroutine())
        {
            ApplyGravity();
            yield return null;
        }

        attackHits.Clear();
    }

    private IEnumerator HurtRoutine()
    {
        while (!GetEndOverrideCoroutine())
        {
            ApplyGravity();
            yield return null;
        }
    }

    private IEnumerator DieRoutine()
    {
        _rb2d.velocity = Vector2.zero;
        yield return new WaitUntil(GetEndOverrideCoroutine);
        Destroy(gameObject);
    }

    public void Attack()
    {
        _motionOverrideCoroutine = StartCoroutine(AttackRoutine());
    }

    public void CheckAttackHitboxFrame(int frame)
    {
        AttackHitbox[] attackFrameHitboxes;

        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            attackFrameHitboxes = attack1Hitboxes[frame].hitboxes;
        }
        else if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
        {
            attackFrameHitboxes = attack2Hitboxes[frame].hitboxes;
        }
        else
        {
            return;
        }

        foreach (AttackHitbox hbox in attackFrameHitboxes)
        {
            foreach (Collider2D collider in Physics2D.OverlapBoxAll((Vector2)transform.position + new Vector2(hbox.rect.position.x * (int)_direction, hbox.rect.position.y), hbox.rect.size, hbox.rotation, enemyMask))
            {
                Damagable dobj = collider.GetComponent<Damagable>();
                if (dobj != null && collider.gameObject != gameObject && !attackHits.Contains(dobj.gameObject))
                {
                    dobj.Hurt(hbox.damage, new Vector2(hbox.knockback.x * (int)_direction, hbox.knockback.y));

                    attackHits.Add(dobj.gameObject);
                }
            }
        }
    }

    public void Hurt()
    {
        _motionOverrideCoroutine = StartCoroutine(HurtRoutine());
    }

    public override void Hurt(int damage, Vector2 knockback)
    {
        base.Hurt(damage, knockback);

        _animator.SetTrigger(hitHash);
    }

    protected override void Die()
    {
        _motionOverrideCoroutine = StartCoroutine(DieRoutine());
    }

    //Returns true if the player is moving horizontally
    public bool IsWalking()
    {
        return Mathf.Abs(_rb2d.velocity.x) > 0f;
    }

    //Returns true if the player is standing on ground
    public bool IsGrounded()
    {
        if (!IsOnGround())
        {
            return false;
        }
        else if (_rb2d.velocity.y > 0.01f) //This case catches the scenario where the player is jumping through a one-way platform
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //Returns true if the player is slightly above a piece of terrain
    //Different from IsGrounded() as it doesn't check the player's velocity,
    //meaning it can return a false positive when jumping through one-way platforms
    private bool IsOnGround()
    {
        return Physics2D.OverlapBox((Vector2)transform.position + movementParams.groundCheckRect.position, movementParams.groundCheckRect.size, 0f, movementParams.groundMask);
    }

    //Returns the direction the player is facing
    public FacingDirection GetFacingDirection()
    {
        return _direction;
    }

    //Returns the player's last negative vertical velocity
    public float GetGroundImpact()
    {
        return _prevFallingSpeed;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        for (int i = 0; i < attack1Sprites.Length; i++)
        {
            if (_spriteRenderer.sprite == attack1Sprites[i])
            {
                DrawAttackFrameHitboxes(attack1Hitboxes, i);
                return;
            }
        }
        for (int i = 0; i < attack2Sprites.Length; i++)
        {
            if (_spriteRenderer.sprite == attack2Sprites[i])
            {
                DrawAttackFrameHitboxes(attack2Hitboxes, i);
                return;
            }
        }
    }

    private void DrawAttackFrameHitboxes(AttackFrame[] attack, int frame)
    {
        foreach (AttackHitbox h in attack[frame].hitboxes)
        {
            float[] cornerAngles = {Mathf.Atan2(h.rect.size.y, h.rect.size.x),
                                    Mathf.Atan2(h.rect.size.y, -h.rect.size.x),
                                    Mathf.Atan2(-h.rect.size.y, -h.rect.size.x),
                                    Mathf.Atan2(-h.rect.size.y, h.rect.size.x)};

            Vector3[] hitboxCorners = new Vector3[cornerAngles.Length];
            for (int i = 0; i < cornerAngles.Length; i++)
            {
                cornerAngles[i] += h.rotation * Mathf.Deg2Rad;
                hitboxCorners[i] = transform.position + (Vector3)h.rect.position + new Vector3(Mathf.Cos(cornerAngles[i]), Mathf.Sin(cornerAngles[i])) * (h.rect.size.magnitude / 2);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLineStrip(hitboxCorners, true);
        }
    }
#endif
}