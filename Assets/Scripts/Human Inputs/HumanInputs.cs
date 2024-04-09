using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanInputs : MonoBehaviour {

	#region Constants & Statics

	public static HumanInputs instance;

	#endregion

	#region References

	[SerializeField] private RectTransform mobileUI;
	[SerializeField] private MobileJoystick movementJoystick;
	[SerializeField] private MobileJoystick mainWeaponJoystick;

	#endregion

	#region Members

	private Vector3 cameraLocalPosition = Vector3.zero;

	#endregion

	#region Functions

	#region Mobile

	private void MobileOnlyUpdate(CombatEntity player) {
		player.SetTurretFollowsMovement(true);

		if (movementJoystick.GetButtonIsDown()) {
			float mag = movementJoystick.GetJoystickMagnitude();
			float angleRad = movementJoystick.GetJoystickAngle();

			Vector3 moveVector = mag * new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));
			player.GetHull().Move(moveVector);
		} else {
			player.GetHull().Move(Vector3.zero);
		}
		if (mainWeaponJoystick.GetButtonIsDown()) {
			player.TryFireMainWeapon();

			float mag = mainWeaponJoystick.GetJoystickMagnitude();
			if (mag > 0.33f) {
				player.MaintainTurretRotation();
				player.GetTurret().SetTargetTurretRotation(
					-mainWeaponJoystick.GetJoystickAngle() * Mathf.Rad2Deg + 90f);
			}
		}
	}
	//for semi-auto release fire
	public void MainWeaponJoystickReleased() { }

	#endregion

	private void PCOnlyUpdate(CombatEntity player) {
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

		//wasd movement
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
		moveVector = moveVector.normalized;
		player.GetHull().Move(moveVector);
	}

	private void Start() {
		cameraLocalPosition = Camera.main.gameObject.transform.localPosition;

		UIController.instance.SetMobileUIEnabled(UIController.GetIsMobile());

		if (UIController.GetIsMobile()) {
			mainWeaponJoystick.OnJoystickReleased += MainWeaponJoystickReleased;
		} else {
			mobileUI.gameObject.SetActive(false);
		}
	}
	//TODO: mobile should still read other inputs here but redirect it
	private void Update() {
		if (EntityController.player == null ||
			EntityController.player.GetNetworker().GetIsDead()) return;

		CombatEntity player = EntityController.player;

		if (UIController.GetIsMobile()) {
			MobileOnlyUpdate(player);
		} else {
			PCOnlyUpdate(player);
		}

		Camera.main.transform.position = player.transform.position + cameraLocalPosition;
	}
	private void Awake() {
		instance = this;
	}

	#endregion
}
