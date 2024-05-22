using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave Data")]
public class WaveData : ScriptableObject {

	[System.Serializable]
	public class WaveInfo {
		public int waveNum;
		public List<int> bogoEnemyPrefabs;
	}

	[SerializeField] private List<GameObject> enemyPrefabs;
	[SerializeField] private List<int> enemyCosts;
	[SerializeField] private List<WaveInfo> waves;

	//GAME SETTINGS
	[SerializeField] private float allocCostMultiplier; //some maps should be easier/harder, etc
	[SerializeField] private int additionalSpawnMoney; //how much money to add to each wave spawner
	[SerializeField] private float enemyRampupFactor; //exponent addition
	[SerializeField] private float enemyIntervalFactor; //<1 = less time between spawns

	private Dictionary<int, WaveInfo> waveDict;

	private void OnEnable() {
		waveDict = new();
		foreach (WaveInfo w in waves) {
			if (waveDict.ContainsKey(w.waveNum)) continue;
			waveDict.Add(w.waveNum, w);
		}
	}
	public float GetIntervalFactor() {
		return enemyIntervalFactor;
	}
	public float GetRampFactor() {
		return enemyRampupFactor;
	}
	public float GetSpendingMultiplier() {
		return allocCostMultiplier;
	}
	public int GetMoney() {
		return additionalSpawnMoney;
	}
	public List<GameObject> GetBogoSpawnPrefabs(int wave) {
		if (!waveDict.ContainsKey(wave)) return null;

		List<GameObject> prefabs = new();

		foreach (int i in waveDict[wave].bogoEnemyPrefabs)
			prefabs.Add(enemyPrefabs[i]);

		if (prefabs.Count == 0) return null;
		return prefabs;
	}
	public List<int> GetBogoSpawnCosts(int wave) {
		if (!waveDict.ContainsKey(wave)) return null;

		List<int> costs = new();

		foreach (int i in waveDict[wave].bogoEnemyPrefabs)
			costs.Add(enemyCosts[i]);

		if (costs.Count == 0) return null;
		return costs;
	}
}
