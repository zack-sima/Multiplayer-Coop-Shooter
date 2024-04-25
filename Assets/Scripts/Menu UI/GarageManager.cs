using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageManager : MonoBehaviour {

	#region Statics & Consts

	#endregion

	#region References

	[SerializeField] private RectTransform garageUI;

	#endregion

	#region Members

	#endregion

	#region Functions

	public void OpenGarageTab() {
		garageUI.gameObject.SetActive(true);
	}
	public void CloseGarageTab() {
		garageUI.gameObject.SetActive(false);
	}

	private void Awake() { }

	private void Start() { }

	private void Update() { }

	#endregion

}
