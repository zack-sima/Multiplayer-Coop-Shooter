using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CSV;
using JSON;
using Unity.VisualScripting;
using ExitGames.Client.Photon.StructWrapping;

public class GarageManager : MonoBehaviour {

	#region Statics & Classes

	public static GarageManager instance;

	//TODO: parse this through CSV system and support multiple levels
	//TODO: actually set hull/turret speed, etc with this
	[System.Serializable]
	public class HullStats {
		public int hp;
		public double speed;
	}
	[System.Serializable]
	public class TurretStats {
		public int damage;
		public int ammo;
		public double shootSpeed;
	}

	#endregion

	#region Prefabs

	[SerializeField] private GameObject scrollButtonPrefab;

	#endregion

	#region References

	[SerializeField] private RectTransform garageUI;
	[SerializeField] private Image garageImage;

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

	// [SerializeField] //stat displays; child 0 = text, child 1 = bar
	// private RectTransform damageDisplay, healthDisplay,
	// 	speedDisplay, shootRateDisplay, ammoDisplay;

	[SerializeField] private GameObject upgradeMenu;

	[Header("Garage CSVs")]
	[Tooltip("Requires a .CSV to function. Auto-Parses and generates out-game UI selections.")]
	[SerializeField] public TextAsset turretCSVProps;
	[SerializeField] public TextAsset hullCSVProps;

	#endregion

	#region Members

	private readonly List<GameObject> spawnedButtons = new();

	private Vector3 normalCameraPosition;

	//interpolate to this
	private Vector3 targetGarageCameraPosition = new Vector3(-.3f, 1.4f, -5.7f);//new(-1f, 1.8f, -5.5f);

	private bool hullMode = true;

	private bool inGarage = false;
	public bool GetIsInGarage() { return inGarage; }

	//stats display; TODO: incorporate into CSV system and actually affect hulls/turrets
	private string selectedHullName = "", selectedTurretName = "";
	private Dictionary<string, HullStats> hullStats;
	private Dictionary<string, TurretStats> turretStats;

	private Dictionary<string, GarageInfo> turretInfos = new (), hullInfos = new();
	private Dictionary<string, UpgradeInfo> upgradeInfos = new();
	private (HullStats, TurretStats) selectedStats;

