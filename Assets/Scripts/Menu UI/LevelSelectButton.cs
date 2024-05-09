using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectButton : MonoBehaviour {

	[SerializeField] private string mapName;
	[SerializeField] private bool isSolo;
	[SerializeField] private bool isRapid;
	[SerializeField] private bool isComp;

	public void SelectMap() {
		LevelSelectManager.instance.SetMap(mapName, isSolo, isRapid, isComp);
	}
}
