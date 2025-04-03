using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Pathfinding;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class Navigate_ACT : ActionTask
	{

		//Maximum walking speed (in units per second)
		public BBParameter<float> maxSpeed;
		//Time to reach maximum speed (in seconds)
		public BBParameter<float> accelerationTime;

		//Maximum apex jump height (in units)
		public BBParameter<float> apexHeight;
		//Time to reach maximum apex jump height (in seconds)
		public BBParameter<float> apexTime;
		//Terminal speed when falling (in units/s)
		public BBParameter<float> terminalVelocity;

		public BBParameter<float> destinationReachedThreshold;

		public BBParameter<Vector2> destination;
		public BBParameter<bool> calculatePathTrigger;
		public BBParameter<bool> pathCalculationCompleteTrigger;
		public BBParameter<bool> destinationReachedTrigger;

		public BBParameter<Vector2> groundCheckOffset;
		public BBParameter<Vector2> groundCheckSize;
		public BBParameter<LayerMask> groundMask;

		private int _nextNodeIndex = 0;

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

		private PlayerController.FacingDirection _direction;

		private int _xMovementHash, _yMovementHash, _groundedHash;

		private ABPath _path;

		private SpriteRenderer _spriteRenderer;
		private Animator _animator;

		private Rigidbody2D _rb2d;
		private Seeker _navAgent;

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit()
		{
			if (!destination.useBlackboard)
			{
				return "Destination must be connected to a blackboard variable";
			}

			_rb2d = agent.GetComponent<Rigidbody2D>();
			_navAgent = agent.GetComponent<Seeker>();
			_spriteRenderer = agent.GetComponent<SpriteRenderer>();
			_animator = agent.GetComponent<Animator>();

			_xMovementHash = Animator.StringToHash("xMovement");
			_yMovementHash = Animator.StringToHash("yMovement");
			_groundedHash = Animator.StringToHash("grounded");

			//Calculate movement values
			_acceleration = maxSpeed.value / accelerationTime.value;
			_gravity = -2 * apexHeight.value / Mathf.Pow(apexTime.value, 2);
			_jumpVelocity = 2 * apexHeight.value / apexTime.value;
			_minMovementTolerance = _acceleration * Time.deltaTime * 2;

			return null;
		}

		//This is called once each time the task is enabled.
		//Call EndAction() to mark the action as finished, either in success or failure.
		//EndAction can be called from anywhere.
		protected override void OnExecute()
		{

		}

		//Called once per frame while the action is active.
		protected override void OnUpdate()
		{
			if (calculatePathTrigger.value)
			{
				CalculatePath();

				calculatePathTrigger.value = false;
			}

			if (_path != null)
			{
				MovementUpdate(FollowPath());
			}
			else
			{
				MovementUpdate(new(0f, 0f));
			}

			AnimUpdate();
		}

		private void CalculatePath()
		{
			_navAgent.StartPath(agent.transform.position, destination.value, OnPathComplete);
		}

		private void OnPathComplete(Path p)
		{
			if (p.error)
			{
				Debug.LogError("Path calculation failed: " + p.errorLog);
				return;
			}

			_path = (ABPath)p;

			Debug.DrawLine(_path.endPoint, _path.originalEndPoint, Color.magenta, 2f);

			_nextNodeIndex = 0;
			pathCalculationCompleteTrigger.value = true;
		}

		private Vector2 FollowPath()
		{
			Vector2 fakeInput = new(0f, 0f);
			Vector2 playerPosition = agent.transform.position;
			Vector2 nodePosition = (Vector3)_path.path[_nextNodeIndex].position;

			if ((playerPosition - nodePosition).magnitude <= destinationReachedThreshold.value)
			{
				Debug.Log($"Node {_nextNodeIndex} reached");

				if (++_nextNodeIndex >= _path.path.Count)
				{
					destinationReachedTrigger.value = true;
					_path = null;

					Debug.Log("Path complete");
					Debug.DrawLine(playerPosition + new Vector2(0f, 20f), playerPosition - new Vector2(0f, 20f), Color.blue, 3f);
					return new(0f, 0f);
				}

				Debug.DrawLine(playerPosition + new Vector2(0f, 20f), playerPosition - new Vector2(0f, 20f), Color.cyan, 3f);
			}

			if (_nextNodeIndex > 0)
			{
				// NodeLink2 jumpLink = NodeLink2.GetNodeLink(_path.path[_nextNodeIndex - 1]);
				// if (jumpLink != null)
				// {
				// 	fakeInput.y = 1f;
				// }

				if (nodePosition.y > ((Vector3)_path.path[_nextNodeIndex - 1].position).y &&
					nodePosition.y > playerPosition.y - 0.25f)
				{
					fakeInput.y = 1f;
				}
			}

			fakeInput.x = playerPosition.x < nodePosition.x ? 1f : -1f;

			return fakeInput;
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
					xVelocity = Mathf.Clamp(-Mathf.Sign(xVelocity) * _acceleration * Time.deltaTime, -maxSpeed.value, maxSpeed.value);
				}
			}
			else
			{
				//Accelerate
				xVelocity = Mathf.Clamp(xVelocity + horizontalInput * _acceleration * Time.deltaTime, -maxSpeed.value, maxSpeed.value);

				//Change facing direction
				if (horizontalInput > 0)
				{
					_direction = PlayerController.FacingDirection.right;
				}
				else if (horizontalInput < 0)
				{
					_direction = PlayerController.FacingDirection.left;
				}
			}
		}

		//Calculate vertical movement (gravity and jumping)
		private void VerticalMovement(float verticalInput, ref float yVelocity)
		{
			if (!IsGrounded())
			{
				//Calculate gravity
				yVelocity = Mathf.Clamp(yVelocity + _gravity * Time.deltaTime, -terminalVelocity.value, float.PositiveInfinity);
			}

			if (verticalInput > 0f && IsGrounded())
			{
				//Jump
				Jump(ref yVelocity);
			}
			if (verticalInput <= 0f && yVelocity > 0f)
			{
				//Shorten jump (for dynamic jump height)
				yVelocity /= 2;
			}
		}

		private void Jump(ref float yVelocity)
		{
			yVelocity = _jumpVelocity;
		}

		private void AnimUpdate()
		{
			float xMovement = Mathf.Abs(_rb2d.velocity.x);

			//Change facing direction
			if (_rb2d.velocity.x > 0.01f)
			{
				_direction = PlayerController.FacingDirection.right;
			}
			else if (_rb2d.velocity.x < -0.01f)
			{
				_direction = PlayerController.FacingDirection.left;
			}

			//Flip the player sprite based on facing direction
			switch (_direction)
			{
				case PlayerController.FacingDirection.left:
					_spriteRenderer.flipX = true;
					break;
				case PlayerController.FacingDirection.right:
				default:
					_spriteRenderer.flipX = false;
					break;
			}

			_animator.SetFloat(_xMovementHash, xMovement);
			_animator.SetFloat(_yMovementHash, _rb2d.velocity.y);
			_animator.SetBool(_groundedHash, IsGrounded());
		}

		//Returns true if the player is standing on ground
		private bool IsGrounded()
		{
			if (!IsOnGround())
			{
				return false;
			}
			else if (_rb2d.velocity.y > 0.1f) //This case catches the scenario where the player is jumping through a one-way platform
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
			return Physics2D.OverlapBox((Vector2)agent.transform.position + groundCheckOffset.value, groundCheckSize.value, 0f, groundMask.value);
		}

		//Called when the task is disabled.
		protected override void OnStop()
		{

		}

		//Called when the task is paused.
		protected override void OnPause()
		{

		}
	}
}