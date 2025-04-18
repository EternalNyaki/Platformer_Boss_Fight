using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Unity.VisualScripting;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class SpawnProjectile_ACT : ActionTask
	{

		public BBParameter<GameObject> projectilePrefab;
		public BBParameter<Transform> target;

		private SpriteRenderer _spriteRenderer;

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit()
		{
			_spriteRenderer = agent.GetComponent<SpriteRenderer>();

			return null;
		}

		//This is called once each time the task is enabled.
		//Call EndAction() to mark the action as finished, either in success or failure.
		//EndAction can be called from anywhere.
		protected override void OnExecute()
		{
			Vector2 vectorToTarget = target.value.position - agent.transform.position;

			_spriteRenderer.flipX = vectorToTarget.x < 0f;

			GameObject.Instantiate(projectilePrefab.value, (Vector2)agent.transform.position + new Vector2(_spriteRenderer.flipX ? -2f : 2f, 1.5f), Quaternion.Euler(0f, 0f, Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg));

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