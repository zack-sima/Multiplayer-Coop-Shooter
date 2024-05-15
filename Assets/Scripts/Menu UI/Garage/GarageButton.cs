using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GarageButton : MonoBehaviour {

	[SerializeField] private GameObject equippedBand;
	[SerializeField] private TMP_Text levelText;
	[SerializeField] private Image itemImage;

	private string itemName = "";
	private int mode = 0; //hull, turret, ability

	public void Init(string name, Sprite sprite, int mode, int level, bool equipped) {
		levelText.text = $"Level {level + 1}";
		itemImage.sprite = sprite;

		itemName = name;
		this.mode = mode;

		equippedBand.SetActive(equipped);
	}
	public string GetItemName() {
		return itemName;
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
		GarageManager.instance.ScreenButtonClicked(itemName, itemImage.sprite, mode);
	}
}
