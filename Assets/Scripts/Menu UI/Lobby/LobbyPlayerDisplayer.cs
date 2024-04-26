using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyPlayerDisplayer : MonoBehaviour {

	#region Statics & Consts

	#endregion

	#region References

	[SerializeField] private TeamMaterialManager teamMaterials;

	[SerializeField] private Transform nameDisplayParent;
	[SerializeField] private TMP_Text playerNameText, playerNameGhostText;
	public void SetPlayerNameText(string nameText) {
		if (playerNameText.text == nameText) return;
		playerNameText.text = nameText;
		playerNameGhostText.text = nameText;
	}

	[SerializeField] private Image hostIcon;
	public void SetHostIcon(bool isOn) {
		if (hostIcon.enabled == isOn) return;
		hostIcon.enabled = isOn;
	}

	//where hull & turret prefabs are stored
	[SerializeField] private PlayerInfo playerInfoRef;

	#endregion

	#region Members

	//NOTE: whenever player calls this function, check if the string was changed.
	//If so, spawn in the corresponding correct hull/turret
	private string turretName = "", hullName = "";

	//storing hulls/turrets
	List<PlayerInfo.TurretInfo> spawnedTurrets = new();
	List<PlayerInfo.HullInfo> spawnedHulls = new();
	private Hull hull = null;
	private Turret turret = null;

	bool initialized = false;

	#endregion

	#region Functions

	//called externally only by LobbyUI
	public void Initialize() {
		initialized = true;
	}
	//changes the turret if different! (NOTE: these functions are called every frame)
	public void SetTurret(string turret) {
		if (!initialized) return;
		if (turretName == turret || turret == "") return;
		turretName = turret;

		//change the turret appearance
		TurretChanged(turret);
	}
	public void SetHull(string hull) {
		if (!initialized) return;
		if (hullName == hull || hull == "") return;
		hullName = hull;

		//change the hull appearance
		HullChanged(hull);
	}
	public void SetTurretRotation(float rotation) {
		if (turret != null) {
			turret.transform.eulerAngles = new Vector3(0, rotation + 180, 0);
		}
	}
	public void SetHullRotation(float rotation) {
		if (hull != null) {
			hull.transform.eulerAngles = new Vector3(0, rotation + 180, 0);
		}
	}
	private void HullChanged(string newHullName) {
		foreach (PlayerInfo.HullInfo h in spawnedHulls) {
			if (h.hullName == newHullName) {
				h.hull.gameObject.SetActive(true);
				hull = h.hull;
			} else {
				h.hull.gameObject.SetActive(false);
			}
		}
		//reset offsets
		foreach (PlayerInfo.TurretInfo t in spawnedTurrets) {
			if (t.turretName == turretName) {
				turret.transform.position = hull.GetTurretAnchorPosition() + t.localPositionOffset;
				break;
			}
		}
		nameDisplayParent.localPosition = new Vector3(0, 3 + hull.GetTurretAnchorPosition().y, -0.15f);
		hull.GetAnimator().SetTeamMaterial(teamMaterials.GetTeamColor(0));
	}
	private void TurretChanged(string newTurretName) {
		foreach (PlayerInfo.TurretInfo t in spawnedTurrets) {
			if (t.turretName == newTurretName) {
				t.turret.gameObject.SetActive(true);
				t.turret.gameObject.transform.position = hull.GetTurretAnchorPosition() + t.localPositionOffset;
				turret = t.turret;
			} else {
				t.turret.gameObject.SetActive(false);
			}
		}
		turret.GetAnimator().SetTeamMaterial(teamMaterials.GetTeamColor(0));
	}

	private void Awake() {
		//spawn in all the required hulls/turrets (init all to disabled)
		List<PlayerInfo.TurretInfo> turrets = playerInfoRef.GetTurrets();
		List<PlayerInfo.HullInfo> hulls = playerInfoRef.GetHulls();

		foreach (PlayerInfo.HullInfo h in hulls) {
			GameObject g = Instantiate(h.hull.gameObject, transform);

			//set hull rotation
			g.transform.SetLocalPositionAndRotation(h.localPositionOffset, Quaternion.Euler(0, 20, 0));

			g.SetActive(false);

			PlayerInfo.HullInfo localInfo = new() {
				hull = g.GetComponent<Hull>(), //NOTE: actually a reference, not a prefab 
				hullName = h.hullName
			};
			spawnedHulls.Add(localInfo);
		}

		foreach (PlayerInfo.TurretInfo t in turrets) {
			GameObject g = Instantiate(t.turret.gameObject, transform);

			//position will be overridden
			g.transform.SetPositionAndRotation(transform.position,
				Quaternion.Euler(0, 160, 0));

			g.SetActive(false);

			PlayerInfo.TurretInfo localInfo = new() {
				turret = g.GetComponent<Turret>(),
				turretName = t.turretName,
				localPositionOffset = t.localPositionOffset
			};
			spawnedTurrets.Add(localInfo);
		}

		hull = spawnedHulls[0].hull;
		turret = spawnedTurrets[0].turret;
	}

	private void Update() { }

	#endregion

}