	//max stats (for bar graph comparisons); TODO: actually use best hull/turret
	private HullStats bestHull;
	private TurretStats bestTurret;

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
		UpdateStatsDisplayMainGarage(); // Temp
	}
	private void SelectTurret(string turretName, Sprite turretSprite) {
		selectedTurretImage.sprite = turretSprite;
		selectedTurretName = turretName;

		CloseSelectionScreen();
		UpdateStatsDisplayMainGarage(); // Temp
	}
	private void UpdateStats() {
		// if (!turretStats.ContainsKey(selectedTurretName) ||
		// 	!hullStats.ContainsKey(selectedHullName)) return;

		// damageDisplay.GetChild(0).GetComponent<TMP_Text>().text =
		// 	$"{turretStats[selectedTurretName].damage}";
		// ammoDisplay.GetChild(0).GetComponent<TMP_Text>().text =
		// 	$"{turretStats[selectedTurretName].ammo}";
		// shootRateDisplay.GetChild(0).GetComponent<TMP_Text>().text =
		// 	$"{turretStats[selectedTurretName].shootSpeed:0.0}/s";
		// speedDisplay.GetChild(0).GetComponent<TMP_Text>().text =
		// 	$"{hullStats[selectedHullName].speed:0.0}m/s";
		// healthDisplay.GetChild(0).GetComponent<TMP_Text>().text =
		// 	$"{hullStats[selectedHullName].hp}";

		// damageDisplay.GetChild(1).localScale = new Vector2(
		// 	turretStats[selectedTurretName].damage / (float)bestTurret.damage, 1);
		// ammoDisplay.GetChild(1).localScale = new Vector2(
		// 	turretStats[selectedTurretName].ammo / (float)bestTurret.ammo, 1);
		// shootRateDisplay.GetChild(1).localScale = new Vector2(
		// 	(float)(turretStats[selectedTurretName].shootSpeed / bestTurret.shootSpeed), 1);
		// speedDisplay.GetChild(1).localScale = new Vector2(
		// 	(float)(hullStats[selectedHullName].speed / bestHull.speed), 1);
		// healthDisplay.GetChild(1).localScale = new Vector2(
		// 	(float)hullStats[selectedHullName].hp / bestHull.hp, 1);

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

	//*============| UICallbacks |===========*//

	//TODO: implement the actual ui lol.
	[SerializeField] private GameObject catalogScreen, turretScreen, hullScreen, garageScreen;

	public void OpenHullsScreen() {
		CloseAllScreens();
		hullScreen.SetActive(true);
	}

	public void OpenTurretsScreen() {
		CloseAllScreens();
		turretScreen.SetActive(true);
	}

	public void OpenCatalogScreen() { 
		CloseAllScreens();
		catalogScreen.SetActive(true);
	}

	public void OpenGarageScreen() { 
		CloseAllScreens();
		garageScreen.SetActive(true);
	}
	#region GarageScreen
	//*============| GarageScreen |===========*//

	[Header("MainGarageUI")]
	[SerializeField] private TextMeshProUGUI damageStatMainGarage;
	[SerializeField] private TextMeshProUGUI damageRateMainGarage, fireRateMainGarage, healthStatMainGarage;
	[SerializeField] private TextMeshProUGUI tempUpgradeButton, tempUpgradeHullButton;
	[SerializeField] private GameObject hullLevelMainGarage, turretLevelMainGarage;
	[SerializeField] private List<GameObject> activeSelectionButtons, gadgetSelectionButtons;
	[SerializeField] private List<GameObject> loadoutSelectionButtons;

	private uint activeSlotsUnlocked = 2, gadgetSlotsUnlocked = 2;
	private int loadoutIndexSlotSelected; // TODO: Update the loadout slot from the persistance dict.
	private bool loadoutStartingPage = false;
	private Color defaultColorLoadoutButton, defaultTopBevelColorLoadoutButton, defaultBottomBevelColorLoadoutButton;

	private void UpdateNSaveLoadout() {
		//TODO: Implement saving the loadout.
		//TODO: Call when the back button is pressed.
	}

	private void LoadoutChangedCallBack(int index) {
		//TODO: Implement this, load the stored loadout.
	}

	public void LoadOutButtonCallBack(int index) {
		if (index >= loadoutSelectionButtons.Count) return;
		ResetAllLoadoutButtons();
		if (loadoutStartingPage) {
			if (index == 3) {
				loadoutStartingPage = false;
				UpdateLoadoutButtons();
			} else { 
				loadoutIndexSlotSelected = index;
				UpdateNSaveLoadout();
				SetActiveLoadoutButton(index);
				LoadoutChangedCallBack(index);
			}
		} else {
			if (index == 0) {
				loadoutStartingPage = true;
				UpdateLoadoutButtons(); 
			} else { 
				loadoutIndexSlotSelected = index + 3;
				UpdateNSaveLoadout();
				SetActiveLoadoutButton(index + 3);
				LoadoutChangedCallBack(index + 3);
			}
		}
		StartCoroutine(ButtonImageAnimation(loadoutSelectionButtons[index].GetComponent<Image>(), .25f));
	}

	private void ResetAllLoadoutButtons() {
		foreach(GameObject g in loadoutSelectionButtons) {
			if (g != null && g.GetComponent<Image>() != null) {
				g.GetComponent<Image>().color = defaultColorLoadoutButton;
				Transform top = g.transform.Find("TopBevel");
				if (top != null) top.GetComponent<Image>().color = defaultTopBevelColorLoadoutButton;
				Transform bottom = g.transform.Find("BottomBevel");
				if (bottom != null) bottom.GetComponent<Image>().color = defaultBottomBevelColorLoadoutButton;
			}
		}
	}

	private void SetActiveLoadoutButton(int index) {
		Color addColor = new Color(-.2f, -.2f, -.2f);
		if (index < 3) {
			if (loadoutStartingPage) { 
				loadoutSelectionButtons[index].GetComponent<Image>().color += addColor;  
				loadoutSelectionButtons[index].transform.Find("TopBevel").GetComponent<Image>().color += addColor * .7f;
				loadoutSelectionButtons[index].transform.Find("BottomBevel").GetComponent<Image>().color += addColor * .7f;
			}
		} else {
			if (!loadoutStartingPage) { 
				loadoutSelectionButtons[index - 3].GetComponent<Image>().color += addColor; 
				loadoutSelectionButtons[index - 3].transform.Find("TopBevel").GetComponent<Image>().color += addColor * .7f;
				loadoutSelectionButtons[index - 3].transform.Find("BottomBevel").GetComponent<Image>().color += addColor * .7f;
			}
		}
	}

	private void InitLoadoutButtons() {
		defaultColorLoadoutButton = loadoutSelectionButtons[0].GetComponent<Image>().color;
		defaultTopBevelColorLoadoutButton = loadoutSelectionButtons[0].transform.Find("TopBevel").GetComponent<Image>().color;
		defaultBottomBevelColorLoadoutButton = loadoutSelectionButtons[0].transform.Find("BottomBevel").GetComponent<Image>().color;
		loadoutIndexSlotSelected = 0; // TODO: SET WHAT LOADOUT IS SELECTED
		if (loadoutIndexSlotSelected < 3) { 
			loadoutStartingPage = true;
			SetActiveLoadoutButton(loadoutIndexSlotSelected);
		} else {
			loadoutStartingPage = false;
			SetActiveLoadoutButton(loadoutIndexSlotSelected + 3);
		}
		UpdateLoadoutButtons();
	}

	private void UpdateLoadoutButtons() {
		if (loadoutStartingPage) {
			loadoutSelectionButtons[0].transform.Find("StatText")?.gameObject.SetActive(true);
			loadoutSelectionButtons[0].transform.Find("Icon")?.gameObject.SetActive(false);
			loadoutSelectionButtons[3].transform.Find("StatText")?.gameObject.SetActive(false);
			loadoutSelectionButtons[3].transform.Find("Icon")?.gameObject.SetActive(true);

			TextMeshProUGUI text1 = loadoutSelectionButtons[0].transform.Find("StatText")?.gameObject.GetComponent<TextMeshProUGUI>();
			if (text1 != null) text1.text = "1";
			TextMeshProUGUI text2 = loadoutSelectionButtons[1].transform.Find("StatText")?.gameObject.GetComponent<TextMeshProUGUI>();
			if (text2 != null) text2.text = "2";
			TextMeshProUGUI text3 = loadoutSelectionButtons[2].transform.Find("StatText")?.gameObject.GetComponent<TextMeshProUGUI>();
			if (text3 != null) text3.text = "3";
		} else {
			loadoutSelectionButtons[0].transform.Find("StatText")?.gameObject.SetActive(false);
			loadoutSelectionButtons[0].transform.Find("Icon")?.gameObject.SetActive(true);
			loadoutSelectionButtons[3].transform.Find("StatText")?.gameObject.SetActive(true);
			loadoutSelectionButtons[3].transform.Find("Icon")?.gameObject.SetActive(false);

			TextMeshProUGUI text1 = loadoutSelectionButtons[1].transform.Find("StatText")?.gameObject.GetComponent<TextMeshProUGUI>();
			if (text1 != null) text1.text = "4";
			TextMeshProUGUI text2 = loadoutSelectionButtons[2].transform.Find("StatText")?.gameObject.GetComponent<TextMeshProUGUI>();
			if (text2 != null) text2.text = "5";
			TextMeshProUGUI text3 = loadoutSelectionButtons[3].transform.Find("StatText")?.gameObject.GetComponent<TextMeshProUGUI>();
			if (text3 != null) text3.text = "6";
		} 
	}

	private void InitSelectionNodes() {
		uint actives = 1, gadgets = 1;
		foreach(GameObject g in activeSelectionButtons) {
			Transform active = g.transform.Find("Active");
			active?.GameObject().SetActive(false);
			Transform button = g.transform.Find("Button");
			if (button != null) {
				button.GetComponent<Image>().enabled = true;
				Color c = button.GetComponent<Image>().color; c.a = 0;
				button.GetComponent<Image>().color = c;
			}
			if (actives <= activeSlotsUnlocked) {
				Transform lockBG = g.transform.Find("LockBG");
				lockBG?.GameObject().SetActive(false);
			} else {
				Transform regBG = g.transform.Find("RegBG");
				regBG?.GameObject().SetActive(false);
			}
			//TODO: Init the selections based on the loadout.
			actives++;
		}
		foreach(GameObject g in gadgetSelectionButtons) {
			Transform gadget = g.transform.Find("Gadget");
			gadget?.GameObject().SetActive(false);
			Transform button = g.transform.Find("Button");
			if (button != null) {
				button.GetComponent<Image>().enabled = true;
				Color c = button.GetComponent<Image>().color; c.a = 0;
				button.GetComponent<Image>().color = c;
			}
			if (gadgets <= gadgetSlotsUnlocked) {
				Transform lockBG = g.transform.Find("LockBG");
				lockBG?.GameObject().SetActive(false);
			} else {
				Transform regBG = g.transform.Find("RegBG");
				regBG?.GameObject().SetActive(false);
			}
			gadgets++;
		}
	}

	public void ActiveSelectionNodeCallBack(int index) {
		if (index >= activeSelectionButtons.Count) return;
		if (index > activeSlotsUnlocked - 1) {
			StartCoroutine(ButtonImageAnimation(activeSelectionButtons[index].transform.Find("LockBG").GetComponent<Image>(), .25f));
			return;
		}
		if (activeSelectionButtons[index].transform.Find("Active") != null 
			&& activeSelectionButtons[index].transform.Find("Active").gameObject.activeInHierarchy) {
				StartCoroutine(ButtonImageAnimation(activeSelectionButtons[index].transform.Find("Active").GetComponent<Image>(), .25f));
				return;
		}
		StartCoroutine(ButtonImageAnimation(activeSelectionButtons[index].transform.Find("RegBG").GetComponent<Image>(), .25f));
	}

	public void GadgetSelectionButtonsCallBack(int index) {
		if (index >= gadgetSelectionButtons.Count) return;
		if (index > gadgetSlotsUnlocked - 1) {
			StartCoroutine(ButtonImageAnimation(gadgetSelectionButtons[index].transform.Find("LockBG").GetComponent<Image>(), .25f));
			return;
		}
		if (gadgetSelectionButtons[index].transform.Find("Gadget") != null 
			&& gadgetSelectionButtons[index].transform.Find("Gadget").gameObject.activeInHierarchy) {
				StartCoroutine(ButtonImageAnimation(gadgetSelectionButtons[index].transform.Find("Gadget").GetComponent<Image>(), .25f));
				return;
		}
		StartCoroutine(ButtonImageAnimation(gadgetSelectionButtons[index].transform.Find("RegBG").GetComponent<Image>(), .25f));
	}

	private void UpdateStatsDisplayMainGarage(bool isHull = false, bool isUpgrade = false) {
		if(turretInfos.TryGetValue(selectedTurretName, out GarageInfo turretInfo) &&
			hullInfos.TryGetValue(selectedHullName, out GarageInfo hullInfo)) {
			var turretStats = turretInfo.GetCurrentStats(turretInfo.currentLevel);
			var hullStats = hullInfo.GetCurrentStats(hullInfo.currentLevel);

			UpdateHullLevelText();
			UpdateTurretLevelText();
			CheckForMaxed();

			if (!isHull && turretStats != null && turretStats.TryGetValue(nameof(UpgradeInfo.ModiName.Damage), out float damage) && turretStats.TryGetValue(nameof(UpgradeInfo.ModiName.FireRate), out float fireRate)) {
				if (isUpgrade) {
					if (turretInfo != null && 
							turretInfo.GetCurrentStats(turretInfo.currentLevel - 1).TryGetValue(nameof(UpgradeInfo.ModiName.Damage), out float prevDamage) 
							&& turretInfo.GetCurrentStats(turretInfo.currentLevel - 1).TryGetValue(nameof(UpgradeInfo.ModiName.FireRate), out float prevFireRate)) {
						upgradeAnimationDict[0].Add(StartCoroutine(AnimateText((int)prevDamage, (int)damage, 3f, damageStatMainGarage)));
						upgradeAnimationDict[0].Add(StartCoroutine(AnimateText((int)prevFireRate, (int)fireRate, 3f, fireRateMainGarage, true)));
						upgradeAnimationDict[0].Add(StartCoroutine(AnimateText((int)(prevDamage * prevFireRate), (int)(damage * fireRate), 3f, damageRateMainGarage, true)));
						ApplyUpgradeEffect();
						UpdateTurretLevelText();
					}
				} else {
					damageStatMainGarage.text = damage.ToString();
					fireRateMainGarage.text = fireRate.ToString() + " /s";
					damageRateMainGarage.text = (damage * fireRate).ToString() + " /s";
				}
			}
			if (isHull && hullStats != null && hullStats.TryGetValue(nameof(UpgradeInfo.ModiName.Health), out float health)) {
				if (isUpgrade) {
					if (hullInfo.GetCurrentStats(hullInfo.currentLevel - 1).TryGetValue(nameof(UpgradeInfo.ModiName.Health), out float prevHealth)) {
						upgradeAnimationDict[1].Add(StartCoroutine(AnimateText((int)prevHealth, (int)health, 3f, healthStatMainGarage)));
						ApplyUpgradeEffect();
						UpdateHullLevelText();
					}
				} else {
					healthStatMainGarage.text = health.ToString();
				} 
			}
		}
	}

	public void UpgradeHullButtonClicked() {
		if (turretInfos.TryGetValue(selectedTurretName, out GarageInfo turretInfo) &&
			hullInfos.TryGetValue(selectedHullName, out GarageInfo hullInfo)) {
			if (hullInfo.GetIsMax() > hullInfo.currentLevel) {
				hullInfo.currentLevel++;
				StopUpgradeAnimations(1);
				UpdateStatsDisplayMainGarage(isHull: true, isUpgrade: true);
			}
		}
	
	}

	public void UpgradeTurretButtonClicked() {
		if (turretInfos.TryGetValue(selectedTurretName, out GarageInfo turretInfo) &&
			hullInfos.TryGetValue(selectedHullName, out GarageInfo hullInfo)) {
			//if (turretInfo.) // TODO: when level == 0, locked.
			if (turretInfo.GetIsMax() > turretInfo.currentLevel) {
				turretInfo.currentLevel++;
				StopUpgradeAnimations(0);
				UpdateStatsDisplayMainGarage(isHull: false, isUpgrade: true);
			}
		}
	}

	public void CheckForMaxed() {
		if (turretInfos.TryGetValue(selectedTurretName, out GarageInfo turretInfo) &&
			hullInfos.TryGetValue(selectedHullName, out GarageInfo hullInfo)) {
			if (turretInfo.GetIsMax() == turretInfo.currentLevel) {
				tempUpgradeButton.text = "MAXED";
			} else {
				tempUpgradeButton.text = "UPGRADE";
			}
			if (hullInfo.GetIsMax() == hullInfo.currentLevel) {
				tempUpgradeHullButton.text = "MAXED";
			} else {
				tempUpgradeHullButton.text = "UPGRADE";
			}
		}
	
	}

	public void ResetUpgradesButtonClicked() {
		turretInfos = JSONParser.InitBlankTurretInfo();
		hullInfos = JSONParser.InitBlankHullInfo();
		if (turretInfos.TryGetValue(selectedTurretName, out GarageInfo turretInfo) &&
			hullInfos.TryGetValue(selectedHullName, out GarageInfo hullInfo)) {
			turretInfo.currentLevel = 0;
			hullInfo.currentLevel = 0;
			StopUpgradeAnimations(0);
			StopUpgradeAnimations(1);
			UpdateStatsDisplayMainGarage(isHull: false, isUpgrade: false);
			UpdateStatsDisplayMainGarage(isHull: true, isUpgrade: false);
			tempUpgradeButton.text = "UPGRADE";
			tempUpgradeHullButton.text = "UPGRADE";
		}
	}

	private void UpdateHullLevelText() {
		if (hullInfos.TryGetValue(selectedHullName, out GarageInfo hullInfo)) {
			TextMeshProUGUI[] hullLevelText = hullLevelMainGarage.GetComponentsInChildren<TextMeshProUGUI>();
			if (hullLevelText != null && hullLevelText.Length > 0) {
				hullLevelText[0].text = (hullInfo.currentLevel + 1).ToString();
				//TODO: Image animation and colors, sounds, etc.
			}
		}
	}

	private void UpdateTurretLevelText() {
		if (turretInfos.TryGetValue(selectedTurretName, out GarageInfo turretInfo)) {
			TextMeshProUGUI[] turretLevelText = turretLevelMainGarage.GetComponentsInChildren<TextMeshProUGUI>();
			if (turretLevelText != null && turretLevelText.Length > 0) {
				turretLevelText[0].text = (turretInfo.currentLevel + 1).ToString();
			}
		}
	}
	#endregion

	//*============| CatalogScreen |===========*//

	//*============| TurretScreen |===========*// 
	//? IDK if u want these two to be different or not, but I'm just gonna make them the different for now ?
	//#region HullScreen
	//*============| HullScreen |===========*//
	//#region Internals
	//*============| Internals |===========*//

	private void CloseAllScreens() {
		if (catalogScreen.activeInHierarchy) catalogScreen.SetActive(false);
		if (turretScreen.activeInHierarchy) turretScreen.SetActive(false);
		if (hullScreen.activeInHierarchy) hullScreen.SetActive(false);
		if (garageScreen.activeInHierarchy) garageScreen.SetActive(false);
	}
	//#region Backend
	//*============| Backend |===========*//

	//private class temp
	//#endregion
	#region Animations
	//*============| Animations |===========*//

	private bool finishAnimating = false;

	[Header("Upgrade Animations")]
	[SerializeField] private GameObject mockPlayer;
	[SerializeField] private GameObject upgradeEffect;

	private Dictionary<int, List<Coroutine>> upgradeAnimationDict = new() {
		{ 0, new List<Coroutine>() },
		{ 1, new List<Coroutine>() },
		{ 2, new List<Coroutine>() },
		{ 3, new List<Coroutine>() },
		{ 4, new List<Coroutine>() }
	};

	//dim the image for the set duration
	private IEnumerator ButtonImageAnimation(Image buttonImage, float duration) {
		float currentTime = 0;
		if (buttonImage == null) yield break;
		Vector3 startingScale = buttonImage.transform.localScale;
		Color c = buttonImage.color;
		while (currentTime < duration) {
			currentTime += Time.deltaTime;
			c.a = Mathf.Lerp(.75f, 1f, currentTime / duration);
			buttonImage.transform.localScale = Vector3.Lerp(startingScale * .9f, startingScale, currentTime / duration);
			buttonImage.color = c;
			yield return null;
		}
		c.a = 1;
		buttonImage.color = c;
		buttonImage.transform.localScale = startingScale;
	}

	private void ApplyUpgradeEffect() {
		GameObject effect = Instantiate(upgradeEffect, mockPlayer.transform);
		Destroy(effect, 2f);
	}

	private float LogarithmicLerp(float start, float end, float value) {
		float scale = end - start;
		return start + scale * Mathf.Log10(10 * value); // Logarithmic easing
	}

	private void StopUpgradeAnimations(int index) {
		if (upgradeAnimationDict.ContainsKey(index)) {
			foreach (Coroutine i in upgradeAnimationDict[index]) {
				if (i != null) StopCoroutine(i);
			}
		}
		PushWhiteTextMeshProGUIUpgradeAnimations(index);
	}

	private void PushWhiteTextMeshProGUIUpgradeAnimations(int index) {
		if (index == 0) {
			damageRateMainGarage.color = Color.white;
			damageStatMainGarage.color = Color.white;
			fireRateMainGarage.color = Color.white;
		} else if (index == 1) {
			healthStatMainGarage.color = Color.white;
		}
	}

	private IEnumerator AnimateText(int startValue, int endValue, float duration, TextMeshProUGUI textComponent, bool isPerSec = false) {
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
	//TODO: Push and pulling from persistent data, Loadouts, UIUpdating, etc.

	#region Awake & Start & Update
	private void Awake() {
		instance = this;


		turretInfos = JSONParser.InitBlankTurretInfo(); // TEMP
		hullInfos = JSONParser.InitBlankHullInfo();
		//upgradeInfos = JSONParser.InitBlankUpgradesInfo();
		// foreach (KeyValuePair<string, GarageInfo> kvp in turretInfos) {
		// 	Debug.LogWarning(kvp.Key);
		// }
		// foreach (KeyValuePair<string, GarageInfo> kvp in hullInfos) {
		// 	Debug.LogWarning(kvp.Key);
		// }

		//TODO: temporary hard-coding for stats displays
		bestHull = new() { hp = 5000, speed = 5.0 };
		bestTurret = new() { ammo = 50, damage = 650, shootSpeed = 20 };

		HullStats tank = new() {
			hp = 5000,
			speed = 5.0
		};
		HullStats spider = new() {
			hp = 5000,
			speed = 5.0
		};
		TurretStats autocannon = new() {
			damage = 150,
			ammo = 35,
			shootSpeed = 10.0 //10/s
		};
		TurretStats explCannon = new() {
			damage = 650,
			ammo = 4,
			shootSpeed = 5.0
		};
		TurretStats gatling = new() {
			damage = 60,
			ammo = 50,
			shootSpeed = 20.0 //x2
		};
		TurretStats mortar = new() {
			damage = 600,
			ammo = 3,
			shootSpeed = 5.0
		};
		TurretStats flamer = new() {
			damage = 70,
			ammo = 50,
			shootSpeed = 15.0 //x2
		};
		TurretStats doubleCannon = new() {
			damage = 250,
			ammo = 20,
			shootSpeed = 6.5
		};

		hullStats = new() { { "Tank", tank }, { "Spider", spider } };
		turretStats = new() {
			{ "Autocannon", autocannon }, { "Explosive Cannon", explCannon},
			{ "Gatling", gatling }, { "Mortar", mortar }, { "Flamethrower", flamer },
			{ "Double Cannon", doubleCannon }
		};

	}

	//TODO: change menu manager dropdown values to correspond with hull & turret names in centralized file
	private void Start() {
		InitSelectionNodes(); // FOR SELECTION UI INITIALIZATION
		InitLoadoutButtons(); // FOR LOADOUT BUTTON INITIALIZATION
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

		//normalCameraPosition = playerCamera.transform.position = new Vector3(0.07f, 2f, -8f);
	}

	private void Update() {
		if (garageUI.gameObject.activeInHierarchy) {
			playerCamera.transform.position = Vector3.MoveTowards(playerCamera.transform.position,
				targetGarageCameraPosition, Time.deltaTime * 10f);
		} else {
			playerCamera.transform.position = Vector3.MoveTowards(playerCamera.transform.position,
				new Vector3(0.07f, 2f, -8f), Time.deltaTime * 10f);
		}
	}

	private void LateUpdate() {

	}

	#endregion
	#endregion
}
