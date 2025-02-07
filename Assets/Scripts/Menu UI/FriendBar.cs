using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendBar : MonoBehaviour {

	#region References

	[SerializeField] private TMP_Text friendNameText, friendIDText, onlineStatusText;
	[SerializeField] private Button acceptInviteButton, rejectInviteButton, inviteToLobbyButton, removeButton;

	#endregion

	#region Members

	//is this an invite (not a friend yet)?
	private bool isPending = false;

	private string friendName = "";

	private string friendId = "";
	public string GetFriendId() { return friendId; }

	private int statusId = 0;

	#endregion

	#region Functions

	//NOTE: must call this function when instantiating this bar for it to behave properly
	//  fields:
	//		isPendingFriend: whether it's a friend or just a friend request
	//		statusId: 0 -> offline, 1 -> online and can be invited, 2 -> in lobby, 3 -> in match
	//		localPlayerInLobby: whether the player of this device is actually in a lobby or not

	public void InitializeFriendBar(string friendName, string friendId, bool isPendingFriend, int statusId) {

		//sets this button to display friend as pending (with accept/reject button)
		isPending = isPendingFriend;

		acceptInviteButton.gameObject.SetActive(isPending);
		rejectInviteButton.gameObject.SetActive(isPending);
		onlineStatusText.gameObject.SetActive(!isPending);
		removeButton.gameObject.SetActive(!isPending);
		inviteToLobbyButton.gameObject.SetActive(false);

		friendNameText.text = friendName;
		friendIDText.text = "#" + friendId;

		this.friendName = friendName;
		this.friendId = friendId;
		this.statusId = statusId;

		//actual friend stuff
		if (!isPending)
			ProcessFriend();
	}
	private void ProcessFriend() {
		//statuses display
		switch (statusId) {
			case 0:
				onlineStatusText.text = "<color=#FF5555>OFFLINE</color>";
				break;
			case 1:
				//NOTE: the only code where invite is possible
				onlineStatusText.text = "<color=#33FF33>ONLINE</color>";
				break;
			case 2:
				onlineStatusText.text = "<color=yellow>IN LOBBY</color>";
				break;
			case 3:
				onlineStatusText.text = "<color=yellow>IN GAME</color>";
				break;
		}

		//can be invited to the lobby! (if not in lobby, replaces online text with invite button)
		if (ServerLinker.instance.GetIsInLobby() && statusId == 1) {
			inviteToLobbyButton.gameObject.SetActive(true);
			onlineStatusText.gameObject.SetActive(false);
		}
	}

	public void AcceptInvite() {
		FriendsManager.instance.AcceptFriendRequest(friendId);
		ProcessFriend();

		acceptInviteButton.gameObject.SetActive(false);
		rejectInviteButton.gameObject.SetActive(false);
		onlineStatusText.gameObject.SetActive(true);
	}
	public void RejectInvite() {
		FriendsManager.instance.RejectFriendRequest(friendId);
		Destroy(gameObject, 0.1f);
	}
	public void RemoveFriend() {
		FriendsManager.instance.RemoveFriend(friendId, friendName);
	}
	public void InviteToLobby() {
		if (!ServerLinker.instance.GetIsInLobby()) return;
		FriendsManager.instance.InviteFriendToLobby(friendId, PlayerPrefs.GetString("room_id"));
	}

	private void Awake() { }

	private void Start() { }

	private void Update() { }

	#endregion

}
