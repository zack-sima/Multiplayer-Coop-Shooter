using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectButton : MonoBehaviour {

	[SerializeField] private string mapName;
	[SerializeField] private bool isSolo;

	public void SelectMap() {
		LevelSelectManager.instance.SetMap(mapName, isSolo);
	}
}
