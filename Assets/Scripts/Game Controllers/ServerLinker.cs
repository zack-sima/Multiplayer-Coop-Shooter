using System.Collections;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class ServerLinker : MonoBehaviour {

	#region Consts & Statics

	public static ServerLinker instance;

	#endregion

	#region Members

	private bool isInLobby = false;
	public bool GetIsInLobby() { return isInLobby; }
	public void SetIsInLobby(bool inLobby) {
		isInLobby = inLobby;
		if (LobbyUI.instance != null) LobbyUI.instance.InLobbyUpdated();
	}

	//world time fetch
	private bool fetchedWorldTime = false;
	public bool GetWorldTimeFetched() { return fetchedWorldTime; }
	public void SetWorldTimeFetched() { fetchedWorldTime = true; }
	public void SetWorldTimeUnfetched() { fetchedWorldTime = false; }

	//prevent audio clipping
	private bool gameStopped = false;
	public bool GetGameIsStopped() { return gameStopped; }

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
	//NOTE: if isJoining but player is master client, leave lobby immediately
	public async void StartLobby(string roomId, bool isJoining) {
		// Change to the gameplay scene (if not currently there)
		if (SceneManager.GetActiveScene().buildIndex != 0)
			await LoadSceneAsync(0);

		// After scene is loaded, start the lobby as shared
		if (!TryFindBootstrap()) return;

		bootstrap.DefaultRoomName = roomId;
		bootstrap.StartSharedClient(); // Adjust the client count as necessary

		PlayerPrefs.SetInt("is_lobby_client", isJoining ? 1 : 0);
	}
	public async void StartShared(int sceneIndex, string roomId = "") {
		// Change to the gameplay scene
		await LoadSceneAsync(sceneIndex);

		// After scene is loaded, start the network game as shared
		if (!TryFindBootstrap()) return;

		bootstrap.DefaultRoomName = roomId;
		bootstrap.StartSharedClient(); // Adjust the client count as necessary

		SetIsInLobby(false);
	}

	public async void StartSinglePlayer(int sceneIndex) {
		// Change to the gameplay scene
		await LoadSceneAsync(sceneIndex);

		if (!TryFindBootstrap()) return;

		// Start the game in single-player mode
		bootstrap.StartSinglePlayer();

		SetIsInLobby(false);
	}
	public void StopLobby() {
		//NOTE: ShutdownAll was modified with this argument so scene change isn't forced
		if (!TryFindBootstrap()) return;

		bootstrap.ShutdownAll(changeScene: false);

		SetIsInLobby(false);
	}
	private IEnumerator DelayedStopGame() {
		gameStopped = true;
		yield return new WaitForSeconds(0.1f);
		try {
			bootstrap.ShutdownAll(changeScene: true); // This shuts down the NetworkRunner instances
		} catch { } finally {
			gameStopped = false;
		}

	}
	public void StopGame() {
		StartCoroutine(DelayedStopGame());
	}
}
