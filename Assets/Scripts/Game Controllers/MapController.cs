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

	#endregion

	#region Functions

	private void Awake() {
		instance = this;
	}

	#endregion
}
