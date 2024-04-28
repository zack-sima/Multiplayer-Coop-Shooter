using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GarageButton : MonoBehaviour {

	[SerializeField] private TMP_Text itemText;
	[SerializeField] private Image itemImage;

	private string itemName = "";
	private bool isTurret = false;

	public void Init(string name, Sprite sprite, bool isTurret) {
		itemText.text = name;
		itemImage.sprite = sprite;

		itemName = name;
		this.isTurret = isTurret;
	}
	public void ButtonClicked() {
		GarageManager.instance.ScreenButtonClicked(itemName, itemImage.sprite, isTurret);
	}
}
