using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Pathfinding;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class AnimUpdate_ACT : ActionTask
	{

		public BBParameter<Vector2> groundCheckOffset;
		public BBParameter<Vector2> groundCheckSize;
		public BBParameter<LayerMask> groundMask;

		private PlayerController.FacingDirection _direction;

		private int _xMovementHash, _yMovementHash, _groundedHash;

		private SpriteRenderer _spriteRenderer;
		private Animator _animator;
		private AIPath _navPath;

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit()
		{
			_spriteRenderer = agent.GetComponent<SpriteRenderer>();
			_animator = agent.GetComponent<Animator>();
			_navPath = agent.GetComponent<AIPath>();

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
			float xMovement = Mathf.Abs(_navPath.velocity.x);

			//Change facing direction
			if (_navPath.velocity.x > 0.01f)
			{
				_direction = PlayerController.FacingDirection.right;
			}
			else if (_navPath.velocity.x < -0.01f)
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
			_animator.SetFloat(_yMovementHash, _navPath.velocity.y);
			_animator.SetBool(_groundedHash, IsGrounded());
		}

		//Called when the task is disabled.
		protected override void OnStop()
		{

		}

		//Called when the task is paused.
		protected override void OnPause()
		{

		}

		//Returns true if the player is standing on ground
		private bool IsGrounded()
		{
			if (!IsOnGround())
			{
				return false;
			}
			else if (_navPath.velocity.y > 0.1f) //This case catches the scenario where the player is jumping through a one-way platform
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
	}
}