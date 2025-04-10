using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{

	public class Retreat_ACT : ActionTask
	{

		public BBParameter<Transform> target;
		public float distance;
		public float variance;
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
			Vector3 dirToTarget = target.value.position - agent.transform.position;
			Vector2 retreatPoint = agent.transform.position + new Vector3(dirToTarget.normalized.x * -distance, dirToTarget.normalized.y / 4 * -distance);
			destination.value = retreatPoint + Random.insideUnitCircle * variance;

			Debug.DrawLine(agent.transform.position, retreatPoint, new Color(255f, 100f, 100f), 2.5f);
			for (int i = 0; i < 8; i++)
			{
				Debug.DrawLine(retreatPoint + new Vector2(Mathf.Cos(i * 45 * Mathf.Deg2Rad), Mathf.Sin(i * 45 * Mathf.Deg2Rad)) * variance,
							   retreatPoint + new Vector2(Mathf.Cos((i + 1) * 45 * Mathf.Deg2Rad), Mathf.Sin((i + 1) * 45 * Mathf.Deg2Rad)) * variance,
							   new Color(255f, 100f, 100f), 2.5f);
			}

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