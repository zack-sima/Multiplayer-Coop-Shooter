using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour {

	#region Statics

	public static MapController instance;

	#endregion

	#region References

	[SerializeField] private Transform spawnpointParent;
	public Transform GetSpawnpointParent() { return spawnpointParent; }

	//TODO: add more options for teams, etc
	[SerializeField] private Transform playerSpawnpoint;
	public Vector3 GetPlayerSpawnpoint() {
		Vector2 rc = Random.insideUnitCircle * 2f;
		return playerSpawnpoint.position + new Vector3(rc.x, 0, rc.y);
	}

	[SerializeField] private List<CapturePoint> capturablePoints;
	public List<CapturePoint> GetCapturePoints() { return capturablePoints; }

	[SerializeField] private List<Transform> blueSpawnpoints;
	[SerializeField] private List<Transform> redSpawnpoints;
	public Vector3 GetTeamSpawnpoint(int team) {
		team %= 2;
		if (team == 0) {
			if (blueSpawnpoints.Count == 0) return playerSpawnpoint.position;
			return blueSpawnpoints[Random.Range(0, blueSpawnpoints.Count)].position;
		} else {
			if (redSpawnpoints.Count == 0) return playerSpawnpoint.position;
			return redSpawnpoints[Random.Range(0, redSpawnpoints.Count)].position;
		}
	}

	#endregion

	#region Functions

	private void Awake() {
		instance = this;
	}
	private void Start() {
		//disable capture points if not in mode
		if (!PlayerInfo.GetIsPointCap()) {
			foreach (CapturePoint p in capturablePoints) {
				p.gameObject.SetActive(false);
			}
		}
	}

	#endregion
}
