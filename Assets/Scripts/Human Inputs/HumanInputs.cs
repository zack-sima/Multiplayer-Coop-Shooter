using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanInputs : MonoBehaviour {

	#region Constants & Statics

	public static HumanInputs instance;
	private const bool USE_MOBILE = false;

	#endregion

	#region Functions

	//TODO: mobile should still read other inputs here but redirect it
	private void Update() {
		if (USE_MOBILE || EntityController.player == null) return;

		CombatEntity player = EntityController.player;

		if (Input.GetMouseButton(0)) {
			player.TryFireMainWeapon();
		}

		//point to mouse
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit hit)) {
			Vector2 diff = new(hit.point.x - player.transform.position.x,
				hit.point.z - player.transform.position.z);
			player.GetTurret().SetTargetTurretRotation(
				Mathf.Atan2(diff.x, diff.y) * Mathf.Rad2Deg
			);
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

		//TODO: formalize camera tracking
		Camera.main.transform.position = player.transform.position + new Vector3(0, 10, -4.19f);
	}
	private void Awake() {
		instance = this;
	}

	#endregion
}
