using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class SetAnimTrigger_ACT : ActionTask
	{

		public BBParameter<string> animTrigger;

		private int _triggerHash;
		private Animator _animator;

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit()
		{
			_animator = agent.GetComponent<Animator>();

			_triggerHash = Animator.StringToHash(animTrigger.value);

			return null;
		}

		//This is called once each time the task is enabled.
		//Call EndAction() to mark the action as finished, either in success or failure.
		//EndAction can be called from anywhere.
		protected override void OnExecute()
		{
			_animator.SetTrigger(_triggerHash);

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