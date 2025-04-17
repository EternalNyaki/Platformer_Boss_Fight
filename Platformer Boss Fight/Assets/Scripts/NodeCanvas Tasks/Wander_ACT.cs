using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Pathfinding;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class Wander_ACT : ActionTask
	{
		public float wanderRange;
		public BBParameter<Vector2> destination;
		public BBParameter<bool> calculatePathTrigger;

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit()
		{
			if (!destination.useBlackboard)
			{
				return "Destination must be connected to a blackboard variable";
			}

			return null;
		}

		//This is called once each time the task is enabled.
		//Call EndAction() to mark the action as finished, either in success or failure.
		//EndAction can be called from anywhere.
		protected override void OnExecute()
		{
			destination.value = (Vector2)agent.transform.position + Random.insideUnitCircle.normalized * wanderRange;
			calculatePathTrigger.value = true;

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