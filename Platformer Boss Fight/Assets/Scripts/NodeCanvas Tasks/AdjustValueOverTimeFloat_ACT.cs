using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class AdjustValueOverTimeFloat_ACT : ActionTask
	{
		public BBParameter<float> bbValue;
		public OperationMethod operation;
		public float valuePerSecond;
		public float maximum;

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit()
		{
			if (!bbValue.useBlackboard)
			{
				return "Value must be a blackboard variable";
			}

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
			switch (operation)
			{
				case OperationMethod.Set:
					bbValue.value = valuePerSecond * Time.deltaTime;
					break;

				case OperationMethod.Add:
					bbValue.value += valuePerSecond * Time.deltaTime;
					break;

				case OperationMethod.Subtract:
					bbValue.value -= valuePerSecond * Time.deltaTime;
					break;

				case OperationMethod.Multiply:
					bbValue.value *= valuePerSecond * Time.deltaTime;
					break;

				case OperationMethod.Divide:
					bbValue.value /= valuePerSecond * Time.deltaTime;
					break;
			}

			if (bbValue.value >= maximum)
			{
				bbValue.value = maximum;
				EndAction(true);
			}
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