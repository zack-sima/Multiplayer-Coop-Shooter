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

	[SerializeField] private TMP_InputField lobbyIdInput, playerNameInput;

	//disabled when player is already in a lobby
	//  NOTE: only set when friends UI is active, otherwise SetActive() for child won't go through
	[SerializeField] private RectTransform joinLobbyParent;

	#endregion

	#region Members

	#endregion

	#region Functions

	//TODO: a cycle every ~5 seconds pulling data from Jonathan's side

	//called by button callback
	public void TryJoinLobby(string lobbyId) {
		MenuManager.instance.StartLobby(lobbyId, true);
	}
	public void CloseFriendsTab() {
		friendsUI.gameObject.SetActive(false);
	}
	//NOTE: call this to open the friends tab and set playerId, etc etc
	public void OpenFriendsTab() {
		//if already in lobby, don't show lobby
		joinLobbyParent.gameObject.SetActive(!ServerLinker.instance.GetIsInLobby());
	}
	private void Awake() {
		instance = this;
	}

	private void Start() { }

	private void Update() { }

	#endregion

}
