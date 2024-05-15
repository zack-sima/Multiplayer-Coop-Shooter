using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GarageManager : MonoBehaviour {

	#region Statics & Classes

	public static GarageManager instance;

	#endregion

	#region Prefabs

	[SerializeField] private GameObject scrollButtonPrefab;

	#endregion

	#region References

	[SerializeField] private RectTransform garageUI;

	[SerializeField] private GameObject playerHealthCanvas;
	public GameObject GetPlayerHealthCanvas() { return playerHealthCanvas; }

	[SerializeField] private RectTransform selectionScreen, selectionContentParent;
	[SerializeField] private TMP_Text selectionScreenTitle;

	[SerializeField] private Image selectedHullImage, selectedTurretImage;

	//TODO: temporary storage of hulls and turrets here -> move to centralized location
	[SerializeField] private List<string> hullNames;
	[SerializeField] private List<Sprite> hullSprites;

	[SerializeField] private List<string> turretNames;
	[SerializeField] private List<Sprite> turretSprites;

	[SerializeField] private Camera playerCamera;
	[SerializeField] private CameraBlur blur;

	#endregion

	#region Members

	private readonly List<GameObject> spawnedButtons = new();

	private Vector3 normalCameraPosition;

	//interpolate to this
	private Vector3 targetGarageCameraPosition = new(-1.5f, 1.29f, -5.7f);

	private bool hullMode = true;

	private bool inGarage = false;
	public bool GetIsInGarage() { return inGarage; }

	//stats display
	private string selectedHullName = "", selectedTurretName = "";

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
		selectedHullName = hullName;

		CloseSelectionScreen();
	}
	private void SelectTurret(string turretName, Sprite turretSprite) {
		selectedTurretImage.sprite = turretSprite;
		selectedTurretName = turretName;

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
			MenuManager.instance.SetHull(itemName);
		} else {
			SelectTurret(itemName, sprite);
			MenuManager.instance.SetTurret(itemName);
		}
	}
	public void CloseSelectionScreen() {
		selectionScreen.gameObject.SetActive(false);
	}
	public void OpenGarageTab() {
		garageUI.gameObject.SetActive(true);
		MenuManager.instance.SetMenuScreen(false);
		playerHealthCanvas.SetActive(false);
		inGarage = true;
		blur.SetBlur(1);
	}
	public void CloseGarageTab() {
		CloseSelectionScreen();
		garageUI.gameObject.SetActive(false);
		MenuManager.instance.SetMenuScreen(true);
		playerHealthCanvas.SetActive(true);
		blur.SetBlur(0);

		inGarage = false;
	}

	#region Cool Lerp

	private float LogarithmicLerp(float start, float end, float value) {
		float scale = end - start;
		return start + scale * Mathf.Log10(10 * value); // Logarithmic easing
	}
	private IEnumerator AnimateText(int startValue, int endValue, float duration,
		TextMeshProUGUI textComponent, bool isPerSec = false) {

		float currentTime = 0;
		int currentValue = startValue;

		// Set the initial text value
		textComponent.text = currentValue.ToString() + (isPerSec ? " /s" : "");
		textComponent.color = Color.green;

		while (currentValue < endValue) {
			currentTime += Time.deltaTime;
			float progress = currentTime / duration;
			progress = Mathf.Clamp(progress, 0, 1); // Ensure progress does not exceed 1

			currentValue = (int)LogarithmicLerp(startValue, endValue, -.05f / (progress + .06f) + 1.05f);

			// Update the text with the current value
			textComponent.text = currentValue.ToString() + (isPerSec ? " /s" : "");

			// Yield until the next frame
			yield return null;
		}

		// Ensure the final value is set after the loop
		textComponent.text = endValue.ToString() + (isPerSec ? " /s" : "");
		textComponent.color = Color.white; // Reset color or set to a new color
	}

	#endregion

	#endregion
	//TODO: Push and pulling from persistent data, Loadouts, UIUpdating, etc.

	#region Awake & Start & Update
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
				targetGarageCameraPosition, Time.deltaTime * 10f);
		} else {
			playerCamera.transform.position = Vector3.MoveTowards(playerCamera.transform.position,
				normalCameraPosition, Time.deltaTime * 10f);
		}
	}

	private void LateUpdate() {

	}

	#endregion
}
