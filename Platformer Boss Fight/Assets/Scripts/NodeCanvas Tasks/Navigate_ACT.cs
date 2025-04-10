using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Pathfinding;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class Navigate_ACT : ActionTask
	{
		public BBParameter<PlatformerControllerParams> movementParams;

		public BBParameter<float> destinationReachedThreshold;

		public BBParameter<Vector2> destination;
		public BBParameter<bool> calculatePathTrigger;
		public BBParameter<bool> pathCalculationCompleteTrigger;
		public BBParameter<bool> destinationReachedTrigger;

		public float movementFailureWaitTime = 5f;

		private int _nextNodeIndex = 0;
		private float _previousNodeReachedTime;

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
			_previousNodeReachedTime = Time.time;
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
				if (nodePosition.y > ((Vector3)_path.path[_nextNodeIndex - 1].position).y &&
					nodePosition.y > playerPosition.y)
				{
					fakeInput.y = 1f;
				}
			}

			fakeInput.x = playerPosition.x < nodePosition.x ? 1f : -1f;

			if (Time.time > _previousNodeReachedTime + movementFailureWaitTime)
			{
				destinationReachedTrigger.value = true;
				_path = null;
				Debug.LogWarning("Could not complete path");
			}

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
				if (xVelocity < movementParams.value.minMovementTolerance && xVelocity > -movementParams.value.minMovementTolerance)
				{
					//Do nothing
					xVelocity = 0f;
				}
				else
				{
					//Decelerate
					xVelocity = Mathf.Clamp(-Mathf.Sign(xVelocity) * movementParams.value.acceleration * Time.deltaTime, -movementParams.value.maxSpeed, movementParams.value.maxSpeed);
				}
			}
			else
			{
				//Accelerate
				xVelocity = Mathf.Clamp(xVelocity + horizontalInput * movementParams.value.acceleration * Time.deltaTime, -movementParams.value.maxSpeed, movementParams.value.maxSpeed);

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
			//Calculate gravity
			yVelocity = Mathf.Clamp(yVelocity + movementParams.value.gravity * Time.deltaTime, -movementParams.value.terminalVelocity, float.PositiveInfinity);

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
			yVelocity = movementParams.value.jumpVelocity;
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
			return Physics2D.OverlapBox((Vector2)agent.transform.position + movementParams.value.groundCheckRect.position, movementParams.value.groundCheckRect.size, 0f, movementParams.value.groundMask);
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