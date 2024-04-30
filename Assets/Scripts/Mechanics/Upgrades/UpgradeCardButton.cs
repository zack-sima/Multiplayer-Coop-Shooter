using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCardButton : MonoBehaviour {
	[SerializeField] private Image image;
	[SerializeField] private TMP_Text nameText, costText;

	private string upgradeName;
	private int upgradeLevel;

	public void Init(string upgradeName, int upgradeLevel, int cost, Sprite imageSprite) {
		this.upgradeName = upgradeName;
		this.upgradeLevel = upgradeLevel;

		nameText.text = upgradeName;
		if (upgradeLevel > 0) nameText.text += " " + UpgradesCatalog.ToRoman(upgradeLevel);

		costText.text = cost.ToString();
		image.sprite = imageSprite;
	}

	public void ButtonClicked() {
		string levelText = upgradeLevel == 0 ? "" : $"_{upgradeLevel}";
		UpgradesCatalog.instance.PurchaseUpgrade(upgradeName + levelText);
	}
}
