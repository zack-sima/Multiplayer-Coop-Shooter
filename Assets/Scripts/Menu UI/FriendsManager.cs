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

	[SerializeField] private RectTransform friendsUI;
	[SerializeField] private TMP_InputField lobbyIdInput, friendIdInput;

	[SerializeField] private TMP_Text friendsTitleText, playerIdText;


	//disabled when player is already in a lobby
	//  NOTE: only set when friends UI is active, otherwise SetActive() for child won't go through
	[SerializeField] private RectTransform joinLobbyParent;

	#endregion

	#region Members


	#endregion

	#region Functions

	//actually adds the pending friend to the real list of friends
	public void AddUserToFriendsList(string friend_uid) {
		List<string> friends = PersistentDict.GetStringList("user_friends");
		friends.Add(friend_uid);
		PersistentDict.SetStringList("user_friends", friends);
	}
	//NOTE: updates the dropdown of friends to reflect new statuses, etc by creating it again
	public void FriendsUpdated(AccountDataSyncer.FriendsBlob newFriendsBlob) {
		//TODO: load friends
	}
	//called by button callback
	public void TryJoinLobby() {
		MenuManager.instance.StartLobby(lobbyIdInput.text.ToUpper(), true);
	}
	public void TryInviteFriend() {
		AccountDataSyncer.instance.InviteFriendToLobby(PersistentDict.GetString("user_id"),
			friendIdInput.text.ToUpper());
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
		if (lobbyIdInput.text != lobbyIdInput.text.ToUpper())
			lobbyIdInput.text = lobbyIdInput.text.ToUpper();
		if (friendIdInput.text != friendIdInput.text.ToUpper())
			friendIdInput.text = friendIdInput.text.ToUpper();
	}

	#endregion

}
