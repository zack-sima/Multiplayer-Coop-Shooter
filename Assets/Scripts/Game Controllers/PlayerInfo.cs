
using System.Collections.Generic;
using UnityEngine;
using Handlers;


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

	public static bool GetIsPVP() {
		switch ((MenuManager.GameMode)PlayerPrefs.GetInt("game_mode")) {
			case MenuManager.GameMode.Singleplayer:
			case MenuManager.GameMode.Coop:
				return false;
			case MenuManager.GameMode.PointCap:
			case MenuManager.GameMode.Comp:
				return true;
			default:
				return false;
		}
	}
	public static bool GetIsPointCap() {
		return (MenuManager.GameMode)PlayerPrefs.GetInt("game_mode") == MenuManager.GameMode.PointCap;
	}

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

	#endregion

	#region Functions

	#region Upgrades

	//NOTE: only call on local player!

	//?======================| Abilities |======================?//
	private float totalDmgDealt = 0;
	public float GetTotalDmgDealt() { return totalDmgDealt; }

	public void IncrementDamageCharge(float dmgDone) {
		totalDmgDealt += dmgDone;
		//Debug.Log(totalDmgDealt);
	}

	public void PushAbilityActivation(int index) {
		NetworkedEntity.playerInstance.GetAbilityList().PushAbilityActivation(index);
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

	private bool TryGetAmmoRegen(out float regenSpeed) {
		regenSpeed = 1f;
		if (NetworkedEntity.playerInstance != null &&
			NetworkedEntity.playerInstance.GetCombatEntity() != null &&
			NetworkedEntity.playerInstance.GetCombatEntity().GetTurret() != null) {
			regenSpeed = NetworkedEntity.playerInstance.GetCombatEntity().GetTurret().GetAmmoRegenSpeed();
			return true;
		}
		return false;
	}
	//called by overclock ability
	public void ReloadFaster() {
		for (int i = 0; i < 3; i++)
			ReloadAmmoOnce(reloadRegardless: true);
	}
	private void Update() {
		ReloadAmmoOnce();
		if (TryGetAmmoRegen(out float regenSpeed)) {
			ammoReloadSpeed = regenSpeed;
		}

	}
	private void Start() {
		TurretChanged(GetLocalPlayerTurretName());

		//cost a hull & turret!
		PersistentDict.SetInt("repair_uses_" + PlayerPrefs.GetString("hull_name"),
			PersistentDict.GetInt("repair_uses_" + PlayerPrefs.GetString("hull_name")) + 1);
		PersistentDict.SetInt("repair_uses_" + PlayerPrefs.GetString("turret_name"),
			PersistentDict.GetInt("repair_uses_" + PlayerPrefs.GetString("turret_name")) + 1);
	}
	private void Awake() {
		instance = this;
	}

	#endregion

}
