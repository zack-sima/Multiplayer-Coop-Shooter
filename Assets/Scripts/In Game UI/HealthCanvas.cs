using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthCanvas : MonoBehaviour {

	#region References

	[SerializeField] private RectTransform healthBar;
	public RectTransform GetHealthBar() { return healthBar; }
	[SerializeField] private RectTransform healthBarChange;
	public RectTransform GetHealthBarChange() { return healthBarChange; }
	[SerializeField] private RectTransform ammoBar;
	public RectTransform GetAmmoBar() { return ammoBar; }
	[SerializeField] private RectTransform ammoGrowBar;
	public RectTransform GetAmmoGrowBar() { return ammoGrowBar; }
	[SerializeField] private TMP_Text healthText;
	public TMP_Text GetHealthText() { return healthText; }
	[SerializeField] private TMP_Text nameText;
	public TMP_Text GetNameText() { return nameText; }
	[SerializeField] private TMP_Text nameGhostText;
	public TMP_Text GetNameGhostText() { return nameGhostText; }

	//player ammo, set to invisible
	[SerializeField] private GameObject ammoTickerRef;

	#endregion

	#region Members

	private readonly List<GameObject> ammoTickers = new();

	#endregion

	#region Functions

	public void UpdateAmmoTickerCount(int tickerCount) {
		//clear existing
		foreach (GameObject g in ammoTickers) {
			if (g != null) Destroy(g);
		}
		ammoTickers.Clear();

		if (tickerCount <= 0) return;

		RectTransform parentRect = ammoTickerRef.transform.parent.GetComponent<RectTransform>();
		float parentWidth = parentRect.rect.width;

		// Calculate the spacing based on the number of ticks and the parent's width
		// We divide the total width by tickerCount + 1 to distribute ticks evenly
		float spacing = parentWidth / (tickerCount + 1);

		for (int i = 0; i < tickerCount; i++) {
			GameObject g = Instantiate(ammoTickerRef, ammoTickerRef.transform.parent);
			// Position each ticker such that they are evenly spaced within the parent
			float posX = spacing * (i + 1);  // Adjusting position to center
			g.transform.localPosition = new Vector2(posX +
				ammoTickerRef.GetComponent<RectTransform>().rect.width / 2f, 0);
			g.SetActive(true);
			ammoTickers.Add(g);
		}
	}

	#endregion

}
