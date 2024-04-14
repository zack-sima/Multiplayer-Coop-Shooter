using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanInputs : MonoBehaviour {

	#region Constants & Statics

	public static HumanInputs instance;

	private const float AIM_DRAG_THRESHOLD = 0.33f;

	#endregion

	#region References

	[SerializeField] private MobileJoystick movementJoystick;
	[SerializeField] private MobileJoystick mainWeaponJoystick;

	#endregion

	#region Members

	private Vector3 cameraLocalPosition = Vector3.zero;

	private bool playerShooting = false;
	public bool GetPlayerShooting() { return playerShooting; }

	//mobile semi auto doesn't shoot if dragged for long enough
	private float mainShootButtonDownTime = 0f;

	//before button release, magnitude of attack joystick
	private float lastMainWeaponJoystickMagnitude = 0f;

	#endregion

	#region Functions

	#region Mobile

	/// <summary>
	/// Returns whether the main weapon joystick is down or not
	/// </summary>

	public bool GetIsMobileAiming() {
		return mainWeaponJoystick.GetButtonIsDown() &&
			mainWeaponJoystick.GetJoystickMagnitude() > AIM_DRAG_THRESHOLD;
	}
	public bool GetJoystickThresholdIsAiming() {
		return lastMainWeaponJoystickMagnitude > AIM_DRAG_THRESHOLD;
	}

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
			if (player.GetTurret().GetIsFullAuto()) {
				playerShooting = true;
				player.TryFireMainWeapon();
			}

			float mag = mainWeaponJoystick.GetJoystickMagnitude();
			if (mag > AIM_DRAG_THRESHOLD) {
				player.MaintainTurretRotation();
				player.GetTurret().SetTargetTurretRotation(
					-mainWeaponJoystick.GetJoystickAngle() * Mathf.Rad2Deg + 90f);
			}
			lastMainWeaponJoystickMagnitude = mainWeaponJoystick.GetJoystickMagnitude();
		} else {
			playerShooting = false;
		}
	}
	//for semi-auto release fire
	public void MainWeaponJoystickReleased() {
		if (EntityController.player == null ||
			EntityController.player.GetNetworker().GetIsDead()) return;

		CombatEntity player = EntityController.player;

		if (player.GetTurret().GetIsFullAuto()) return;

		if (UIController.GetIsMobile() && Time.time - mainShootButtonDownTime > 0.5f &&
			!GetJoystickThresholdIsAiming()) return;

		player.TryFireMainWeapon();
	}
	public void MainWeaponJoystickDown() {
		mainShootButtonDownTime = Time.time;
	}

	#endregion

	private void PCOnlyUpdate(CombatEntity player) {

#if UNITY_EDITOR //NOTE: for testing, temporary turret switching
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			player.GetNetworker().SetTurretName(
				PlayerInfo.instance.GetTurrets()[0].turretName
			);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			player.GetNetworker().SetTurretName(
				PlayerInfo.instance.GetTurrets()[1].turretName
			);
		}
#endif
		if (Input.GetKeyDown(KeyCode.Escape)) {
			UIController.instance.ToggleOptions();
		}
		if (Input.GetMouseButtonUp(0)) {
			MainWeaponJoystickReleased();
		}
		if (Input.GetMouseButton(0) && player.GetTurret().GetIsFullAuto()) {
			playerShooting = true;
			player.TryFireMainWeapon();
		} else {
			playerShooting = false;
		}

		//point to mouse
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit hit, 100, ~LayerMask.GetMask("In Game UI"))) {
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
		UIController.instance.SetPCUIEnabled(!UIController.GetIsMobile());

		if (UIController.GetIsMobile()) {
			mainWeaponJoystick.OnJoystickReleased += MainWeaponJoystickReleased;
			mainWeaponJoystick.OnJoystickPressed += MainWeaponJoystickDown;
		}
	}
	//TODO: mobile should still read other inputs here but redirect it
	private void Update() {
		if (EntityController.player == null ||
			EntityController.player.GetNetworker().GetIsDead()) return;

		CombatEntity player = EntityController.player;

		if (!UIController.instance.InOptions()) {
			if (UIController.GetIsMobile()) {
				MobileOnlyUpdate(player);
			} else {
				PCOnlyUpdate(player);
			}
		}

		Camera.main.transform.position = player.transform.position + cameraLocalPosition;
	}
	private void Awake() {
		instance = this;
	}

	#endregion
}
