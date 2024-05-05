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

	[SerializeField] private GameObject playerHealthCanvas;

	[SerializeField] private RectTransform selectionScreen, selectionContentParent;
	[SerializeField] private TMP_Text selectionScreenTitle;

	[SerializeField] private Image selectedHullImage, selectedTurretImage;

	//TODO: temporary storage of hulls and turrets here -> move to centralized location
	[SerializeField] private List<string> hullNames;
	[SerializeField] private List<Sprite> hullSprites;

	[SerializeField] private List<string> turretNames;
	[SerializeField] private List<Sprite> turretSprites;

	[SerializeField] private Camera playerCamera;

	#endregion

	#region Members

	private readonly List<GameObject> spawnedButtons = new();

	private Vector3 normalCameraPosition = Vector3.zero;

	//interpolate to this
	private Vector3 targetCameraPosition = new(-1f, 1.8f, -5.5f);

	private bool hullMode = true;

	private bool inGarage = false;
	public bool GetIsInGarage() { return inGarage; }

	#endregion

	#region Functions

	private void ClearButtons() {
		foreach (GameObject g in spawnedButtons) {
			if (g != null) Destroy(g);
		}
		spawnedButtons.Clear();
	}
	private void SelectHull(string hullName, Sprite hullSprite) {
		selectedHullImage.sprite = hullSprite;

		CloseSelectionScreen();
	}
	private void SelectTurret(string turretName, Sprite turretSprite) {
		selectedTurretImage.sprite = turretSprite;

		CloseSelectionScreen();
	}
	public void OpenHulls() {
		if (hullMode && selectionScreen.gameObject.activeInHierarchy) {
			CloseSelectionScreen();
			return;
		}

		hullMode = true;
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
		if (!hullMode && selectionScreen.gameObject.activeInHierarchy) {
			CloseSelectionScreen();
			return;
		}

		hullMode = false;
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
		playerHealthCanvas.SetActive(false);
		inGarage = true;
	}
	public void CloseGarageTab() {
		CloseSelectionScreen();
		garageUI.gameObject.SetActive(false);
		MenuManager.instance.SetMenuScreen(true);
		garageImage.enabled = false;
		playerHealthCanvas.SetActive(true);
		inGarage = false;
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
		normalCameraPosition = playerCamera.transform.position;
	}

	private void Update() {
		if (garageUI.gameObject.activeInHierarchy) {
			playerCamera.transform.position = Vector3.MoveTowards(playerCamera.transform.position,
				targetCameraPosition, Time.deltaTime * 10f);
		} else {
			playerCamera.transform.position = Vector3.MoveTowards(playerCamera.transform.position,
				normalCameraPosition, Time.deltaTime * 10f);
		}
	}

	#endregion

}
