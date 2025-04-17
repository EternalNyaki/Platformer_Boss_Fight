using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

	public class ExecuteAttack_ACT : ActionTask
	{

		public AttackData attackData;
		public GameObject telegraphSpritePrefab;
		public GameObject attackSpritePrefab;
		public float waitTime;

		private List<GameObject> _telegraphSpriteObjects;
		private float _telegraphTimer;
		private float _attackTimer;

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

			_telegraphTimer = 0f;
			_attackTimer = 0f;
		}

		//Called once per frame while the action is active.
		protected override void OnUpdate()
		{
			// if (_attackTimer >= attackData.data[attackData.data.Length - 1].timing)
			// {
			// 	EndAction(true);
			// }
			// else if (_telegraphTimer >= waitTime)
			// {
			// 	Attack();
			// }
			// else
			// {
			// 	Telegraph();
			// }
		}

		private void Telegraph()
		{
			// _telegraphTimer += Time.deltaTime;

			// for (int i = attackData.data.Length - 1; i >= 0; i--)
			// {
			// 	if (_telegraphTimer >= attackData.data[i].timing && _telegraphSpriteObjects.Count <= i)
			// 	{
			// 		_telegraphSpriteObjects.Add(GameObject.Instantiate(telegraphSpritePrefab));
			// 		_telegraphSpriteObjects[_telegraphSpriteObjects.Count - 1].transform.position = attackData.data[i].hitboxRect.position;
			// 		_telegraphSpriteObjects[_telegraphSpriteObjects.Count - 1].transform.localScale = attackData.data[i].hitboxRect.size;
			// 		_telegraphSpriteObjects[_telegraphSpriteObjects.Count - 1].transform.rotation = Quaternion.Euler(new(0f, 0f, attackData.data[i].hitboxRotation));

			// 		break;
			// 	}
			// }
		}

		private void Attack()
		{
			// _attackTimer += Time.deltaTime;

			// for (int i = attackData.data.Length - 1; i >= 0; i--)
			// {
			// 	if (_attackTimer >= attackData.data[i].timing && _telegraphSpriteObjects.Count >= attackData.data.Length - i)
			// 	{
			// 		if (_telegraphSpriteObjects[0] != null)
			// 		{
			// 			GameObject.Destroy(_telegraphSpriteObjects[0]);
			// 			_telegraphSpriteObjects.RemoveAt(0);
			// 		}

			// 		Transform t = GameObject.Instantiate(attackSpritePrefab).transform;
			// 		t.position = attackData.data[i].hitboxRect.position;
			// 		t.localScale = attackData.data[i].hitboxRect.size;
			// 		t.rotation = Quaternion.Euler(new(0f, 0f, attackData.data[i].hitboxRotation));

			// 		break;
			// 	}
			// }
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