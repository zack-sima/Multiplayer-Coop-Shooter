using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendsManager : MonoBehaviour {

	#region Statics & Consts

	public static FriendsManager instance;

	#endregion

	#region Prefabs

	[SerializeField] private GameObject friendBarPrefab;

	#endregion

	#region References

	[SerializeField] private RectTransform friendsUI, friendsScrollParent;
	[SerializeField] private TMP_InputField lobbyIdInput, friendIdInput;

	[SerializeField] private TMP_Text friendsTitleText, playerIdText;


	//disabled when player is already in a lobby
	//  NOTE: only set when friends UI is active, otherwise SetActive() for child won't go through
	[SerializeField] private RectTransform joinLobbyParent;

	[SerializeField] private RectTransform joinInviteLobbyScreen;

	#endregion

	#region Members

	private readonly List<GameObject> friendBars = new();

	//if a player rejected a friend request, next query's pending friend id matching this will be discarded
	private readonly List<string> rejectedFriendIDs = new();
	public bool GetIdInRejectedList(string uid) { return rejectedFriendIDs.Contains(uid); }

	//if id is in here, even if server still may think it is a pending friend, treat as a normal friend
	private readonly List<string> acceptedFriendIDs = new();
	public bool GetIdInAcceptedList(string uid) { return acceptedFriendIDs.Contains(uid); }

	//should not be empty if an invite was received
	private string invitedLobbyId = "";

	#endregion

	#region Functions

	//TODO: FIX READY UP FOR LOCAL PLAYER
	//TODO: sort so that pending comes on top, and then the rest alphabetically
	//NOTE: updates the dropdown of friends to reflect new statuses, etc by creating it again
	public void FriendsUpdated(AccountDataSyncer.FriendsBlob newFriendsBlob) {
		//load friends
		friendsTitleText.text = "Friends List";

		foreach (GameObject g in friendBars) {
			if (g == null) continue;
			Destroy(g);
		}
		friendBars.Clear();

		if (newFriendsBlob.friends == null) newFriendsBlob.friends = new();

		foreach (AccountDataSyncer.FriendsBlob.FriendStatus f in newFriendsBlob.friends) {
			//ignore if in rejected list
			if (rejectedFriendIDs.Contains(f.uid)) continue;

			if (f.lobby_invite != "") {
				//send player invite
				LobbyInviteReceived(f.name, f.uid, f.lobby_invite);
			}

			FriendBar fb = Instantiate(friendBarPrefab, friendsScrollParent).GetComponent<FriendBar>();

			//if accepted, pending should always be false
			bool pending = acceptedFriendIDs.Contains(f.uid) ? false : f.is_pending;
			fb.InitializeFriendBar(f.name, f.uid, pending, f.status_id);

			friendBars.Add(fb.gameObject);
		}
	}
	//called by button callback
	public void TryJoinLobby() {
		MenuManager.instance.StartLobby(lobbyIdInput.text.ToUpper(), true);
	}
	public void FriendRequest() {
		if (friendIdInput.text == "") return;

		AccountDataSyncer.instance.MakeFriendRequest(friendIdInput.text);

		friendIdInput.text = "";
	}
	public void InviteFriendToLobby(string friendId) {
		AccountDataSyncer.instance.InviteFriendToLobby(friendId);
	}
	//called from FriendBar
	public void AcceptFriendRequest(string friendId) {
		acceptedFriendIDs.Add(friendId);
		AccountDataSyncer.instance.AcceptedFriendRequest(friendId);
	}
	public void RejectFriendRequest(string friendId) {
		rejectedFriendIDs.Add(friendId);
		AccountDataSyncer.instance.RejectedFriendRequest(friendId);
	}
	//called from FriendsUpdated() callback if server gave an invite code
	public void LobbyInviteReceived(string friendName, string friendId, string lobbyId) {
		if (joinInviteLobbyScreen.gameObject.activeInHierarchy) return;

		joinInviteLobbyScreen.gameObject.SetActive(true);

		invitedLobbyId = lobbyId;

		joinInviteLobbyScreen.GetChild(0).GetComponent<TMP_Text>().text =
			$"{friendName} (#{friendId}) invited you to their lobby.";
	}
	public void LobbyInviteAccepted() {
		joinInviteLobbyScreen.gameObject.SetActive(false);

		if (invitedLobbyId != "") {
			MenuManager.instance.StartLobby(invitedLobbyId, true);
			invitedLobbyId = "";
		}
	}
	public void LobbyInviteRejected() {
		joinInviteLobbyScreen.gameObject.SetActive(false);
		invitedLobbyId = "";
	}
	public void CloseFriendsTab() {
		friendsUI.gameObject.SetActive(false);
	}
	//NOTE: call this to open the friends tab and set playerId, etc etc
	public void OpenFriendsTab() {
		friendsUI.gameObject.SetActive(true);

		//if already in lobby, don't show lobby
		joinLobbyParent.gameObject.SetActive(!ServerLinker.instance.GetIsInLobby());
	}
	public void ReceivedUserId() {
		playerIdText.text = $"Your ID: #{PersistentDict.GetString("user_id")}";
	}
	private void Awake() {
		instance = this;
	}

	private void Start() {
		if (PersistentDict.HasKey("user_id")) {
			playerIdText.text = $"Your ID: #{PersistentDict.GetString("user_id")}";
		} else {
			playerIdText.text = $"Your ID: [LOADING]";
			AccountDataSyncer.instance.AskForUserID();
		}
	}

	private void Update() {
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Q)) {
			LobbyInviteReceived("skibidi", "toilet", "abcdef");
		}
#endif

		if (lobbyIdInput.text != lobbyIdInput.text.ToUpper())
			lobbyIdInput.text = lobbyIdInput.text.ToUpper();
		if (friendIdInput.text != friendIdInput.text.ToUpper())
			friendIdInput.text = friendIdInput.text.ToUpper();
	}

	#endregion

}
