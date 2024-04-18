using System;
using System.Collections;
using System.Collections.Generic;

namespace Lobby {
    public static class LobbyEventsHandler {
        /* Player Spawned Event */
        public static event Action OnJoinLobby;

        public static void InvokePlayerJoinLobby() {
            OnJoinLobby?.Invoke();
        }
        
        /* Lobby Players Event */
        public static event Action<LobbyPlayer> OnPlayerSpawn;

        public static void RaisePlayerSpawn(LobbyPlayer lobbyPlayer) {
            OnPlayerSpawn?.Invoke(lobbyPlayer);
        }
        
        public static event Action<LobbyPlayer> OnPlayerUpdate;

        public static void RaisePlayerUpdate(LobbyPlayer lobbyPlayer) {
            OnPlayerUpdate?.Invoke(lobbyPlayer);
        }
        
        public static event Action<LobbyPlayer> OnPlayerQuit;

        public static void RaisePlayerQuit(LobbyPlayer lobbyPlayer) {
            OnPlayerQuit?.Invoke(lobbyPlayer);
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