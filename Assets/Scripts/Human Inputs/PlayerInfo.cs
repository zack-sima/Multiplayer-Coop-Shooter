using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the player's info, such as ammunition and battery power; automatically handles reloads & regen.
/// Should be a singleton with UI controllers, etc.
/// </summary>

public class PlayerInfo : MonoBehaviour {

	#region Statics

	public static PlayerInfo instance;

	#endregion

	#region Members

	//TODO: actually set this from shop, init, etc
	private int maxAmmo = 50;
	public int GetMaxAmmo() { return maxAmmo; }

	private float ammoLeft = 0;
	public float GetAmmoLeft() { return Mathf.Clamp(ammoLeft, 0, maxAmmo); }
	public void ConsumeAmmo() { ammoLeft--; }

	//TODO: add
	private int energyLeft = 30;

	#endregion

	#region Functions

	private void Update() {
		if (ammoLeft < maxAmmo && !HumanInputs.instance.GetPlayerShooting())
			ammoLeft = Mathf.Min(maxAmmo, ammoLeft + Time.deltaTime * 7f);
	}
	private void Start() {
		ammoLeft = maxAmmo;
	}
	private void Awake() {
		instance = this;
	}

	#endregion

}
