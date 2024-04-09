using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hull : MonoBehaviour {

	#region References

	//NOTE: if rigidbody doesn't exist, use transform for all movements
	[SerializeField] private Rigidbody optionalRigidbody;

	//move this root transform (which could have a rigidbody) but rotate locally
	[SerializeField] private Transform rootTransform;

	[SerializeField] private HullAnimatorBase animator;
	public HullAnimatorBase GetAnimator() { return animator; }

	#endregion

	#region Members

	[SerializeField] private float speed;
	public float GetSpeed() { return speed; }

	#endregion

	#region Functions

	//NOTE: movement can also be performed by AINavigator for AI combat entities that have a NavMeshAgent
	public void Move(Vector3 direction) {
		if (optionalRigidbody == null) {
			rootTransform.Translate(speed * Time.deltaTime * direction);
		} else {
			optionalRigidbody.velocity = speed * direction;
		}
	}

	#endregion

}
