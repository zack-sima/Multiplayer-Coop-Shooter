using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GarageButton : MonoBehaviour {

	[SerializeField] private GameObject equippedBand, repairBand;
	[SerializeField] private TMP_Text levelText;
	[SerializeField] private Image itemImage;

	[SerializeField] private TMP_Text usesLeftText;
	[SerializeField] private Image usesLeftImage;

	[SerializeField] private Color noUsesColor, haveUsesColor;
	[SerializeField] private Image noUsesImage;

	private string itemName = "";
	private int mode = 0; //hull, turret, ability

	public void Init(string name, Sprite sprite, int mode, int level, bool equipped) {
		levelText.text = $"Lv. {level + 1}";
		itemImage.sprite = sprite;

		itemName = name;
		this.mode = mode;

		equippedBand.SetActive(equipped);
	}
	//for hull/turret, # of uses left
	public void SetUsesLeft(int usesLeft, int maxUses) {
		usesLeftImage.color = usesLeft == 0 ? noUsesColor : haveUsesColor;
		usesLeftText.color = usesLeft == 0 ? noUsesColor : haveUsesColor;
		noUsesImage.gameObject.SetActive(usesLeft == 0);
		usesLeftText.text = usesLeft + "/" + maxUses;
	}
	public string GetItemName() {
		return itemName;
	}
	public void SetRepairing(bool repairing) {
		repairBand.SetActive(repairing);
	}
	public bool GetIsEquipped() {
		return equippedBand.activeInHierarchy;
	}
	public void ToggleEquipped() {
		equippedBand.SetActive(!equippedBand.activeInHierarchy);
	}
	public void SetEquipped(bool equipped) {
		equippedBand.SetActive(equipped);
	}
	public void ButtonClicked() {
		print(mode);
		GarageManager.instance.ScreenButtonClicked(itemName, itemImage.sprite, mode, GetIsEquipped());
	}
}
