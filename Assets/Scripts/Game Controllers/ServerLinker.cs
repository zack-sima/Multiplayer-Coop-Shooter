using System.Collections;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class ServerLinker : MonoBehaviour {

	public static ServerLinker instance;

	private FusionBootstrap bootstrap;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
			return;
		}
		instance = this;
		DontDestroyOnLoad(gameObject);

		// Find FusionBootstrap in the scene
		bootstrap = FindObjectOfType<FusionBootstrap>();
		if (bootstrap == null) {
			Debug.LogError("FusionBootstrap instance not found in the scene. Make sure it's added and properly configured.");
		}
	}
	private void Update() {
	}
	public async Task LoadSceneAsync(int sceneIndex) {
		// Wrap the AsyncOperation in a Task
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		var asyncOp = SceneManager.LoadSceneAsync(sceneIndex);
		asyncOp.completed += _ => tcs.SetResult(true);
		await tcs.Task;
		// Additional code here after scene has loaded
	}
	public async void StartShared(int sceneIndex, string roomId = "") {
		// Make sure FusionBootstrap and NetworkRunner are ready
		if (bootstrap != null) {
			// Change to the gameplay scene
			await LoadSceneAsync(sceneIndex);

			// After scene is loaded, start the network game as shared
			bootstrap.DefaultRoomName = roomId;
			bootstrap.StartSharedClient(); // Adjust the client count as necessary
		}
	}

	public async void StartSinglePlayer(int sceneIndex) {
		if (bootstrap != null) {
			// Change to the gameplay scene
			await LoadSceneAsync(sceneIndex);

			// Start the game in single-player mode
			bootstrap.StartSinglePlayer();
		}
	}

	public void StopGame() {
		// This function stops the game and returns to the menu scene (index 0)
		if (bootstrap != null) {
			bootstrap.ShutdownAll(); // This shuts down the NetworkRunner instances
		}

		// Load the menu scene
		SceneManager.LoadSceneAsync(0);
	}
}
