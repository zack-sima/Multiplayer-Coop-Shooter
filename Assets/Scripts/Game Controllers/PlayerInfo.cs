using System.Collections;
using System.Collections.Generic;
using System.Data;
using Abilities;
using UnityEngine;


/// <summary>
/// Holds the player's info, such as ammunition and battery power; automatically handles reloads & regen.
/// Should be a singleton with UI controllers, etc.
/// </summary>

public class PlayerInfo : MonoBehaviour {

	[System.Serializable]
	public class TurretInfo {
		public Turret turret;
		public string turretName;
		public Vector3 localPositionOffset; //in addition to the anchor
	}
	[System.Serializable]
	public class HullInfo {
		public Hull hull;
		public string hullName;
		public Vector3 localPositionOffset;
	}

	#region Statics

	public static PlayerInfo instance;

	#endregion

	#region Prefabs

	//uses turret name to query for turret (fallback to first turret)
	[SerializeField] private List<HullInfo> hullPrefabs;
	[SerializeField] private List<TurretInfo> turretPrefabs;

	#endregion

	#region References

	#endregion

	#region Members

	//set by turret
	private int maxAmmo = 1;
	public int GetMaxAmmo() { return maxAmmo; }

	private float ammoReloadSpeed = 1f;

	private float ammoLeft = 0;
	public float GetAmmoLeft() { return Mathf.Clamp(ammoLeft, 0, maxAmmo); }
	public void ConsumeAmmo() { ammoLeft--; }

	//TODO: add
	//private int energyLeft = 30;

	#endregion

	#region Functions

	#region Abilities 

	//NOTE: only call on local player!

	//stores ability shit here!
	private float totalDmgDealt = 0;
	public float GetTotalDmgDealt() { return totalDmgDealt; }

	private List<(IAbility ability, bool isActivated)> abilities = new();

	public List<(IAbility, bool)> GetAbilityList() { return abilities; }

	public void IncrementDamageCharge(float dmgDone) { 
		totalDmgDealt += dmgDone;
		//Debug.Log(totalDmgDealt);
	}

	public void PushAbilityActivation(int index) { 
		abilities.PushAbilityActivation(index);
	}

	#endregion

	//NOTE: should only be called at start, but for debug is called by player
	public void TurretChanged(string newTurretName) {
		Turret turret = GetTurretPrefab(newTurretName).turret;

		maxAmmo = turret.GetMaxAmmo();
		ammoLeft = maxAmmo;

		ammoReloadSpeed = turret.GetAmmoRegenSpeed();
	}

	//the local player calls this as soon as it is fully instantiated; non-local entities
	//call this when the synced variable for turret name is changed
	public TurretInfo GetTurretPrefab(string turretName) {
		foreach (TurretInfo t in turretPrefabs) {
			if (t.turretName == turretName) return t;
		}
		return turretPrefabs[0];
	}
	public List<TurretInfo> GetTurrets() {
		return turretPrefabs;
	}
	public HullInfo GetHullPrefab(string hullName) {
		foreach (HullInfo h in hullPrefabs) {
			if (h.hullName == hullName) return h;
		}
		return hullPrefabs[0];
	}
	public List<HullInfo> GetHulls() {
		return hullPrefabs;
	}
	public string GetLocalPlayerName() {
		return PlayerPrefs.GetString("player_name");
	}
	public string GetLocalPlayerHullName() {
		if (PlayerPrefs.GetString("hull_name") == "") return "Spider";
		return PlayerPrefs.GetString("hull_name");
	}
	public string GetLocalPlayerTurretName() {
		if (PlayerPrefs.GetString("turret_name") == "") return "Autocannon";
		return PlayerPrefs.GetString("turret_name");
	}
	private void ReloadAmmoOnce(bool reloadRegardless = false) {
		if (ammoLeft < maxAmmo && (!HumanInputs.instance.GetPlayerShooting() || reloadRegardless))
			ammoLeft = Mathf.Min(maxAmmo, ammoLeft + Time.deltaTime * ammoReloadSpeed);
	}
	//called by overclock ability
	public void ReloadFaster() {
		for (int i = 0; i < 3; i++)
			ReloadAmmoOnce(reloadRegardless: true);
	}
	private void Update() {
		ReloadAmmoOnce();
		NetworkedEntity.playerInstance.SysTickAndAbilityHandler(abilities); // for ability Manager.
	}
	private void Start() {
		TurretChanged(GetLocalPlayerTurretName());
	}
	private void Awake() {
		instance = this;
	}

	#endregion

}
