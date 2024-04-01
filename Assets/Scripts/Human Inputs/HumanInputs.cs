using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanInputs : MonoBehaviour {

	#region References

	[SerializeField] private CombatEntity player;

	#endregion

	#region Functions

	void Update() {

		//TODO: change to use input manager

		if (Input.GetKey(KeyCode.Space)) {
			player.GetTurret().TryFireMainWeapon();
		}
		if (Input.GetKey(KeyCode.Q)) {
			player.GetTurret().RotateTurret(isRight: false);
		}
		if (Input.GetKey(KeyCode.E)) {
			player.GetTurret().RotateTurret(isRight: true);
		}
		if (Input.GetKey(KeyCode.W)) {
			player.GetHull().Move(Vector3.forward);
		}
		if (Input.GetKey(KeyCode.S)) {
			player.GetHull().Move(Vector3.back);
		}
		if (Input.GetKey(KeyCode.A)) {
			player.GetHull().Move(Vector3.left);
		}
		if (Input.GetKey(KeyCode.D)) {
			player.GetHull().Move(Vector3.right);
		}
	}

	#endregion
}
