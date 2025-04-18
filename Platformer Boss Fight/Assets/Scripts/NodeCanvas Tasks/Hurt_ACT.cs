using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class Hurt_ACT : ActionTask
	{

		public BBParameter<BTCharacterHurtEventArgs> hurtEventArgs;
		public BBParameter<int> health;

		private int _hurtHash, _healthHash;

		private Rigidbody2D _rb2d;
		private Animator _animator;

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit()
		{
			_rb2d = agent.GetComponent<Rigidbody2D>();
			_animator = agent.GetComponent<Animator>();

			_hurtHash = Animator.StringToHash("hurt");
			_healthHash = Animator.StringToHash("health");

			return null;
		}

		//This is called once each time the task is enabled.
		//Call EndAction() to mark the action as finished, either in success or failure.
		//EndAction can be called from anywhere.
		protected override void OnExecute()
		{
			health.value -= hurtEventArgs.value.damage;

			_rb2d.velocity = hurtEventArgs.value.knockback;

			_animator.SetInteger(_healthHash, health.value);
			_animator.SetTrigger(_hurtHash);

			EndAction(true);
		}

		//Called once per frame while the action is active.
		protected override void OnUpdate()
		{

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