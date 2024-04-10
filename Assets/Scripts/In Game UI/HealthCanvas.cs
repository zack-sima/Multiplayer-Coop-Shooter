using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthCanvas : MonoBehaviour {
	[SerializeField] private RectTransform healthBar;
	public RectTransform GetHealthBar() { return healthBar; }
	[SerializeField] private RectTransform healthBarChange;
	public RectTransform GetHealthBarChange() { return healthBarChange; }
	[SerializeField] private RectTransform ammoBar;
	public RectTransform GetAmmoBar() { return ammoBar; }
	[SerializeField] private TMP_Text healthText;
	public TMP_Text GetHealthText() { return healthText; }
}
