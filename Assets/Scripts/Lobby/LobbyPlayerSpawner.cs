using Fusion;
using UnityEngine;

public class LobbyPlayerSpawner : SimulationBehaviour, IPlayerJoined {
	public GameObject PlayerPrefab;
	public GameObject LobbyStatsSyncerPrefab;

	public void PlayerJoined(PlayerRef player) {
		if (player == Runner.LocalPlayer) {
			Runner.Spawn(PlayerPrefab, new Vector3(0, 0, 0), Quaternion.identity);

			//spawn lobby syncer
			if (Runner.IsSharedModeMasterClient || Runner.IsSinglePlayer) {
				if (GameStatsSyncer.instance == null)
					Runner.Spawn(LobbyStatsSyncerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
			}
		}
	}
}