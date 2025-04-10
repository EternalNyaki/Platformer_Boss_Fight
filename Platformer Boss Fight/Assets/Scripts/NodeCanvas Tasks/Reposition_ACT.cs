using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class Reposition_ACT : ActionTask
	{

		public BBParameter<Vector2[]> repositionPoints;
		public float repositionCalcRadius;
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
			List<Vector2> pointsWithinRange = new List<Vector2>();
			Vector2 closestPoint = new(float.PositiveInfinity, float.PositiveInfinity);
			foreach (Vector2 point in repositionPoints.value)
			{
				float distanceToPoint = (point - (Vector2)agent.transform.position).magnitude;
				float distanceToClosest = (closestPoint - (Vector2)agent.transform.position).magnitude;

				if (distanceToPoint < 2f) { continue; }

				if (distanceToPoint <= repositionCalcRadius)
				{
					pointsWithinRange.Add(point);
				}

				if (distanceToPoint < distanceToClosest)
				{
					closestPoint = point;
				}
			}

			destination.value = pointsWithinRange.Count > 0 ? pointsWithinRange[Random.Range(0, pointsWithinRange.Count - 1)] : closestPoint;

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