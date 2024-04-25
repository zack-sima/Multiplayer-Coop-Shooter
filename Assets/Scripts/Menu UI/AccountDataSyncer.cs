using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountDataSyncer : MonoBehaviour {

	#region Serializable Data

	//NOTE: upload class -- what is uploaded to the server every ~2s will always be of this json format
	[System.Serializable]
	public class UploadBlob {
		public string uid = "[ERROR]";
		public string name = "[ERROR]";
		public int status_id = 0;
	}
	//NOTE: download class (parser) for the JSON list of friends that Jonathan returns
	[System.Serializable]
	public class FriendsBlob {

		[System.Serializable]
		public class FriendStatus {
			//first two MUST be populated
			public string uid = "[ERROR]";
			public string name = "[ERROR]";

			public int status_id = 0;
			public bool is_pending = false;

			//NOTE: this is usually an empty string, but for ONE RETURN should include a lobby ID
			//  that the friend wants the player to join.
			public string lobby_invite = "";
		}

		//NOTE: all player receive their list of friends from the server
		public List<FriendStatus> friends;
	}

	#endregion

	#region Consts & Statics

	public static AccountDataSyncer instance;

	#endregion

	#region Members

	//NOTE: don't use infinite coroutines because switching scenes kills them; do these calls in update
	private float updateInfoTimer = 0f;

	#endregion

	#region Functions

	#region Networking Calls

	//NOTE: only should be called ONCE. Before the user receives their own local ID, no other networking calls
	//with the server that require user information should be made.
	private IEnumerator GenerateUserID() {
		yield return null;

		//TODO: callback to FriendsManager with correct ID

		//temporary fake ID generated locally
		string fakeID = Random.Range(10000000, 100000000).ToString();
		PersistentDict.SetString("user_id", fakeID);

		if (FriendsManager.instance != null) FriendsManager.instance.ReceivedUserId();
	}
	//NOTE: posts friends list AFTER having fetched the player's list of friends (if server didn't have guest ID)
	private IEnumerator UpdateInformation() {
		//make sure player has been given an ID!
		if (!PersistentDict.HasKey("user_id")) { yield break; }

		UploadBlob dump = new() {
			uid = PersistentDict.GetString("user_id"),
		};

		string dumpJson = MyJsonUtility.ToJson(typeof(UploadBlob), dump);

		//TODO: make http call with dumpJson as UploadBlob args; this should return FriendsBlob
		FriendsBlob downloadedBlob = new() { friends = new() };
		FriendsBlob.FriendStatus f1 = new() { uid = "AAAAAAAA", name = "A", status_id = 1, is_pending = true };
		FriendsBlob.FriendStatus f2 = new() { uid = "BBBBBBBB", name = "B", status_id = 1, is_pending = false };
		FriendsBlob.FriendStatus f3 = new() { uid = "CCCCCCCC", name = "C", status_id = 2, is_pending = false };
		FriendsBlob.FriendStatus f4 = new() { uid = "DDDDDDDD", name = "D", status_id = 0, is_pending = false };
		downloadedBlob.friends.Add(f1);
		downloadedBlob.friends.Add(f2);
		downloadedBlob.friends.Add(f3);
		downloadedBlob.friends.Add(f4);

		Debug.Log("friends updated");
		FriendsManager.instance.FriendsUpdated(downloadedBlob);
	}
	private IEnumerator ProcessedFriendRequest(string uid, string friendUid, bool accepted) {
		//TODO: make http call to corresponding function
		yield return null;
	}
	private IEnumerator InviteToLobby(string uid, string friendUid) {
		//TODO: make http call that invites to lobby
		yield return null;
	}

	#endregion

	//called by FriendsManager
	public void AskForUserID() {
		StartCoroutine(GenerateUserID());
	}
	public void InviteFriendToLobby(string uid, string friendUid) {
		StartCoroutine(InviteToLobby(uid, friendUid));
	}
	public void AcceptedFriendRequest(string friendUid) {
		StartCoroutine(ProcessedFriendRequest(PersistentDict.GetString("user_id"), friendUid, true));
	}
	public void RejectedFriendRequest(string friendUid) {
		StartCoroutine(ProcessedFriendRequest(PersistentDict.GetString("user_id"), friendUid, false));
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
	}
	private void Update() {
		updateInfoTimer -= Time.deltaTime;
		if (updateInfoTimer <= 0f) {
			updateInfoTimer = 2.5f;
			StartCoroutine(UpdateInformation());
		}
	}

	#endregion

}
