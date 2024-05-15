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

	[SerializeField] private GameObject scrollHullTurretPrefab, scrollAbilityPrefab;

	#endregion

	#region References

	[SerializeField] private RectTransform garageUI;

	[SerializeField] private GameObject playerHealthCanvas;
	public GameObject GetPlayerHealthCanvas() { return playerHealthCanvas; }

	[SerializeField] private RectTransform selectionContentParent;
	[SerializeField] private GridLayoutGroup selectionGrid;
	[SerializeField] private TMP_Text selectionScreenTitle, selectedItemText;

	[SerializeField] private Image selectedHullImage, selectedTurretImage;

	//TODO: temporary storage of hulls and turrets here -> move to centralized location
	[SerializeField] private List<string> hullNames;
	[SerializeField] private List<Sprite> hullSprites;

	[SerializeField] private List<string> turretNames;
	[SerializeField] private List<Sprite> turretSprites;

	[SerializeField] private List<string> gadgetNames;
	[SerializeField] private List<Sprite> gadgetSprites;

	[SerializeField] private List<string> abilityNames;
	[SerializeField] private List<Sprite> abilitySprites;

	[SerializeField] private Camera playerCamera;
	[SerializeField] private CameraBlur blur;

	#endregion

	#region Members

	private readonly List<GameObject> spawnedButtons = new();

	private Vector3 normalCameraPosition;

	//interpolate to this
	private Vector3 targetGarageCameraPosition = new(-1.9f, 1.29f, -5.7f);

	private bool inGarage = false;
	public bool GetIsInGarage() { return inGarage; }

	//spacings
	// 215/125 for hull/turret; 139/125 for abilities
	private Vector2 hullTurretCell = new(215, 125);
	private Vector2 abilityCell = new(139, 125);

	//stats display
	private string selectedHullName = "", selectedTurretName = "",
		selectedGadgetName = "", selectedAbilityName = "";
	private Sprite selectedHullSprite = null, selectedTurretSprite = null,
		selectedGadgetSprite = null, selectedAbilitySprite = null;

	private int currentSelectedMode = 0; //hull, turret, abilities

	#endregion

	#region Functions

	private void ClearButtons() {
		foreach (GameObject g in spawnedButtons) {
			if (g != null) Destroy(g);
		}
		spawnedButtons.Clear();
	}
	private void SelectHull(string hullName, Sprite hullSprite) {
		selectedHullSprite = hullSprite;
		selectedHullName = hullName;

		selectedItemText.text = hullName;
	}
	private void SelectTurret(string turretName, Sprite turretSprite) {
		selectedTurretName = turretName;
		selectedTurretSprite = turretSprite;

		selectedItemText.text = turretName;
	}
	private void SelectGadget(string gadgetName, Sprite gadgetSprite) {
		selectedGadgetName = gadgetName;
		selectedGadgetSprite = gadgetSprite;

		selectedItemText.text = gadgetName;
	}
	private void SelectAbility(string abilityName, Sprite abilitySprite) {
		selectedAbilityName = abilityName;
		selectedAbilitySprite = abilitySprite;

		selectedItemText.text = abilityName;
	}
	public void OpenHulls() {
		selectionScreenTitle.text = "HULLS";
		selectionGrid.cellSize = hullTurretCell;
		ClearButtons();

		for (int i = 0; i < hullNames.Count; i++) {
			GarageButton b = Instantiate(scrollHullTurretPrefab, selectionContentParent).GetComponent<GarageButton>();
			spawnedButtons.Add(b.gameObject);
			b.Init(hullNames[i], hullSprites[i], mode: 0, level: 0,
				equipped: hullNames[i] == PlayerPrefs.GetString("hull_name"));
		}
		currentSelectedMode = 0;
	}
	public void OpenTurrets() {
		selectionScreenTitle.text = "TURRETS";
		selectionGrid.cellSize = hullTurretCell;
		ClearButtons();

		for (int i = 0; i < turretNames.Count; i++) {
			GarageButton b = Instantiate(scrollHullTurretPrefab, selectionContentParent).GetComponent<GarageButton>();
			spawnedButtons.Add(b.gameObject);
			b.Init(turretNames[i], turretSprites[i], mode: 1, level: 0,
				equipped: turretNames[i] == PlayerPrefs.GetString("turret_name"));
		}
		currentSelectedMode = 1;
	}
	public void OpenGadgets() {
		selectionScreenTitle.text = "GADGETS";
		selectionGrid.cellSize = abilityCell;
		ClearButtons();

		for (int i = 0; i < gadgetNames.Count; i++) {
			GarageButton b = Instantiate(scrollAbilityPrefab, selectionContentParent).GetComponent<GarageButton>();
			spawnedButtons.Add(b.gameObject);
			b.Init(gadgetNames[i], gadgetSprites[i], mode: 2, level: 0,
				equipped: gadgetNames[i] == PlayerPrefs.GetString("gadget_name"));
		}
		currentSelectedMode = 2;
	}
	public void OpenAbilities() {
		selectionScreenTitle.text = "ABILITIES";
		selectionGrid.cellSize = abilityCell;
		ClearButtons();

		for (int i = 0; i < abilityNames.Count; i++) {
			GarageButton b = Instantiate(scrollAbilityPrefab, selectionContentParent).GetComponent<GarageButton>();
			spawnedButtons.Add(b.gameObject);
			b.Init(abilityNames[i], abilitySprites[i], mode: 3, level: 0,
				equipped: abilityNames[i] == PlayerPrefs.GetString("ability_name"));
		}
		currentSelectedMode = 3;
	}
	public void EquipItem() {
		if (currentSelectedMode == 0) {
			//equip hull
			MenuManager.instance.SetHull(selectedHullName, isTemporary: false);
			UpdateEquipped(selectedHullName, false, true);
			selectedHullImage.sprite = selectedHullSprite;
		} else if (currentSelectedMode == 1) {
			//equip turret
			MenuManager.instance.SetTurret(selectedTurretName, isTemporary: false);
			UpdateEquipped(selectedTurretName, false, true);
			selectedTurretImage.sprite = selectedTurretSprite;
		} else if (currentSelectedMode == 2) {
			//equip gadget
			bool equipped = UpdateEquipped(selectedGadgetName, true, false);
			PlayerPrefs.SetInt("gadget_" + selectedGadgetName, equipped ? 1 : 0);
		} else {
			//equip ability
			bool equipped = UpdateEquipped(selectedAbilityName, true, false);
			PlayerPrefs.SetInt("ability_" + selectedAbilityName, equipped ? 1 : 0);
		}
	}
	private bool UpdateEquipped(string itemName, bool isToggle, bool onlyOne) {
		foreach (GameObject g in spawnedButtons) {
			if (g == null || !g.TryGetComponent(out GarageButton b)) continue;

			if (b.GetItemName() != itemName) {
				//if only one can be selected unequip all else
				if (onlyOne) {
					b.SetEquipped(false);
				}
			} else {
				if (isToggle) {
					b.ToggleEquipped();
					return b.GetIsEquipped();
				} else b.SetEquipped(true);
			}
		}
		return true;
	}
	public void ScreenButtonClicked(string itemName, Sprite sprite, int mode) {
		if (mode == 0) {
			SelectHull(itemName, sprite);
			MenuManager.instance.SetHull(itemName, isTemporary: true);
		} else if (mode == 1) {
			SelectTurret(itemName, sprite);
			MenuManager.instance.SetTurret(itemName, isTemporary: true);
		} else if (mode == 2) {
			SelectGadget(itemName, sprite);
		} else {
			SelectAbility(itemName, sprite);
		}
	}
	public void OpenGarageTab() {
		garageUI.gameObject.SetActive(true);
		MenuManager.instance.SetMenuScreen(false);
		playerHealthCanvas.SetActive(false);
		inGarage = true;
		blur.SetBlur(1);
	}
	public void CloseGarageTab() {
		garageUI.gameObject.SetActive(false);
		MenuManager.instance.SetMenuScreen(true);
		playerHealthCanvas.SetActive(true);
		blur.SetBlur(0);

		//revert hull/turret to non-temp state
		MenuManager.instance.SetHull(PlayerPrefs.GetString("hull_name"), false);
		MenuManager.instance.SetTurret(PlayerPrefs.GetString("turret_name"), false);

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
		currentSelectedMode = 0;
		EquipItem();

		for (int i = 0; i < turretNames.Count; i++) {
			if (turretNames[i] == PlayerPrefs.GetString("turret_name")) {
				SelectTurret(turretNames[i], turretSprites[i]);
				break;
			}
		}
		currentSelectedMode = 1;
		EquipItem();

		for (int i = 0; i < gadgetNames.Count; i++) {
			if (gadgetNames[i] == PlayerPrefs.GetString("gadget_name")) {
				SelectGadget(gadgetNames[i], gadgetSprites[i]);
				break;
			}
		}
		currentSelectedMode = 2;
		EquipItem();

		for (int i = 0; i < abilityNames.Count; i++) {
			if (abilityNames[i] == PlayerPrefs.GetString("ability_name")) {
				SelectGadget(abilityNames[i], abilitySprites[i]);
				break;
			}
		}
		currentSelectedMode = 3;
		EquipItem();

		normalCameraPosition = playerCamera.transform.position;

		OpenHulls();
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
