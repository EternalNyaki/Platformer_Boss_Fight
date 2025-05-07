using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerCharacterController : MonoBehaviour
{
    /// <summary>
    /// Describes the direction the player is facing
    /// </summary>
    public enum FacingDirection
    {
        left = -1, right = 1
    }

    [System.Flags]
    public enum InputFlags : byte
    {
        None = 0,
        Up = 0b_1000_0000,
        Down = 0b_0100_0000,
        Left = 0b_0010_0000,
        Right = 0b_0001_0000,
        Jump = 0b_0000_1000,
        Misc = 0b_0000_0100,
        Attack1 = 0b_0000_0010,
        Attack2 = 0b_0000_0001
    }

    public bool disableControls = false;
    public InputFlags inputFlags { get; private set; }

    public PlatformerControllerParams movementParams;

    public AttackFrame[] attack1Hitboxes;
    public AttackFrame[] attack2Hitboxes;
    public LayerMask enemyMask;

    public event System.EventHandler attackEvent;

    private InputFlags prevInputs;
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

    private List<GameObject> attackHits;

    private Rigidbody2D _rb2d;
    private Damagable _damageable;

    void Start()
    {
        //Get component references
        _rb2d = GetComponent<Rigidbody2D>();
        _damageable = GetComponent<Damagable>();

        attackHits = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!disableControls) { ApplyInputs(); }

        //Increment coyote time timer
        if (!IsGrounded())
        {
            _timeSinceLastGrounded += Time.deltaTime;
        }
        else
        {
            _timeSinceLastGrounded = 0;
        }
    }

    private void ApplyInputs()
    {
        //Get directional input
        _playerInput = Vector2.zero;
        if ((inputFlags & InputFlags.Left) != InputFlags.None)
        {
            _playerInput.x += -1;
        }
        if ((inputFlags & InputFlags.Right) != InputFlags.None)
        {
            _playerInput.x += 1;
        }

        if ((inputFlags & InputFlags.Down) != InputFlags.None)
        {
            _playerInput.y += -1;
        }
        if ((inputFlags & InputFlags.Up) != InputFlags.None)
        {
            _playerInput.y += 1;
        }

        if ((inputFlags & InputFlags.Jump) != InputFlags.None &&
            (prevInputs & InputFlags.Jump) == InputFlags.None)
        {
            _jumpTrigger = true;
        }
        else if ((inputFlags & InputFlags.Jump) == InputFlags.None &&
                 (prevInputs & InputFlags.Jump) != InputFlags.None)
        {
            _jumpReleaseTrigger = true;
        }

        if ((inputFlags & InputFlags.Attack1) != InputFlags.None &&
            (prevInputs & InputFlags.Attack1) == InputFlags.None &&
            IsGrounded())
        {
            OnAttackEvent(System.EventArgs.Empty);
        }

        prevInputs = inputFlags;
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

    private void OnAttackEvent(System.EventArgs e)
    {
        attackEvent?.Invoke(this, e);
    }

    private void ApplyGravity()
    {
        //Calculate gravity
        _rb2d.velocity = new Vector2(_rb2d.velocity.x, Mathf.Clamp(_rb2d.velocity.y + movementParams.gravity * Time.deltaTime, -movementParams.terminalVelocity, float.PositiveInfinity));
    }

    public void SetInputs(InputFlags inputs)
    {
        inputFlags = inputs;
    }

    // Returns true if the player is moving horizontally
    public bool IsWalking()
    {
        return Mathf.Abs(_rb2d.velocity.x) > 0f;
    }

    // Returns true if the player is standing on ground
    public bool IsGrounded()
    {
        if (!IsOnGround())
        {
            return false;
        }
        else if (_rb2d.velocity.y > 0.01f)
        {
            // This case catches the scenario where the player is jumping through a one-way platform
            return false;
        }
        else
        {
            return true;
        }
    }

    // Returns true if the player is slightly above any terrain
    // Different from IsGrounded() as it doesn't check the player's velocity, meaning it can return a false positive
    // when jumping through one-way platforms
    private bool IsOnGround()
    {
        Rect groundCheckRect = new Rect(movementParams.groundCheckRect);
        groundCheckRect.position += (Vector2)transform.position;

        return Physics2D.OverlapBox(groundCheckRect.position, groundCheckRect.size, 0f, movementParams.groundMask);
    }

    // Returns the direction the player is facing
    public FacingDirection GetFacingDirection()
    {
        return _direction;
    }

    public Vector2 GetVelocity()
    {
        return _rb2d.velocity;
    }
}