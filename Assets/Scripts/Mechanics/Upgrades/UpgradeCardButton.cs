using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class UpgradeCardButton : MonoBehaviour {
	[SerializeField] private Image image;
	[SerializeField] private TMP_Text nameText, costText, descriptionText;

	private bool purchased;

	private UpgradesCatalog.UpgradeNode node;
	public UpgradesCatalog.UpgradeNode GetNode() { return node; }

	public void PurchaseSuccessful() {
		purchased = true;
		costText.text = "Purchased";
	}
	public void UpdateCost(int playerMoney) {
		if (node == null) return;
		if (playerMoney >= node.cost) {
			costText.color = new Color(0.3f, 1f, 0.3f);
		} else {
			costText.color = new Color(1f, 0.3f, 0.3f);
		}
	}
	public void Init(UpgradesCatalog.UpgradeNode node) {
		this.node = node;

		if (node == null) return;

		nameText.text = Translator.Translate(node.displayName);
		if (node.level > 0) nameText.text += " " + UpgradesCatalog.ToRoman(node.level);

		descriptionText.text = node.description;
		costText.text = "$" + node.cost.ToString();
		if (node.internalIcon != null) image.sprite = node.internalIcon;
		purchased = false;
	}

	public void ButtonClicked() {
		if (purchased) return;

		UpgradesCatalog.instance.PurchaseUpgrade(this, node.GetUpgradeId());
	}
}
