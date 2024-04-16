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

	FusionBootstrap bootstrap = null;

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
	private bool TryFindBootstrap() {
		bootstrap = null;
		bootstrap = FindObjectOfType<FusionBootstrap>();
		return bootstrap != null;
	}
	//NOTE: starting lobby will stay in the menu scene (should be 0) 
	public async void StartLobby(string roomId = "") {
		// Change to the gameplay scene (if not currently there)
		if (SceneManager.GetActiveScene().buildIndex != LOBBY_SCENE)
			await LoadSceneAsync(LOBBY_SCENE);

		// After scene is loaded, start the lobby as shared
		if (!TryFindBootstrap()) return;

		bootstrap.DefaultRoomName = roomId;
		bootstrap.StartSharedClient(); // Adjust the client count as necessary
	}
	public async void StartShared(int sceneIndex, string roomId = "") {
		// Change to the gameplay scene
		await LoadSceneAsync(sceneIndex);

		// After scene is loaded, start the network game as shared
		if (!TryFindBootstrap()) return;

		bootstrap.DefaultRoomName = roomId;
		bootstrap.StartSharedClient(); // Adjust the client count as necessary
	}

	public async void StartSinglePlayer(int sceneIndex) {
		if (bootstrap != null) {
			// Change to the gameplay scene
			await LoadSceneAsync(sceneIndex);

			if (!TryFindBootstrap()) return;

			// Start the game in single-player mode
			bootstrap.StartSinglePlayer();
		}
	}
	public void StopLobby() {
		//NOTE: ShutdownAll was modified with this argument so scene change isn't forced
		if (!TryFindBootstrap()) return;

		bootstrap.ShutdownAll(changeScene: false);
	}
	public void StopGame() {
		try {
			bootstrap.ShutdownAll(changeScene: true); // This shuts down the NetworkRunner instances
		} catch { }
	}
}
