using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectButton : MonoBehaviour {

	[SerializeField] private string mapName;
	[SerializeField] private bool isSolo;
	[SerializeField] private bool isRapid;

	public void SelectMap() {
		LevelSelectManager.instance.SetMap(mapName, isSolo, isRapid);
	}
}
