using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class TelegraphAttack_ACT : ActionTask
	{

		public BBParameter<GameObject> telegraphSpritePrefab;
		public BBParameter<AttackData> attackData;
		public BBParameter<Vector2> rootPosition;
		public BBParameter<float> telegraphDelay;

		private List<GameObject> _telegraphSpriteObjects;

		private float _timer;

		//Use for initialization. This is called only once in the lifetime of the task.
		//Return null if init was successfull. Return an error string otherwise
		protected override string OnInit()
		{
			return null;
		}

		//This is called once each time the task is enabled.
		//Call EndAction() to mark the action as finished, either in success or failure.
		//EndAction can be called from anywhere.
		protected override void OnExecute()
		{
			_telegraphSpriteObjects = new List<GameObject>();

			_timer = 0f;
		}

		//Called once per frame while the action is active.
		protected override void OnUpdate()
		{
			_timer += Time.deltaTime;

			List<AttackHitbox> hitboxesToTelegraph = new List<AttackHitbox>();
			for (int i = attackData.value.data.Length - 1; i >= 0; i--)
			{
				if (i < _telegraphSpriteObjects.Count) { break; }

				if (_timer >= attackData.value.data[i].timing)
				{
					hitboxesToTelegraph.Add(attackData.value.data[i].hitbox);
				}
			}

			foreach (AttackHitbox hbox in hitboxesToTelegraph)
			{
				GameObject obj = GameObject.Instantiate(telegraphSpritePrefab.value);

				obj.transform.position = rootPosition.value + hbox.rect.position;
				obj.transform.localScale = hbox.rect.size;
				obj.transform.rotation = Quaternion.Euler(new(0f, 0f, hbox.rotation));
				obj.GetComponent<DestructionTimer>().SetTimer(attackData.value.data[attackData.value.data.Length - 1].timing + telegraphDelay.value);

				_telegraphSpriteObjects.Add(obj);
			}

			if (_timer > attackData.value.data[attackData.value.data.Length - 1].timing)
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