using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthCanvas : MonoBehaviour {
	[SerializeField] private RectTransform healthBar;
	public RectTransform GetHealthBar() { return healthBar; }
	[SerializeField] private RectTransform healthBarChange;
	public RectTransform GetHealthBarChange() { return healthBarChange; }
}
