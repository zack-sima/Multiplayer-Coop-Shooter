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

	#endregion

	#region Functions

	private void Awake() {
		instance = this;
	}

	#endregion
}
