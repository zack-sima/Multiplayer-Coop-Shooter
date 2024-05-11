using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CSV;
using JSON;
using Unity.VisualScripting;
using System;

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

	//*============| GarageScreen |===========*//

	[Header("MainGarageUI")]
	[SerializeField] private TextMeshProUGUI damageStatMainGarage;
	[SerializeField] private TextMeshProUGUI damageRateMainGarage, fireRateMainGarage, healthStatMainGarage;
	[SerializeField] private TextMeshProUGUI tempUpgradeButton, tempUpgradeHullButton;

	private void UpdateStatsDisplayMainGarage(bool isHull = false, bool isUpgrade = false) {
		if(turretInfos.TryGetValue(selectedTurretName, out GarageInfo turretInfo) &&
			hullInfos.TryGetValue(selectedHullName, out GarageInfo hullInfo)) {
			var turretStats = turretInfo.GetCurrentStats(turretInfo.currentLevel);
			var hullStats = hullInfo.GetCurrentStats(hullInfo.currentLevel);

			if (!isHull && turretStats != null && turretStats.TryGetValue(nameof(UpgradeInfo.ModiName.Damage), out float damage) && turretStats.TryGetValue(nameof(UpgradeInfo.ModiName.FireRate), out float fireRate)) {
				if (isUpgrade) {
					if (turretInfo.GetCurrentStats(turretInfo.currentLevel - 1).TryGetValue(nameof(UpgradeInfo.ModiName.Damage), out float prevDamage) && turretInfo.GetCurrentStats(turretInfo.currentLevel - 1).TryGetValue(nameof(UpgradeInfo.ModiName.FireRate), out float prevFireRate)) {
						upgradeAnimations.Add(StartCoroutine(AnimateText((int)prevDamage, (int)damage, 3f, damageStatMainGarage)));
						upgradeAnimations.Add(StartCoroutine(AnimateText((int)prevFireRate, (int)fireRate, 3f, fireRateMainGarage, true)));
						upgradeAnimations.Add(StartCoroutine(AnimateText((int)(prevDamage * prevFireRate), (int)(damage * fireRate), 3f, damageRateMainGarage, true)));
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
						upgradeAnimations.Add(StartCoroutine(AnimateText((int)prevHealth, (int)health, 3f, healthStatMainGarage)));
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
				StopAllUpgradeAnimations();
				UpdateStatsDisplayMainGarage(isHull: true, isUpgrade: true);
				Debug.LogWarning("Upgraded");
			}
			if (hullInfo.GetIsMax() == hullInfo.currentLevel) {
				Debug.LogWarning("Maxed out");
				tempUpgradeHullButton.text = "MAXED";
			}
		}
	
	}

	public void UpgradeButtonClicked() {
		if (turretInfos.TryGetValue(selectedTurretName, out GarageInfo turretInfo) &&
			hullInfos.TryGetValue(selectedHullName, out GarageInfo hullInfo)) {
			//if (turretInfo.) // TODO: when level == 0, locked.
			if (turretInfo.GetIsMax() > turretInfo.currentLevel) {
				turretInfo.currentLevel++;
				StopAllUpgradeAnimations();
				UpdateStatsDisplayMainGarage(isHull: false, isUpgrade: true);
				Debug.LogWarning("Upgraded");
			}
			if (turretInfo.GetIsMax() == turretInfo.currentLevel) {
				Debug.LogWarning("Maxed out");
				tempUpgradeButton.text = "MAXED";
			}
		}
	}

	public void ResetUpgradesButtonClicked() {
		if (turretInfos.TryGetValue(selectedTurretName, out GarageInfo turretInfo) &&
			hullInfos.TryGetValue(selectedHullName, out GarageInfo hullInfo)) {
			turretInfo.currentLevel = 0;
			hullInfo.currentLevel = 0;
			UpdateStatsDisplayMainGarage();
			tempUpgradeButton.text = "UPGRADE";
			tempUpgradeHullButton.text = "UPGRADE";
		}
	}

	//*============| CatalogScreen |===========*//

	//*============| TurretScreen |===========*// 
	//? IDK if u want these two to be different or not, but I'm just gonna make them the different for now ?

	//*============| HullScreen |===========*//

	//*============| Internals |===========*//

	private void CloseAllScreens() {
		if (catalogScreen.activeInHierarchy) catalogScreen.SetActive(false);
		if (turretScreen.activeInHierarchy) turretScreen.SetActive(false);
		if (hullScreen.activeInHierarchy) hullScreen.SetActive(false);
		if (garageScreen.activeInHierarchy) garageScreen.SetActive(false);
	}

	//*============| Backend |===========*//

	//private class temp

	//*============| Animations |===========*//

	private bool finishAnimating = false;
	private List<Coroutine> upgradeAnimations = new();

	private float LogarithmicLerp(float start, float end, float value) {
		float scale = end - start;
		return start + scale * Mathf.Log10(10 * value); // Logarithmic easing
	}

	private void StopAllUpgradeAnimations() {
		foreach (Coroutine i in upgradeAnimations) {
			StopCoroutine(i);
		}
		upgradeAnimations.Clear();
		PushWhiteTextMeshProGUIUpgradeAnimations();
	}

	private void PushWhiteTextMeshProGUIUpgradeAnimations() {
		damageRateMainGarage.color = Color.white;
		damageStatMainGarage.color = Color.white;
		fireRateMainGarage.color = Color.white;
		healthStatMainGarage.color = Color.white;
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




	//TODO: Push and pulling from persistent data, Loadouts, UIUpdating, etc.

	private void Awake() {
		instance = this;

		turretInfos = JSONParser.InitBlankTurretInfo();
		hullInfos = JSONParser.InitBlankHullInfo();
		//upgradeInfos = JSONParser.InitBlankUpgradesInfo();
		foreach (KeyValuePair<string, GarageInfo> kvp in turretInfos) {
			Debug.LogWarning(kvp.Key);
		}
		foreach (KeyValuePair<string, GarageInfo> kvp in hullInfos) {
			Debug.LogWarning(kvp.Key);
		}

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

	#endregion

}
