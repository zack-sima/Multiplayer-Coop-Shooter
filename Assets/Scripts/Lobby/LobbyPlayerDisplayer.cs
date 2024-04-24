using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyPlayerDisplayer : MonoBehaviour {

	#region Statics & Consts

	#endregion

	#region References

	[SerializeField] private TMP_Text playerNameText;
	public void SetPlayerNameText(string nameText) { playerNameText.text = nameText; }

	//where hull & turret prefabs are stored
	[SerializeField] private PlayerInfo playerInfoRef;

	#endregion

	#region Members

	#endregion

	#region Functions

	private void Awake() { }

	private void Start() { }

	private void Update() { }

	#endregion

}
