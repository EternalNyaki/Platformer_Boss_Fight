using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Unity.VisualScripting;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class ExecuteAttack_ACT : ActionTask
	{

		public BBParameter<AttackData> attackData;
		public BBParameter<LayerMask> attackMask;
		public BBParameter<Vector2> rootPosition;

		private float _attackTimer;
		private int _numOfHitboxesExecuted;

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
			_attackTimer = 0f;
			_numOfHitboxesExecuted = 0;
		}

		//Called once per frame while the action is active.
		protected override void OnUpdate()
		{
			_attackTimer += Time.deltaTime;

			List<AttackHitbox> hitboxesToExecute = new List<AttackHitbox>();
			for (int i = attackData.value.data.Length - 1; i >= 0; i--)
			{
				if (i < _numOfHitboxesExecuted) { break; }

				if (_attackTimer >= attackData.value.data[i].timing)
				{
					hitboxesToExecute.Add(attackData.value.data[i].hitbox);
				}
			}

			foreach (AttackHitbox hbox in hitboxesToExecute)
			{
				foreach (Collider2D collider in Physics2D.OverlapBoxAll(rootPosition.value + hbox.rect.position, hbox.rect.size, hbox.rotation, attackMask.value))
				{
					Damagable dobj = collider.GetComponent<Damagable>();
					if (dobj != null && collider.gameObject != agent)
					{
						dobj.Hurt(hbox.damage, hbox.knockback);
					}
				}

				_numOfHitboxesExecuted++;
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