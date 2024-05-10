using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined {
	public static PlayerSpawner instance;

	public GameObject PlayerPrefab;
	public GameObject StatsSyncerPrefab, EnemySpawnerPrefab;

	private int playerTeam = -1;

	private void GeneratePlayerTeam() {
		int playerId = Runner.LocalPlayer.PlayerId;
		Debug.Log(playerId);

		if (playerId < 0) playerTeam = 0;
		else playerTeam = (playerId + 1) % 2;
	}
	public int GetPlayerTeam() {
		return playerTeam;
	}

	public void PlayerJoined(PlayerRef player) {
		if (player == Runner.LocalPlayer) {
			GeneratePlayerTeam();
			Vector3 spawnpoint = MapController.instance.GetPlayerSpawnpoint();

			if (PlayerInfo.GetIsPVP()) {
				spawnpoint = MapController.instance.GetTeamSpawnpoint(playerTeam);
			}

			Runner.Spawn(PlayerPrefab, spawnpoint, Quaternion.identity);

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
	void Awake() {
		instance = this;
	}
}