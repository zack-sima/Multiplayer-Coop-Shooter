using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AINavigator : MonoBehaviour {

	#region References

	[SerializeField] private NavMeshAgent agent;

	#endregion

	#region Functions

	//set this to false for all spiders
	public void SetRotatable(bool rotatable) {
		agent.updateRotation = rotatable;
	}
	public void SetSpeed(float speed) {
		agent.speed = speed;
	}
	public void SetTarget(Vector3 targetPosition) {
		agent.SetDestination(targetPosition);
	}
	public void SetStopped(bool stopped) {
		agent.isStopped = stopped;
	}
	public void SetActive(bool isActive) {
		if (agent.enabled != isActive)
			agent.enabled = isActive;
	}

	#endregion

}
