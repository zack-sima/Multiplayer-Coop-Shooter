using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanInputs : MonoBehaviour {

	#region Constants & Statics

	public static HumanInputs instance;
	private const bool USE_MOBILE = false;

	#endregion

	#region Members

	private Vector3 cameraLocalPosition = Vector3.zero;

	#endregion

	#region Functions

	private void Start() {
		cameraLocalPosition = Camera.main.gameObject.transform.localPosition;
	}
	//TODO: mobile should still read other inputs here but redirect it
	private void Update() {
		if (USE_MOBILE || EntityController.player == null ||
			EntityController.player.GetNetworker().GetIsDead()) return;

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
		Vector3 moveVector = Vector3.zero;
		if (Input.GetKey(KeyCode.W)) {
			moveVector += Vector3.forward;
		}
		if (Input.GetKey(KeyCode.S)) {
			moveVector += Vector3.back;
		}
		if (Input.GetKey(KeyCode.A)) {
			moveVector += Vector3.left;
		}
		if (Input.GetKey(KeyCode.D)) {
			moveVector += Vector3.right;
		}
		player.GetHull().Move(moveVector);

		//TODO: formalize camera tracking
		Camera.main.transform.position = player.transform.position + cameraLocalPosition;
	}
	private void Awake() {
		instance = this;
	}

	#endregion
}
