using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Pathfinding;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class Wander_ACT : ActionTask
	{
		public float wanderRange;

		private Seeker _navAgent;
		private AIPath _navPath;
		private bool _isPathCalculated;

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit()
		{
			_navAgent = agent.GetComponent<Seeker>();
			_navPath = agent.GetComponent<AIPath>();

			return null;
		}

		//This is called once each time the task is enabled.
		//Call EndAction() to mark the action as finished, either in success or failure.
		//EndAction can be called from anywhere.
		protected override void OnExecute()
		{
			_isPathCalculated = false;

			_navAgent.StartPath(agent.transform.position, new(Random.Range(-wanderRange, wanderRange), 0f, 0f), OnPathFinished);
		}

		private void OnPathFinished(Path p)
		{
			_isPathCalculated = true;
		}

		//Called once per frame while the action is active.
		protected override void OnUpdate()
		{
			if (_navPath.remainingDistance <= 0.01f)
			{
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