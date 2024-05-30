using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class AccountDataSyncer : MonoBehaviour {

	#region Consts

	public static string baseURL = "http://main.indiewargames.net:22222/";

	#endregion

	#region Serializable Data

	//NOTE: upload class -- what is uploaded to the server every ~2s will always be of this json format
	[System.Serializable]
	public class UploadBlob {
		public string UID;
		public string username;
		public int status_id;
	}
	//NOTE: download class (parser) for the JSON list of friends that Jonathan returns
	[System.Serializable]
	public class FriendsBlob {
		//NOTE: all player receive their list of friends from the server
		public List<Dictionary<string, string>> friends_list;
	}

	#endregion

	#region Consts & Statics

	public static AccountDataSyncer instance;

	#endregion

	#region Members

	//NOTE: don't use infinite coroutines because switching scenes kills them; do these calls in update
	private float updateInfoTimer = 0f;

	//every second decrement repair list
	private float repairIncrementTimer = 1f;

	private float lastChangedTimestamp = -20f;

	private long lastRepairsTimestamp = 0;

	public static long GetSystemTime() {
		System.DateTimeOffset dateTimeOffset = System.DateTimeOffset.UtcNow;
		return dateTimeOffset.ToUnixTimeSeconds();
	}

	#endregion

	#region Functions

	#region Networking Calls

	public string SanitizeDownloadHandlerText(string input) {
		if (string.IsNullOrEmpty(input)) return input;

		char firstChar = input[0];
		char lastChar = input[input.Length - 1];

		if ((firstChar == '"' || firstChar == '\'') && (lastChar == '"' || lastChar == '\'')) {
			return input.Substring(1, input.Length - 2);
		}

		return input;
	}

	//NOTE: only should be called ONCE. Before the user receives their own local ID, no other networking calls
	//with the server that require user information should be made.
	private IEnumerator GenerateUserID() {
		yield return null;

		string url = $"{baseURL}generate/UID";

		WWWForm form = new();
		form.AddField("username", LobbyUI.GetPlayerName());

		using UnityWebRequest webRequest = UnityWebRequest.Post(url, form);
		yield return webRequest.SendWebRequest();

		if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
			webRequest.result == UnityWebRequest.Result.ProtocolError) {
			Debug.LogError($"Error: {webRequest.error}");

			yield return new WaitForSeconds(2);
			StartCoroutine(GenerateUserID());
		} else {
			Debug.Log($"Received: {webRequest.downloadHandler.text}");

			PersistentDict.SetString("user_id", SanitizeDownloadHandlerText(webRequest.downloadHandler.text));
			if (FriendsManager.instance != null) FriendsManager.instance.ReceivedUserId();
		}
	}
	//NOTE: posts friends list AFTER having fetched the player's list of friends (if server didn't have guest ID)
	private IEnumerator UpdateInformation() {
		//make sure player has been given an ID!
		if (!PersistentDict.HasKey("user_id")) { yield break; }
		if (ServerLinker.instance == null) yield break;

		bool canJoinLobby = !ServerLinker.instance.GetIsInLobby() && MenuManager.instance != null;

		UploadBlob dump = new() {
			UID = PersistentDict.GetString("user_id"),
			username = LobbyUI.GetPlayerName(),

			//2 == in lobby, 3 == in game, 1 == online, 0 == offline
			status_id = canJoinLobby ? 1 : (MenuManager.instance != null ? 2 : 3)
		};

		string dumpJson = MyJsonUtility.ToJson(typeof(UploadBlob), dump);

		string url = baseURL + "update/information";

		WWWForm form = new();
		form.AddField("blob", dumpJson);

		using UnityWebRequest webRequest = UnityWebRequest.Post(url, form);
		yield return webRequest.SendWebRequest();

		if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
			webRequest.result == UnityWebRequest.Result.ProtocolError) {
			Debug.LogError($"Error: {webRequest.error}");
		} else {
			//Debug.Log($"Response: {webRequest.downloadHandler.text}");

			string downloadJson = SanitizeDownloadHandlerText(webRequest.downloadHandler.text);
			//Debug.Log(downloadJson);
			FriendsBlob downloadedBlob = (FriendsBlob)MyJsonUtility.FromJson(typeof(FriendsBlob), downloadJson);

			//Debug.Log("friends updated");

			if (FriendsManager.instance != null)
				FriendsManager.instance.FriendsUpdated(downloadedBlob);
		}

		//TODO: make http call with dumpJson as UploadBlob args; this should return FriendsBlob
		//FriendsBlob downloadedBlob = new() { friends = new() };
		//FriendsBlob.FriendStatus f1 = new() { uid = "AAAAAAAA", name = "A", status_id = 1, is_pending = true };
		//FriendsBlob.FriendStatus f2 = new() { uid = "BBBBBBBB", name = "B", status_id = 1, is_pending = false };
		//FriendsBlob.FriendStatus f3 = new() { uid = "CCCCCCCC", name = "C", status_id = 2, is_pending = false };
		//FriendsBlob.FriendStatus f4 = new() { uid = "DDDDDDDD", name = "D", status_id = 0, is_pending = false };
		//downloadedBlob.friends.Add(f1);
		//downloadedBlob.friends.Add(f2);
		//downloadedBlob.friends.Add(f3);
		//downloadedBlob.friends.Add(f4);
	}
	private IEnumerator ProcessedFriendRequest(string uid, string friendUid, bool accepted) {
		WWWForm form = new();
		form.AddField("selfUID", uid);
		form.AddField("friendUID", friendUid);
		form.AddField("username", LobbyUI.GetPlayerName());
		form.AddField("accepted", (accepted ? 1 : 0).ToString());

		string url = $"{baseURL}friend/process_request";

		using UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

		yield return webRequest.SendWebRequest();

		Debug.Log(webRequest.downloadHandler.text);
	}
	private IEnumerator InviteToLobby(string uid, string friendUid, string lobbyId) {
		WWWForm form = new();
		form.AddField("selfUID", uid);
		form.AddField("friendUID", friendUid);
		form.AddField("lobby_invite", lobbyId);


		string url = $"{baseURL}friend/lobby";

		using UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

		yield return webRequest.SendWebRequest();

		Debug.Log(webRequest.downloadHandler.text);
	}
	private IEnumerator RemoveFriend(string uid, string friendUid) {
		WWWForm form = new();
		form.AddField("selfUID", uid);
		form.AddField("friendUID", friendUid);

		string url = $"{baseURL}friend/remove";

		using UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

		yield return webRequest.SendWebRequest();

		Debug.Log(webRequest.downloadHandler.text);
	}
	private IEnumerator RequestFriend(string uid, string friendUid) {
		WWWForm form = new();
		form.AddField("username", LobbyUI.GetPlayerName());
		form.AddField("friendUID", friendUid);
		form.AddField("selfUID", uid);

		string url = $"{baseURL}friend/request";

		using UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

		yield return webRequest.SendWebRequest();

		Debug.Log(webRequest.downloadHandler.text);
	}
	private IEnumerator ChangeUsername(string uid) {
		WWWForm form = new();

		form.AddField("username", LobbyUI.GetPlayerName());
		form.AddField("UID", uid);

		string url = $"{baseURL}change/username";

		using UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

		yield return webRequest.SendWebRequest();

		Debug.Log(webRequest.downloadHandler.text);
	}

	#endregion

	//called by FriendsManager
	public void AskForUserID() {
		StartCoroutine(GenerateUserID());
	}
	public void InviteFriendToLobby(string friendUid, string lobbyId) {
		StartCoroutine(InviteToLobby(PersistentDict.GetString("user_id"), friendUid, lobbyId));
	}
	public void AcceptedFriendRequest(string friendUid) {
		StartCoroutine(ProcessedFriendRequest(PersistentDict.GetString("user_id"), friendUid, true));
	}
	public void RejectedFriendRequest(string friendUid) {
		StartCoroutine(ProcessedFriendRequest(PersistentDict.GetString("user_id"), friendUid, false));
	}
	public void RemovedFriend(string friendUid) {
		StartCoroutine(RemoveFriend(PersistentDict.GetString("user_id"), friendUid));
	}
	public void MakeFriendRequest(string friendUid) {
		StartCoroutine(RequestFriend(PersistentDict.GetString("user_id"), friendUid));
	}
	public void ChangedUsername() {
		if (Time.time - lastChangedTimestamp < 10) return;

		lastChangedTimestamp = Time.time;
		StartCoroutine(ChangeUsername(PersistentDict.GetString("user_id")));
	}
	//for hull and turret repairs
	public void UpdateRepairs(int time) {
		List<int> repairTimers = PersistentDict.GetIntList("repair_timers");
		List<string> repairNames = PersistentDict.GetStringList("repair_names");

		for (int i = 0; i < repairTimers.Count; i++) {
			repairTimers[i] -= time;

			//repair complete
			if (repairTimers[i] <= 0) {
				//0 means no wear and tear
				PersistentDict.SetInt("repair_uses_" + repairNames[i], 0);
				repairTimers.RemoveAt(i);
				repairNames.RemoveAt(i);
				i--;
			}
		}

		PersistentDict.SetIntList("repair_timers", repairTimers);
		PersistentDict.SetStringList("repair_names", repairNames);

		if (RepairsManager.instance != null) {
			RepairsManager.instance.UpdateRepairs();
		}
	}
	public void CheckBackgroundTime() {
		if (lastRepairsTimestamp == 0) {
			lastRepairsTimestamp = GetSystemTime();
		}

		//application went to background
		if (GetSystemTime() - lastRepairsTimestamp > 10) {
			//Debug.Log($"Application went to background for {GetSystemTime() - lastRepairsTimestamp}s");
			ServerLinker.instance.SetWorldTimeUnfetched();
		}
	}
	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
			return;
		}
		instance = this;
		DontDestroyOnLoad(gameObject);
	}
	private void Start() {
		if (instance != this) return;

		print("started again");
		lastRepairsTimestamp = GetSystemTime();
	}
	private void Update() {
		updateInfoTimer -= Time.deltaTime;
		if (updateInfoTimer <= 0f) {
			updateInfoTimer = 2.5f;
			StartCoroutine(UpdateInformation());
		}

		//repairs; TODO: research also here
		repairIncrementTimer -= Time.deltaTime;
		if (repairIncrementTimer <= 0f) {
			CheckBackgroundTime();
			repairIncrementTimer = 1f;
			UpdateRepairs(1);
			lastRepairsTimestamp = GetSystemTime();
		}
	}

	#endregion

}
