using System;
using System.Collections;
using System.Collections.Generic;

namespace Lobby {
    public static class LobbyEventsHandler {
        /* Player Spawned Event */
        public static event Action OnPlayerSpawn;

        public static void InvokePlayerSpawn() {
            OnPlayerSpawn?.Invoke();
        }
        
        /* Lobby Players Event */
        public static event Action<LobbyPlayer> OnPlayerUpdate;

        public static void UpdateLobbyPlayer(LobbyPlayer lobbyPlayer) {
            OnPlayerUpdate?.Invoke(lobbyPlayer);
        }

        /* LobbyId Change Events */
        public static event Action<string> OnLobbyIdChanged;

        public static void RaiseLobbyIdChanged(string id) {
            OnLobbyIdChanged?.Invoke(id);
        }

        /* Map Change Events */
        public static event Action<string> OnMapChanged;

        public static void RaiseMapChanged(string map) {
            OnMapChanged?.Invoke(map);
        }

        /* Wave Change Events */
        public static event Action<int> OnWaveChanged;

        public static void RaiseWaveChanged(int waveNum) {
            OnWaveChanged?.Invoke(waveNum);
            // Debug.Log("Invoked wave change");
        }
    }
}