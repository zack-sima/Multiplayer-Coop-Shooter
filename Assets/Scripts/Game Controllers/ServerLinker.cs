using System.Collections;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class ServerLinker : MonoBehaviour {

	#region Consts & Statics

	public static ServerLinker instance;

	public const int LOBBY_SCENE = 0;

	#endregion

	[SerializeField] FusionBootstrap gameBootstrap, lobbyBootstrap;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
			return;
		}
		instance = this;
		DontDestroyOnLoad(gameObject);
	}
	public async Task LoadSceneAsync(int sceneIndex) {
		// Wrap the AsyncOperation in a Task
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		var asyncOp = SceneManager.LoadSceneAsync(sceneIndex);
		asyncOp.completed += _ => tcs.SetResult(true);
		await tcs.Task;
	}
	//NOTE: starting lobby will stay in the menu scene (should be 0) 
	public async void StartLobby(string roomId = "") {
		// Make sure FusionBootstrap and NetworkRunner are ready
		if (lobbyBootstrap != null) {
			// Change to the gameplay scene (if not currently there)
			if (SceneManager.GetActiveScene().buildIndex != LOBBY_SCENE)
				await LoadSceneAsync(LOBBY_SCENE);

			// After scene is loaded, start the lobby as shared
			lobbyBootstrap.DefaultRoomName = roomId;
			lobbyBootstrap.StartSharedClient(); // Adjust the client count as necessary
		}
	}
	public async void StartShared(int sceneIndex, string roomId = "") {
		// Make sure FusionBootstrap and NetworkRunner are ready
		if (gameBootstrap != null) {
			// Change to the gameplay scene
			await LoadSceneAsync(sceneIndex);

			// After scene is loaded, start the network game as shared
			gameBootstrap.DefaultRoomName = roomId;
			gameBootstrap.StartSharedClient(); // Adjust the client count as necessary
		}
	}

	public async void StartSinglePlayer(int sceneIndex) {
		if (gameBootstrap != null) {
			// Change to the gameplay scene
			await LoadSceneAsync(sceneIndex);

			// Start the game in single-player mode
			gameBootstrap.StartSinglePlayer();
		}
	}
	public void StopLobby() {
		if (lobbyBootstrap != null) {
			//NOTE: ShutdownAll was modified with this argument so scene change isn't forced
			lobbyBootstrap.ShutdownAll(changeScene: false);
		}
	}
	public void StopGame() {
		// This function stops the game and returns to the lobby/menu scene
		if (gameBootstrap != null) {
			gameBootstrap.ShutdownAll(); // This shuts down the NetworkRunner instances
		}

		//TODO: if player was in a lobby before starting game, return to that lobby with start lobby
		if (SceneManager.GetActiveScene().buildIndex != LOBBY_SCENE) {
			Debug.Log("loading scene...");
			SceneManager.LoadSceneAsync(LOBBY_SCENE);
		}
	}
}
