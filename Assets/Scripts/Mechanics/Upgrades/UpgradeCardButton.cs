using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCardButton : MonoBehaviour {
	[SerializeField] private Image image;
	[SerializeField] private TMP_Text nameText, costText;

	private bool purchased;

	private UpgradesCatalog.UpgradeNode node;
	public UpgradesCatalog.UpgradeNode GetNode() {return node;}

	public void PurchaseSuccessful() {
		purchased = true;
		costText.text = "Purchased";
	}
	public void Init(UpgradesCatalog.UpgradeNode node) {
		this.node = node;

		nameText.text = node.upgradeName;
		if (node.level > 0) nameText.text += " " + UpgradesCatalog.ToRoman(node.level);

		costText.text = "$" + node.cost.ToString();
		image.sprite = node.icon;
		purchased = false;
	}

	public void ButtonClicked() {
		if (purchased) return;

		UpgradesCatalog.instance.PurchaseUpgrade(this, node.GetUpgradeId());
	}
}
