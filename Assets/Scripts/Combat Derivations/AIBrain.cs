using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Autonomously decides on an AI enemy's course of action
/// </summary>

public class AIBrain : MonoBehaviour {

	#region References

	[SerializeField] private AINavigator navigator;
	[SerializeField] private CombatEntity entity;

	#endregion

	#region Functions

	//TODO: for networking, give pathfinding target-setting authority to only ONE player (lowest-id?)
	//to prevent AI decisions from overriding each other
	private void Update() {
		//TODO: test for navigator
		if (Input.GetMouseButtonDown(0)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				navigator.SetTarget(hit.point);
			}
		}
	}
	private void Start() {
		navigator.SetRotatable(false);
		navigator.SetSpeed(5f);
	}

	#endregion

}
