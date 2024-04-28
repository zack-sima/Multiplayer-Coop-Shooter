using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GarageManager : MonoBehaviour {

	#region Statics & Consts

	public static GarageManager instance;

	#endregion

	#region Prefabs

	[SerializeField] private GameObject scrollButtonPrefab;

	#endregion

	#region References

	[SerializeField] private RectTransform garageUI;
	[SerializeField] private Image garageImage;

	[SerializeField] private RectTransform selectionScreen, selectionContentParent;
	[SerializeField] private TMP_Text selectionScreenTitle;

	[SerializeField] private TMP_Text selectedHullTitle, selectedTurretTitle;
	[SerializeField] private Image selectedHullImage, selectedTurretImage;

	//TODO: temporary storage of hulls and turrets here -> move to centralized location
	[SerializeField] private List<string> hullNames;
	[SerializeField] private List<Sprite> hullSprites;

	[SerializeField] private List<string> turretNames;
	[SerializeField] private List<Sprite> turretSprites;

	#endregion

	#region Members

	private readonly List<GameObject> spawnedButtons = new();

	#endregion

	#region Functions

	private void ClearButtons() {
		foreach (GameObject g in spawnedButtons) {
			if (g != null) Destroy(g);
		}
		spawnedButtons.Clear();
	}
	private void SelectHull(string hullName, Sprite hullSprite) {
		selectedHullTitle.text = hullName;
		selectedHullImage.sprite = hullSprite;
	}
	private void SelectTurret(string turretName, Sprite turretSprite) {
		selectedTurretTitle.text = turretName;
		selectedTurretImage.sprite = turretSprite;
	}
	public void OpenHulls() {
		selectionScreenTitle.text = "HULLS";
		selectionScreen.gameObject.SetActive(true);
		ClearButtons();

		for (int i = 0; i < hullNames.Count; i++) {
			GarageButton b = Instantiate(scrollButtonPrefab, selectionContentParent).GetComponent<GarageButton>();
			spawnedButtons.Add(b.gameObject);
			b.Init(hullNames[i], hullSprites[i], false);
		}
	}
	public void OpenTurrets() {
		selectionScreenTitle.text = "TURRETS";
		selectionScreen.gameObject.SetActive(true);
		ClearButtons();

		for (int i = 0; i < turretNames.Count; i++) {
			GarageButton b = Instantiate(scrollButtonPrefab, selectionContentParent).GetComponent<GarageButton>();
			spawnedButtons.Add(b.gameObject);
			b.Init(turretNames[i], turretSprites[i], true);
		}
	}
	public void ScreenButtonClicked(string itemName, Sprite sprite, bool isTurret) {
		if (!isTurret) {
			SelectHull(itemName, sprite);
			MenuManager.instance.SetHullDropdown(itemName);
		} else {
			SelectTurret(itemName, sprite);
			MenuManager.instance.SetTurretDropdown(itemName);
		}
	}
	public void CloseSelectionScreen() {
		selectionScreen.gameObject.SetActive(false);
	}
	public void OpenGarageTab() {
		garageUI.gameObject.SetActive(true);
		MenuManager.instance.SetMenuScreen(false);
		garageImage.enabled = true;
	}
	public void CloseGarageTab() {
		garageUI.gameObject.SetActive(false);
		MenuManager.instance.SetMenuScreen(true);
		garageImage.enabled = false;
	}

	private void Awake() {
		instance = this;
	}

	//TODO: change menu manager dropdown values to correspond with hull & turret names in centralized file
	private void Start() {
		for (int i = 0; i < hullNames.Count; i++) {
			if (hullNames[i] == PlayerPrefs.GetString("hull_name")) {
				SelectHull(hullNames[i], hullSprites[i]);
				break;
			}
		}
		for (int i = 0; i < turretNames.Count; i++) {
			if (turretNames[i] == PlayerPrefs.GetString("turret_name")) {
				SelectTurret(turretNames[i], turretSprites[i]);
				break;
			}
		}
	}

	private void Update() { }

	#endregion

}
