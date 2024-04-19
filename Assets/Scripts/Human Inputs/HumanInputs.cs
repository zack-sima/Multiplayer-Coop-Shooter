using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class HumanInputs : MonoBehaviour {

	#region Constants & Statics

	public static HumanInputs instance;

	private const float AIM_DRAG_THRESHOLD = 0.5f;
	private const float DEFAULT_LOB_DISTANCE = 5f;
	private const float MAX_LOB_DISTANCE = 13.5f;
	private const float FULL_AUTO_DELAY = 0.15f;

	#endregion

	#region References

	[SerializeField] private MobileJoystick movementJoystick;
	[SerializeField] private MobileJoystick mainWeaponJoystick;

	#endregion

	#region Members

	private Vector3 cameraLocalPosition = Vector3.zero;

	private bool playerShooting = false;
	public bool GetPlayerShooting() { return playerShooting; }

	//set to true until fire released or ammo is full
	private bool stopAutoShooting = false;

	//mobile semi auto doesn't shoot if dragged for long enough
	private float mainShootButtonDownTime = 0f;

	//force wait of ~0.15s before shooting full auto
	private float fullAutoDelay = 0f;

	//before button release, magnitude of attack joystick
	private float lastMainWeaponJoystickMagnitude = 0f;

	//lobbing distance (resets by default)
	private float mobileLobDistance = DEFAULT_LOB_DISTANCE;
	public float GetMobileLobDistance() { if (!GetIsMobileAiming()) return DEFAULT_LOB_DISTANCE; return mobileLobDistance; }

	private Vector3 mouseWorldPos = Vector3.zero;
	public Vector3 GetMouseWorldPos() { return mouseWorldPos; }

	//PC tank drive
	private float tankRotation = 0f;

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
			float mag = mainWeaponJoystick.GetJoystickMagnitude();
			float targetRotationY = -mainWeaponJoystick.GetJoystickAngle() * Mathf.Rad2Deg + 90f;

			if (player.GetTurret().GetIsFullAuto()) {
				//only allow shooting if turret is oriented
				bool onTarget = Quaternion.Angle(player.GetTurret().transform.rotation,
					Quaternion.Euler(0, targetRotationY, 0)) < 10f;

				if (fullAutoDelay > 0 && (mag < AIM_DRAG_THRESHOLD || !onTarget)) {
					fullAutoDelay -= Time.deltaTime;
				} else if (PlayerInfo.instance.GetAmmoLeft() > 1 && !stopAutoShooting) {
					playerShooting = true;
					player.TryFireMainWeapon();
				} else {
					if (PlayerInfo.instance.GetAmmoLeft() == PlayerInfo.instance.GetMaxAmmo()) {
						stopAutoShooting = false;
					} else {
						stopAutoShooting = true;
						playerShooting = false;
					}
				}
			}
			if (mag > AIM_DRAG_THRESHOLD) {
				mobileLobDistance = (mag - AIM_DRAG_THRESHOLD) * MAX_LOB_DISTANCE * (1f + AIM_DRAG_THRESHOLD);
				player.MaintainTurretRotation();
				player.GetTurret().SnapToTargetRotation(targetRotationY, true);
			}
			lastMainWeaponJoystickMagnitude = mainWeaponJoystick.GetJoystickMagnitude();
		} else {
			stopAutoShooting = false;
			playerShooting = false;
			if (player.GetTurret().GetIsFullAuto()) {
				fullAutoDelay = FULL_AUTO_DELAY;
			}
		}
	}
	//for semi-auto release fire
	public void MainWeaponJoystickReleased() {
		if (EntityController.player == null ||
			EntityController.player.GetNetworker().GetIsDead()) return;

		CombatEntity player = EntityController.player;

		if (player.GetTurret().GetIsFullAuto()) {
			//if player tapped shoot a single bullet
			if (UIController.GetIsMobile()) {
				if (Time.time - mainShootButtonDownTime < FULL_AUTO_DELAY) {
					player.TryFireMainWeapon();
				}
			}
			return;
		}

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
			if (PlayerInfo.instance.GetAmmoLeft() > 1 && !stopAutoShooting) {
				playerShooting = true;
				player.TryFireMainWeapon();
			} else if (PlayerInfo.instance.GetAmmoLeft() == PlayerInfo.instance.GetMaxAmmo()) {
				stopAutoShooting = false;
			} else {
				stopAutoShooting = true;
				playerShooting = false;
			}
		} else {
			stopAutoShooting = false;
			playerShooting = false;
		}

		//point to mouse
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit hit, 100, LayerMask.GetMask("Ground"))) {
			mouseWorldPos = hit.point;
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

		//NOTE: PC-only tank drive
		if (player.GetNetworker().GetHullName() == "Tank") {
			float moveMagnitude = 0f;
			float moveDir = 0f;
			if (Input.GetKey(KeyCode.W)) {
				moveMagnitude++;
			}
			//if (Input.GetKey(KeyCode.S)) { //TODO: make driving backwards possible? Is it necessary?
			//	moveMagnitude--;
			//}
			if (Input.GetKey(KeyCode.A)) {
				moveDir += Time.deltaTime * 150f;
			}
			if (Input.GetKey(KeyCode.D)) {
				moveDir -= Time.deltaTime * 150f;
			}
			if (moveDir != 0 && moveMagnitude == 0) moveMagnitude = 0.05f;

			tankRotation += moveDir;
			player.GetHull().Move(moveMagnitude * new Vector3(
				Mathf.Cos((tankRotation + 90) * Mathf.Deg2Rad), 0,
				Mathf.Sin((tankRotation + 90) * Mathf.Deg2Rad)
			));
		} else {
			player.GetHull().Move(moveVector);
		}
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
