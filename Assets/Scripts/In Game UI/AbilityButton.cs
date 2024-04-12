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

	[SerializeField] private KeyCode triggerKey;

	//callback (hook to functions, TODO: currently done in PlayerInfo script)
	public UnityEvent OnAbilityUsed;

	//TODO: add energy costs for abilities
	//TODO: load in abilities from player presets when more are made
	private float abilityReadiness = 0; //ability charge bar

	[SerializeField] private float abilityCooldown = 2f;

	#endregion

	#region Functions

	//TODO: some sort of init function to be called by PlayerInfo

	public void TryUseAbility() {
		if (abilityReadiness < 1f) return;

		abilityReadiness = 0f;
		OnAbilityUsed?.Invoke();
	}
	private void Update() {
		if (abilityReadiness < 1f)
			abilityReadiness += Time.deltaTime / abilityCooldown;

		if (Input.GetKeyDown(triggerKey)) TryUseAbility();

		progressBar.fillAmount = Mathf.Clamp(abilityReadiness, 0, 1);
	}

	#endregion

}
