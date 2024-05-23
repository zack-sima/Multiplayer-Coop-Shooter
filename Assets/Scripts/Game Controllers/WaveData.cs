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
	[System.Serializable]
	public class EnemyPrefabList {
		public List<GameObject> prefabs;
	}
	[System.Serializable]
	public class EnemyCostList {
		public List<int> costs;
	}

	[SerializeField] private List<EnemyPrefabList> enemyPrefabs;
	[SerializeField] private List<EnemyCostList> enemyCosts;

	//for every 5th wave
	[SerializeField] private List<EnemyPrefabList> enemyBossPrefabs;
	[SerializeField] private List<EnemyCostList> enemyBossCosts;

	[SerializeField] private List<WaveInfo> waves;

	//GAME SETTINGS
	[SerializeField] private float allocCostMultiplier; //some maps should be easier/harder, etc
	[SerializeField] private int additionalSpawnMoney; //how much money to add to each wave spawner
	[SerializeField] private float enemyRampupFactor; //exponent addition
	[SerializeField] private float enemyIntervalFactor; //<1 = less time between spawns
	[SerializeField] private int rampUpWaveNumber;

	private Dictionary<int, WaveInfo> waveDict;

	private int GetTier(int wave) {
		int tier = wave / rampUpWaveNumber;

		if (tier >= enemyPrefabs.Count) tier = enemyPrefabs.Count - 1;
		return tier;
	}
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
	public List<GameObject> GetBogoSpawnPrefabs(int wave, bool isBoss) {
		if (!waveDict.ContainsKey(wave)) return isBoss ? enemyBossPrefabs[GetTier(wave)].prefabs :
				enemyPrefabs[GetTier(wave)].prefabs;

		List<GameObject> prefabs = new();

		foreach (int i in waveDict[wave].bogoEnemyPrefabs)
			prefabs.Add(isBoss ? enemyBossPrefabs[GetTier(wave)].prefabs[i] : enemyPrefabs[GetTier(wave)].prefabs[i]);

		if (prefabs.Count == 0) return null;
		return prefabs;
	}
	public List<int> GetBogoSpawnCosts(int wave, bool isBoss) {
		if (!waveDict.ContainsKey(wave)) return isBoss ? enemyBossCosts[GetTier(wave)].costs : enemyCosts[GetTier(wave)].costs;

		List<int> costs = new();

		foreach (int i in waveDict[wave].bogoEnemyPrefabs)
			costs.Add(isBoss ? enemyBossCosts[GetTier(wave)].costs[i] : enemyCosts[GetTier(wave)].costs[i]);

		if (costs.Count == 0) return null;
		return costs;
	}
}
