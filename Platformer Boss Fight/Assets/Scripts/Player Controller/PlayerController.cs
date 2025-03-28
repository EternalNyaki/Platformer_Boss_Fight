using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Describes the direction the player is facing
    /// </summary>
    public enum FacingDirection
    {
        left = -1, right = 1
    }

    //Player health, only used to trigger death animation
    //HACK: Unused functionality to demonstrate death animation
    public int health = 10;

    //Maximum walking speed (in units per second)
    public float maxSpeed = 7f;
    //Time to reach maximum speed (in seconds)
    public float accelerationTime = 0.1f;

    //Maximum apex jump height (in units)
    public float apexHeight = 2.5f;
    //Time to reach maximum apex jump height (in seconds)
    public float apexTime = 0.33f;
    //Terminal speed when falling (in units/s)
    public float terminalVelocity = 20f;

    //The duration of time during which the player can still jump after leaving the ground (in seconds)
    public float coyoteTime = 0.13f;

    //Size and offset of BoxCast to check for terrain below the player
    public Vector2 groundCheckSize, groundCheckOffset;

    //Layermask for terrain the player can walk on and wall jump off of
    public LayerMask groundMask;

    //Vector for storing directional input
    private Vector2 _playerInput;
    //The direction the player is facing
    private FacingDirection _direction = FacingDirection.right;

    //The player's horizontal acceleration (in units/s^2)
    //Derived from the player's maximum walking speed and time to reach maximum walking speed
    private float _acceleration;
    //The minimum amount of movement for the player to be considered moving
    //Used to stop the player from vibrating endlessly instead of stopping
    private float _minMovementTolerance;

    //The player's vertical acceleration due to gravity (in units/s^2)
    //Derived from the player's maximum apex height and time to reach maximum apex height
    private float _gravity;
    //The initial vertical speed of the player's jump (in units/s)
    //Derived from the player's maximum apex height and time to reach maximum apex height
    private float _jumpVelocity;

    //Booleans from storing player input between Update() and FixedUpdate()
    private bool _jumpTrigger, _jumpReleaseTrigger;

    //Amount of time since the player was last grounded
    //Used for coyote time
    private float _timeSinceLastGrounded = 0;

    //The player's previous falling speed
    //Used to calculate camera shake intensity when landing
    private float _prevFallingSpeed = 0f;

    //Reference to the player's Rigidbody
    private Rigidbody2D _rb2d;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private int xMovementHash, yMovementHash, groundedHash;

    // Start is called before the first frame update
    void Start()
    {
        //Get component references
        _rb2d = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        //Calculate movement values
        _acceleration = maxSpeed / accelerationTime;
        _gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        _jumpVelocity = 2 * apexHeight / apexTime;
        _minMovementTolerance = _acceleration * Time.deltaTime * 2;

        xMovementHash = Animator.StringToHash("xMovement");
        yMovementHash = Animator.StringToHash("yMovement");
        groundedHash = Animator.StringToHash("grounded");
    }

    // Update is called once per frame
    void Update()
    {
        //Get directional input
        _playerInput.x = Input.GetAxisRaw("Horizontal");
        _playerInput.y = Input.GetAxisRaw("Vertical");
        _playerInput.Normalize();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpTrigger = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            _jumpReleaseTrigger = true;
        }

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

        AnimUpdate();
    }

    private void AnimUpdate()
    {
        _animator.SetFloat(xMovementHash, Mathf.Abs(_rb2d.velocity.x));
        _animator.SetFloat(yMovementHash, _rb2d.velocity.y);
        _animator.SetBool(groundedHash, IsGrounded());

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
        Vector2 velocity = _rb2d.velocity;

        //Calculate movement
        HorizontalMovement(playerInput.x, ref velocity.x);
        VerticalMovement(playerInput.y, ref velocity.y);

        //Apply movement
        _rb2d.velocity = velocity;
    }

    //Calculate horizontal movement (horizontal input)
    private void HorizontalMovement(float horizontalInput, ref float xVelocity)
    {
        if (horizontalInput == 0)
        {
            if (xVelocity < _minMovementTolerance && xVelocity > -_minMovementTolerance)
            {
                //Do nothing
                xVelocity = 0f;
            }
            else
            {
                //Decelerate
                xVelocity = Mathf.Clamp(-Mathf.Sign(xVelocity) * _acceleration * Time.deltaTime, -maxSpeed, maxSpeed);
            }
        }
        else
        {
            //Accelerate
            xVelocity = Mathf.Clamp(xVelocity + horizontalInput * _acceleration * Time.deltaTime, -maxSpeed, maxSpeed);

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
        if (!IsGrounded())
        {
            //Calculate gravity
            yVelocity = Mathf.Clamp(yVelocity + _gravity * Time.deltaTime, -terminalVelocity, float.PositiveInfinity);
        }

        if (_jumpTrigger && (IsGrounded() || _timeSinceLastGrounded < coyoteTime))
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
        yVelocity = _jumpVelocity;
    }

    public void Die()
    {
        gameObject.SetActive(false);
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
        return Physics2D.OverlapBox((Vector2)transform.position + groundCheckOffset, groundCheckSize, 0f, groundMask);
    }

    //Returns true if the player's health is 0
    public bool IsDead()
    {
        return health <= 0;
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
}