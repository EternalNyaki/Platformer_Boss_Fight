using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class JumpOffScreen_ACT : ActionTask
	{

		public float jumpSpeed;
		public float time;

		private float _timer;

		private Rigidbody2D _rb2d;

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit()
		{
			_rb2d = agent.GetComponent<Rigidbody2D>();

			return null;
		}

		//This is called once each time the task is enabled.
		//Call EndAction() to mark the action as finished, either in success or failure.
		//EndAction can be called from anywhere.
		protected override void OnExecute()
		{
			_timer = time;
		}

		//Called once per frame while the action is active.
		protected override void OnUpdate()
		{
			_rb2d.velocity = Vector2.up * jumpSpeed;

			if (_timer <= 0)
			{
				EndAction(true);
			}

			_timer -= Time.deltaTime;
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