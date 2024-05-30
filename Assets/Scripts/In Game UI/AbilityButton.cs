using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class AbilityButton : MonoBehaviour {

	#region Statics & Consts

	#endregion

	#region References

	[SerializeField] private Image progressBar;

	#endregion

	#region Members

	[SerializeField] private string triggerKey;

	//callback
	public UnityEvent OnAbilityUsed;

	#endregion

	#region Functions

	public void TryUseAbility() {
		OnAbilityUsed?.Invoke();
	}
	private void Update() {
		if (triggerKey != "" && Input.GetKeyDown(triggerKey.ToLower())) TryUseAbility();
	}

	#endregion

}
