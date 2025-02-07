using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GarageManager : MonoBehaviour {

	#region Statics & Classes

	public static GarageManager instance;

	private const int MAX_ABILITIES = 3;
	private const int MAX_GADGETS = 6;

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
	[SerializeField] private TMP_Text selectionScreenTitle, selectedItemText, equipText, buyText;

	[SerializeField] private GameObject equipButton, repairButton, buyButton;

	[SerializeField] private Image selectedHullImage, selectedTurretImage, selectedAbilityImage, selectedGadgetImage;

	//hulls & turrets
	[SerializeField] private List<string> hullNames;
	[SerializeField] private List<Sprite> hullSprites;
	[SerializeField] private List<int> hullRepairDurabilities; //# of times hull can be used
	[SerializeField] private List<int> hullRepairTimes; //sec, multiplied by number of uses

	[SerializeField] private List<string> turretNames;
	[SerializeField] private List<Sprite> turretSprites;
	[SerializeField] private List<int> turretRepairDurabilities;
	[SerializeField] private List<int> turretRepairTimes;

	[SerializeField] private Camera playerCamera;
	public Camera GetPlayerCamera() { return playerCamera; }

	[SerializeField] private CameraBlur blur;
	public CameraBlur GetBlur() { return blur; }

	[SerializeField] private IconScriptableObj iconScriptObj;

	#endregion

	#region Members

	private List<string> gadgetNames;
	private List<Sprite> gadgetSprites;

	private List<string> abilityNames;
	private List<Sprite> abilitySprites;

	private readonly List<GameObject> spawnedButtons = new();

	private Vector3 normalCameraPosition;

	//interpolate to this
	private Vector3 targetGarageCameraPosition = new(-1.9f, 0.5f, -5.7f);
	private Vector3 targetGarageCameraRotation = new(7f, 0, 0);

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

	private bool selectedAbilityIsOn = false;

	private int currentSelectedMode = 0; //hull, turret, abilities

	#endregion

	#region Functions

	public Sprite GetHullSprite(string hullName) {
		for (int i = 0; i < hullNames.Count; i++) {
			if (hullNames[i] == hullName) return hullSprites[i];
		}
		return null;
	}
	public Sprite GetTurretSprite(string turretName) {
		for (int i = 0; i < turretNames.Count; i++) {
			if (turretNames[i] == turretName) return turretSprites[i];
		}
		return null;
	}
	private void ClearButtons() {
		foreach (GameObject g in spawnedButtons) {
			if (g != null) Destroy(g);
		}
		spawnedButtons.Clear();
	}
	public int GetHullDurability(string hullName) {
		for (int i = 0; i < hullNames.Count; i++) {
			if (hullNames[i] == hullName) return hullRepairDurabilities[i];
		}
		return 1;
	}
	public int GetTurretDurability(string turretName) {
		for (int i = 0; i < turretNames.Count; i++) {
			if (turretNames[i] == turretName) return turretRepairDurabilities[i];
		}
		return 1;
	}
	private void SetEquipButtonHull() {
		equipButton.SetActive(!PersistentDict.GetStringList("repair_names").Contains(selectedHullName) &&
			GetHullDurability(selectedHullName) - PersistentDict.GetInt("repair_uses_" + selectedHullName) > 0);
	}
	private void SetEquipButtonTurret() {
		equipButton.SetActive(!PersistentDict.GetStringList("repair_names").Contains(selectedTurretName) &&
			GetTurretDurability(selectedTurretName) - PersistentDict.GetInt("repair_uses_" + selectedTurretName) > 0);
	}
	private void SelectHull(string hullName, Sprite hullSprite) {
		selectedHullSprite = hullSprite;
		selectedHullName = hullName;

		selectedItemText.text = Translator.Translate(hullName);

		CheckRepairButtonHull();
		SetEquipButtonHull();
	}
	private void SelectTurret(string turretName, Sprite turretSprite) {
		selectedTurretName = turretName;
		selectedTurretSprite = turretSprite;

		selectedItemText.text = Translator.Translate(turretName);
		CheckRepairButtonTurret();

		SetEquipButtonTurret();
	}
	private void CheckRepairButtonTurret() {
		repairButton.SetActive(PersistentDict.GetInt("repair_uses_" + selectedTurretName) > 0 &&
			!PersistentDict.GetStringList("repair_names").Contains(selectedTurretName) &&
			RepairsManager.instance.HasRepairRoom());
		SetEquipButtonTurret();
	}
	private void CheckRepairButtonHull() {
		repairButton.SetActive(PersistentDict.GetInt("repair_uses_" + selectedHullName) > 0 &&
			!PersistentDict.GetStringList("repair_names").Contains(selectedHullName) &&
			RepairsManager.instance.HasRepairRoom());
		SetEquipButtonHull();
	}
	private void SelectGadget(string gadgetName, Sprite gadgetSprite) {
		selectedGadgetName = gadgetName;
		selectedGadgetSprite = gadgetSprite;

		selectedItemText.text = Translator.Translate(gadgetName);
		repairButton.SetActive(false);
		equipButton.SetActive(true);
	}
	private void SelectAbility(string abilityName, Sprite abilitySprite) {
		selectedAbilityName = abilityName;
		selectedAbilitySprite = abilitySprite;

		selectedItemText.text = Translator.Translate(abilityName);
		repairButton.SetActive(false);
		equipButton.SetActive(true);
	}
	private void ReselectHull() {
		for (int i = 0; i < hullNames.Count; i++) {
			if (hullNames[i] == PlayerPrefs.GetString("hull_name")) {
				SelectHull(hullNames[i], hullSprites[i]);
				break;
			}
		}
	}
	private void ReselectTurret() {
		for (int i = 0; i < turretNames.Count; i++) {
			if (turretNames[i] == PlayerPrefs.GetString("turret_name")) {
				SelectTurret(turretNames[i], turretSprites[i]);
				break;
			}
		}
	}
	public void OpenHulls() {
		//revert hull/turret to non-temp state
		MenuManager.instance.SetTurret(PlayerPrefs.GetString("turret_name"), false);
		ReselectHull();

		selectionScreenTitle.text = "HULLS";
		selectionGrid.cellSize = hullTurretCell;
		ClearButtons();

		for (int i = 0; i < hullNames.Count; i++) {
			GarageButton b = Instantiate(scrollHullTurretPrefab, selectionContentParent).GetComponent<GarageButton>();
			spawnedButtons.Add(b.gameObject);

			b.Init(hullNames[i], hullSprites[i], mode: 0, level: 0,
				equipped: hullNames[i] == PlayerPrefs.GetString("hull_name"));

			b.SetUsesLeft(hullRepairDurabilities[i] - PersistentDict.GetInt("repair_uses_" + hullNames[i]),
				hullRepairDurabilities[i]);
		}
		equipText.text = Translator.Translate("EQUIP");
		currentSelectedMode = 0;

		UpdateRepairStatus();
	}
	public void OpenTurrets() {
		//revert hull/turret to non-temp state
		MenuManager.instance.SetHull(PlayerPrefs.GetString("hull_name"), false);
		ReselectTurret();

		for (int i = 0; i < turretNames.Count; i++) {
			if (turretNames[i] == PlayerPrefs.GetString("turret_name")) {
				SelectTurret(turretNames[i], turretSprites[i]);
				break;
			}
		}

		selectionScreenTitle.text = "TURRETS";
		selectionGrid.cellSize = hullTurretCell;
		ClearButtons();

		for (int i = 0; i < turretNames.Count; i++) {
			GarageButton b = Instantiate(scrollHullTurretPrefab, selectionContentParent).GetComponent<GarageButton>();
			spawnedButtons.Add(b.gameObject);

			b.Init(turretNames[i], turretSprites[i], mode: 1, level: 0,
				equipped: turretNames[i] == PlayerPrefs.GetString("turret_name"));

			b.SetUsesLeft(turretRepairDurabilities[i] - PersistentDict.GetInt("repair_uses_" + turretNames[i]),
				turretRepairDurabilities[i]);
		}
		equipText.text = Translator.Translate("EQUIP");
		currentSelectedMode = 1;

		UpdateRepairStatus();
	}
	public void OpenGadgets() {
		//revert hull/turret to non-temp state
		MenuManager.instance.SetHull(PlayerPrefs.GetString("hull_name"), false);
		MenuManager.instance.SetTurret(PlayerPrefs.GetString("turret_name"), false);

		SetGadgetsText();
		selectionGrid.cellSize = abilityCell;
		ClearButtons();

		for (int i = 0; i < gadgetNames.Count; i++) {
			GarageButton b = Instantiate(scrollAbilityPrefab, selectionContentParent).GetComponent<GarageButton>();
			spawnedButtons.Add(b.gameObject);

			bool equipped = PlayerPrefs.GetInt("gadget_" + gadgetNames[i]) == 1;
			b.Init(gadgetNames[i], gadgetSprites[i], mode: 2, level: 0, equipped: equipped);

			if (i == 0) equipText.text = equipped ? Translator.Translate("UNEQUIP") : Translator.Translate("EQUIP");
		}
		currentSelectedMode = 2;

		repairButton.SetActive(false);

		SelectGadget(gadgetNames[0], gadgetSprites[0]);
	}
	public void OpenAbilities() {
		//revert hull/turret to non-temp state
		MenuManager.instance.SetHull(PlayerPrefs.GetString("hull_name"), false);
		MenuManager.instance.SetTurret(PlayerPrefs.GetString("turret_name"), false);

		SetAbilitiesText();
		selectionGrid.cellSize = abilityCell;
		ClearButtons();

		for (int i = 0; i < abilityNames.Count; i++) {
			GarageButton b = Instantiate(scrollAbilityPrefab, selectionContentParent).GetComponent<GarageButton>();
			spawnedButtons.Add(b.gameObject);

			bool equipped = PlayerPrefs.GetInt("ability_" + abilityNames[i]) == 1;
			b.Init(abilityNames[i], abilitySprites[i], mode: 3, level: 0, equipped: equipped);

			if (i == 0) equipText.text = equipped ? Translator.Translate("UNEQUIP") : Translator.Translate("EQUIP");
		}
		currentSelectedMode = 3;

		repairButton.SetActive(false);

		SelectAbility(abilityNames[0], abilitySprites[0]);
	}
	//called every second through RepairsManager and in startup
	public void UpdateRepairStatus() {
		if (currentSelectedMode > 1) return;

		List<string> itemsInRepair = PersistentDict.GetStringList("repair_names");

		foreach (GameObject g in spawnedButtons) {
			if (g == null || !g.TryGetComponent(out GarageButton b)) continue;

			bool repairing = itemsInRepair.Contains(b.GetItemName());
			b.SetRepairing(repairing);
		}
	}
	//sends selected to repair shop; TODO: do the same thing for upgrade
	public void SendToRepair() {
		if (currentSelectedMode > 1) return; //wrong mode; TODO: remove repair button in other modes!

		//NOTE: hull/turret must not be equipped when sending to repair shop
		string selected = currentSelectedMode == 0 ? selectedHullName : selectedTurretName;
		string current = currentSelectedMode == 0 ? PlayerPrefs.GetString("hull_name") : PlayerPrefs.GetString("turret_name");

		//not equipped (NOTE: not enforced anymore)
		if (selected != current || true) {
			List<int> repairTimers = PersistentDict.GetIntList("repair_timers");
			List<string> repairNames = PersistentDict.GetStringList("repair_names");

			//already in repair or don't need repair!
			if (repairNames.Contains(selected) || PersistentDict.GetInt("repair_uses_" + selected) == 0) return;

			int time = 20; //fallback 20s

			if (currentSelectedMode == 0) {
				for (int i = 0; i < hullNames.Count; i++) {
					if (hullNames[i] == selected) {
						time = hullRepairTimes[i] * PersistentDict.GetInt("repair_uses_" + selected);
						break;
					}
				}
			} else {
				for (int i = 0; i < turretNames.Count; i++) {
					if (turretNames[i] == selected) {
						time = turretRepairTimes[i] * PersistentDict.GetInt("repair_uses_" + selected);
						break;
					}
				}
			}

			repairNames.Add(selected);
			repairTimers.Add(time);

			PersistentDict.SetIntList("repair_timers", repairTimers);
			PersistentDict.SetStringList("repair_names", repairNames);

			UpdateRepairing(selected, true);
		}
		if (currentSelectedMode == 0) {
			CheckRepairButtonHull();
		} else {
			CheckRepairButtonTurret();
		}
	}
	private void EquipItem(bool saveAbilities) {
		if (currentSelectedMode == 0) {
			//equip hull
			if (!PersistentDict.GetStringList("repair_names").Contains(selectedHullName)) {
				MenuManager.instance.SetHull(selectedHullName, isTemporary: false);
				UpdateEquipped(selectedHullName, false, true);
				selectedHullImage.sprite = selectedHullSprite;
			}
			CheckRepairButtonHull();
		} else if (currentSelectedMode == 1) {
			//equip turret
			if (!PersistentDict.GetStringList("repair_names").Contains(selectedTurretName)) {
				MenuManager.instance.SetTurret(selectedTurretName, isTemporary: false);
				UpdateEquipped(selectedTurretName, false, true);
				selectedTurretImage.sprite = selectedTurretSprite;
			}
			CheckRepairButtonTurret();
		} else if (currentSelectedMode == 2) {
			//equip gadget if under max
			if (!saveAbilities || selectedAbilityIsOn || CountGadgets() < MAX_GADGETS) {
				bool equipped = UpdateEquipped(selectedGadgetName, true, false);
				PlayerPrefs.SetInt("gadget_" + selectedGadgetName, equipped ? 1 : 0);
				if (saveAbilities) SaveAbilities();
				selectedAbilityIsOn = equipped;

				SetGadgetsText();

				if (equipped) {
					selectedGadgetImage.sprite = selectedGadgetSprite;
				}
				equipText.text = equipped ? Translator.Translate("UNEQUIP") : Translator.Translate("EQUIP");
			}

		} else {
			//equip ability if under max
			if (!saveAbilities || selectedAbilityIsOn || CountAbilities() < MAX_ABILITIES) {
				bool equipped = UpdateEquipped(selectedAbilityName, true, false);
				PlayerPrefs.SetInt("ability_" + selectedAbilityName, equipped ? 1 : 0);
				if (saveAbilities) SaveAbilities();
				selectedAbilityIsOn = equipped;

				SetAbilitiesText();

				if (equipped) {
					selectedAbilityImage.sprite = selectedAbilitySprite;
				}
				equipText.text = equipped ? Translator.Translate("UNEQUIP") : Translator.Translate("EQUIP");
			}
		}
	}
	private void SetAbilitiesText() {
		selectionScreenTitle.text = $"ABILITIES ({CountAbilities()}/{MAX_ABILITIES})";
	}
	private void SetGadgetsText() {
		selectionScreenTitle.text = $"GADGETS ({CountGadgets()}/{MAX_GADGETS})";
	}
	public void EquipItem() {
		EquipItem(true);
	}
	private int CountGadgets() {
		int gadgets = 0;
		for (int i = 0; i < gadgetNames.Count; i++) {
			if (PlayerPrefs.GetInt("gadget_" + gadgetNames[i]) == 1) {
				gadgets++;
			}
		}
		return gadgets;
	}
	private int CountAbilities() {
		int abilities = 0;
		for (int i = 0; i < abilityNames.Count; i++) {
			if (PlayerPrefs.GetInt("ability_" + abilityNames[i]) == 1) {
				abilities++;
			}
		}
		return abilities;
	}
	//loads gadgets and abilities into one big persistentdict list
	private void SaveAbilities() {
		List<string> abilitiesToLoad = new();
		List<string> gadgetsToLoad = new();

		for (int i = 0; i < gadgetNames.Count; i++) {
			if (PlayerPrefs.GetInt("gadget_" + gadgetNames[i]) == 1) {
				gadgetsToLoad.Add(gadgetNames[i]);
			}
		}
		for (int i = 0; i < abilityNames.Count; i++) {
			if (PlayerPrefs.GetInt("ability_" + abilityNames[i]) == 1) {
				abilitiesToLoad.Add(abilityNames[i]);
			}
		}
		PersistentDict.SetStringList("active_abilities", abilitiesToLoad);
		PersistentDict.SetStringList("active_gadgets", gadgetsToLoad);
	}
	private void UpdateRepairing(string itemName, bool isRepairing) {
		foreach (GameObject g in spawnedButtons) {
			if (g == null || !g.TryGetComponent(out GarageButton b)) continue;

			if (b.GetItemName() == itemName) {
				b.SetRepairing(isRepairing);
				return;
			}
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
	public void ScreenButtonClicked(string itemName, Sprite sprite, int mode, bool isEnabled) {
		if (mode == 0) {
			SelectHull(itemName, sprite);
			MenuManager.instance.SetHull(itemName, isTemporary: true);
		} else if (mode == 1) {
			SelectTurret(itemName, sprite);
			MenuManager.instance.SetTurret(itemName, isTemporary: true);
		} else if (mode == 2) {
			equipText.text = isEnabled ? Translator.Translate("UNEQUIP") :
				 Translator.Translate("EQUIP");
			selectedAbilityIsOn = isEnabled;
			SelectGadget(itemName, sprite);
		} else {
			equipText.text = isEnabled ? Translator.Translate("UNEQUIP") : Translator.Translate("UNEQUIP");
			selectedAbilityIsOn = isEnabled;
			SelectAbility(itemName, sprite);
		}
	}
	public void OpenGarageTab() {
		garageUI.gameObject.SetActive(true);
		MenuManager.instance.SetMenuScreen(false);
		playerHealthCanvas.SetActive(false);
		inGarage = true;
		blur.SetBlur(1);

		Init();
	}
	public void CloseGarageTab() {
		garageUI.gameObject.SetActive(false);
		MenuManager.instance.SetMenuScreen(true);
		playerHealthCanvas.SetActive(true);
		blur.SetBlur(0);

		//revert hull/turret to non-temp state
		MenuManager.instance.SetHull(PlayerPrefs.GetString("hull_name"), false);
		MenuManager.instance.SetTurret(PlayerPrefs.GetString("turret_name"), false);

		MenuManager.instance.SetLastClosed(0);

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

	#region Awake & Start & Update

	private void Awake() {
		instance = this;
	}
	private void Start() {
		Init();
	}
	private void Init() {
		//abilitySprites
		//icons setup
		abilitySprites = new();
		abilityNames = new();

		gadgetSprites = new();
		gadgetNames = new();

		var icons = iconScriptObj.GetAbilitiesIcons();

		foreach (var i in icons) {
			abilitySprites.Add(i.icon);
			abilityNames.Add(i.id);
		}

		var icons2 = iconScriptObj.GetGadgetIcons();

		foreach (var i in icons2) {
			gadgetSprites.Add(i.icon);
			gadgetNames.Add(i.id);
		}

		for (int i = 0; i < hullNames.Count; i++) {
			if (hullNames[i] == PlayerPrefs.GetString("hull_name")) {
				SelectHull(hullNames[i], hullSprites[i]);
				break;
			}
		}
		currentSelectedMode = 0;
		EquipItem(false);

		for (int i = 0; i < turretNames.Count; i++) {
			if (turretNames[i] == PlayerPrefs.GetString("turret_name")) {
				SelectTurret(turretNames[i], turretSprites[i]);
				break;
			}
		}
		currentSelectedMode = 1;
		EquipItem(false);

		currentSelectedMode = 2;
		for (int i = 0; i < gadgetNames.Count; i++) {
			if (PlayerPrefs.GetInt("gadget_" + gadgetNames[i]) == 1) {
				SelectGadget(gadgetNames[i], gadgetSprites[i]);
				EquipItem(false);
			}
		}

		currentSelectedMode = 3;
		for (int i = 0; i < abilityNames.Count; i++) {
			if (PlayerPrefs.GetInt("ability_" + abilityNames[i]) == 1) {
				SelectAbility(abilityNames[i], abilitySprites[i]);
				EquipItem(false);
			}
		}

		normalCameraPosition = playerCamera.transform.position;

		OpenHulls();
	}

	private void Update() {
		if (garageUI.gameObject.activeInHierarchy) {
			Vector3 target = targetGarageCameraPosition;

			//standard aspect ratio adjustment
			float aspectRatio = (float)Screen.width / Screen.height / (1920f / 1080f);
			aspectRatio = (aspectRatio + 1f) / 2f;
			target.x *= aspectRatio;

			playerCamera.transform.position = Vector3.MoveTowards(playerCamera.transform.position,
				target, Time.deltaTime * 10f);
			playerCamera.transform.rotation = Quaternion.RotateTowards(playerCamera.transform.rotation,
				Quaternion.Euler(targetGarageCameraRotation), Time.deltaTime * 25f);
		} else if (MenuManager.instance.GetLastClosedId() == 0 && !RepairsManager.instance.GetIsInRepairs() &&
			!UpgradesManager.instance.GetIsInUpgrades()) {
			playerCamera.transform.position = Vector3.MoveTowards(playerCamera.transform.position,
				normalCameraPosition, Time.deltaTime * 10f);
			playerCamera.transform.rotation = Quaternion.RotateTowards(playerCamera.transform.rotation,
				Quaternion.identity, Time.deltaTime * 25f);
		}
	}

	#endregion
}
