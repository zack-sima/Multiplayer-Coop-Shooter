using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined {
	public GameObject PlayerPrefab;
	public GameObject StatsSyncerPrefab, EnemySpawnerPrefab;

	public void PlayerJoined(PlayerRef player) {
		if (player == Runner.LocalPlayer) {

			//Debug.Log(PlayerPrefab.name);
			Runner.Spawn(PlayerPrefab, MapController.instance.GetPlayerSpawnpoint(), Quaternion.identity);

			//spawn stats syncer
			if (Runner.IsSharedModeMasterClient || Runner.IsSinglePlayer) {
				if (GameStatsSyncer.instance == null)
					Runner.Spawn(StatsSyncerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
				if (EnemySpawner.instance == null)
					Runner.Spawn(EnemySpawnerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
			}
		}
	}
	//callback from NetworkEvents
	public void GameDisconnected() {
		ServerLinker.instance.StopGame();
	}
}