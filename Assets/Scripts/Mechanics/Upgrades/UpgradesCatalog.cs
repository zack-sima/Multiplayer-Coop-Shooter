using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CSV;
using CSV.Parsers;

public class UpgradesCatalog : MonoBehaviour {

	#region Statics & Consts & Member Classes

	public static UpgradesCatalog instance;

	/// <summary>
	/// TODO:
	///  - Pause goes on top of this
	///  - Prevent shooting when clicking (add one frame of ignore)
	///  - Button callbacks & actual card change reflects
	/// </summary>
	///

	[System.Serializable]
	public class UpgradeIcon {
		public string upgradeName;
		public Sprite icon;
	}
	public class UpgradeNode {
		public string upgradeName;
		public string displayName;
		public string parentId;
		public Sprite internalIcon;
		public int cost;
		public int level;
		public string description;
		public bool unlocked;
		public readonly bool replacePrior;
		public InGameUpgradeInfo info;
		public UpgradeNode prior;

		//if any tech in here was bought, this cannot be bought
		private List<string> mutuallyExclusiveUpgrades;

		//all parent upgrades must be unlocked to meet hard requirements (converging branches can use this)
		private List<string> hardRequirements;

		//any parent upgrade unlocked = req. met (mutually exclusive pre-branches use this)
		private List<string> softRequirements;

		//CONSTRUCTOR
		public UpgradeNode(
			string upgradeName, string parentId, string displayName, int cost, InGameUpgradeInfo info, int level = 0,
			string description = "No description", bool unlocked = false, bool replacePrior = false,
			List<string> mutuallyExclusiveUpgrades = null,
			List<string> hardRequirements = null,
			List<string> softRequirements = null) {

			this.mutuallyExclusiveUpgrades = mutuallyExclusiveUpgrades;
			this.hardRequirements = hardRequirements;
			this.softRequirements = softRequirements;
			this.info = info;

			//create new if lists are null
			this.mutuallyExclusiveUpgrades ??= new();
			this.hardRequirements ??= new();
			this.softRequirements ??= new();

			this.description = description;
			this.upgradeName = upgradeName;
			this.displayName = displayName;
			this.parentId = parentId;
			if (instance.iconScriptObj.TryGetIcon(upgradeName, out Sprite s)) {
				internalIcon = s;
			} else {
				Debug.LogError("Icon not found for " + upgradeName);
				internalIcon = null;
			}
			this.cost = cost;
			this.unlocked = unlocked;
			this.level = level;
		}

		public string GetUpgradeId() {
			if (level == 0) return upgradeName;
			return upgradeName + " " + ToRoman(level);
		}

		public string GetIconId() {
			return upgradeName;
		}

		//returns whether soft and hard requirements have been met;
		//NOTE: call this function once per round to decide which ones to feed to player
		public bool CanUnlock(Dictionary<string, UpgradeNode> playerUpgrades) {
			//already unlocked
			if (unlocked) return true;

			foreach (string s in mutuallyExclusiveUpgrades) {
				if (playerUpgrades.TryGetValue(s, out UpgradeNode n) && n.unlocked) {
					//mutually exclusive upgrade already unlocked
					return false;
				}
			}
			foreach (string s in hardRequirements) {
				if (playerUpgrades.TryGetValue(s, out UpgradeNode n) && !n.unlocked) {
					//missing a requirement
					return false;
				}
			}
			foreach (string s in softRequirements) {
				if (playerUpgrades.TryGetValue(s, out UpgradeNode n) && n.unlocked) {
					//satisfied requirement
					return true;
				}
			}
			//no soft requirements
			if (softRequirements.Count == 0) return true;

			//failed soft requirement
			return false;
		}

		public override string ToString() {
			string returnString = "";
			returnString += GetUpgradeId();
			if (softRequirements.Count > 0) {
				returnString += " Softs: ";
				foreach (string s in softRequirements) {
					returnString += s + " ";
				}
			}
			if (hardRequirements.Count > 0) {
				returnString += " Hards: ";
				foreach (string s in hardRequirements) {
					returnString += s + " ";
				}
			}
			if (mutuallyExclusiveUpgrades.Count > 0) {
				returnString += " Mutuals: ";
				foreach (string s in mutuallyExclusiveUpgrades) {
					returnString += s + " ";
				}
			}
			return returnString;
		}
	}
	//from stackoverflow.com/questions/7040289/converting-integers-to-roman-numerals
	public static string ToRoman(int number) {
		if ((number < 0) || (number > 3999)) return "∞";
		if (number < 1) return string.Empty;
		if (number >= 50) return "L" + ToRoman(number - 50);
		if (number >= 40) return "XL" + ToRoman(number - 40);
		if (number >= 10) return "X" + ToRoman(number - 10);
		if (number >= 9) return "IX" + ToRoman(number - 9);
		if (number >= 5) return "V" + ToRoman(number - 5);
		if (number >= 4) return "IV" + ToRoman(number - 4);
		if (number >= 1) return "I" + ToRoman(number - 1);
		return "Unknown";
	}

	#endregion

	#region Prefabs

	[SerializeField] private GameObject upgradeDisplayPrefab;

	#endregion

	#region References

	[SerializeField] private RectTransform waveUpgradeUI;
	public bool UpgradeUIOn() {
		if (Time.time - closedUpgradeTimestamp < 0.07f) return true;
		return waveUpgradeUI.gameObject.activeInHierarchy;
	}
	public void DisableUpgradeUI() {
		if (waveUpgradeUI.gameObject.activeInHierarchy) {
			waveUpgradeUI.gameObject.SetActive(false);
			closedUpgradeTimestamp = Time.time;
		}
	}

	//actually shows the player upgrade stuff
	[SerializeField] private List<UpgradeCardButton> waveUpgradeCards;

	[SerializeField] private TMP_Text PCHotkeyText;

	[SerializeField] private RectTransform upgradeDisplayParent;

	[SerializeField] private GameObject buyReminderIcon;
	[SerializeField] private GameObject debugUIPrefab;

	[SerializeField] private IconScriptableObj iconScriptObj;

	#endregion

	#region Members

	//get all the icon sprites
	[SerializeField] private List<UpgradeIcon> upgradeIcons;
	private Dictionary<string, Sprite> upgradeIconsDict;

	//don't make null so it doesn't look that bad
	[SerializeField] private Sprite fallbackSprite;
	public Sprite GetFallbackSprite() { return fallbackSprite; }

	//the real list
	private Dictionary<string, UpgradeNode> playerUpgrades;

	//NOTE: player money
	private int playerMoney = 250;
	public int GetPlayerMoney() { return playerMoney; }
	public void SetPlayerMoney(int newMoney) {
		playerMoney = newMoney;
		MoneyChanged();
	}

	//whenever score changes, compare with this to see how much money player should get
	private int lastScore = 0;

	private float closedUpgradeTimestamp = 0f;

	private List<string> upgradeIds = new();
	public List<string> GetUpgradeIds() { return upgradeIds; }

	private List<GameObject> upgradeDisplays = new();

	#endregion

	#region Functions

	public void ForceMaxUpgrades() {
		foreach (UpgradeNode n in playerUpgrades.Values) {
			NetworkedEntity.playerInstance.PushUpgradeModi(n);
			n.unlocked = true;
		}
		ReRollUpgrades();
		RerenderUpgrades();
	}

	//destroys GameObjects in UI and replaces them with new ones
	private void RerenderUpgrades() {

		//clear out old
		foreach (GameObject g in upgradeDisplays) {
			if (g != null) Destroy(g);
		}
		upgradeDisplays.Clear();

		//renders them out
		SortedDictionary<string, int> highestUnlockedUpgrades = new();
		foreach (UpgradeNode n in playerUpgrades.Values) {
			if (n == null || !n.unlocked) continue;
			if (!highestUnlockedUpgrades.ContainsKey(n.parentId)) {
				highestUnlockedUpgrades.Add(n.parentId, 1);
			} else highestUnlockedUpgrades[n.parentId]++; // increment max level

		}

		//look through dict
		foreach (KeyValuePair<string, int> kv in highestUnlockedUpgrades) { // for the top bar ui
			string levelText = kv.Value == 0 ? "" : ToRoman(kv.Value);

			//Sprite sprite = GetUpgradeIcon(kv.Key);
			if (instance.iconScriptObj.TryGetIcon(kv.Key, out Sprite i)) {
				Sprite sprite = i;

				GameObject ins = Instantiate(upgradeDisplayPrefab, upgradeDisplayParent);
				upgradeDisplays.Add(ins);

				ins.GetComponent<Image>().sprite = sprite;
				ins.transform.GetChild(0).GetComponent<TMP_Text>().text = levelText;
			} else Debug.LogError("Icon not found for " + kv.Key);
		}
	}

	public void ReRollUpgrades() {
		List<string> totalUpgradableList = new();
		foreach (UpgradeNode n in playerUpgrades.Values) {
			if (n.CanUnlock(playerUpgrades) && !n.unlocked) totalUpgradableList.Add(n.GetUpgradeId());
		}

		List<string> upgradableList = new();

		//pick up to 4 random upgradable cards
		for (int i = 0; i < 4; i++) {
			if (totalUpgradableList.Count == 0) break;
			int addIndex = Random.Range(0, totalUpgradableList.Count);
			upgradableList.Add(totalUpgradableList[addIndex]);
			totalUpgradableList.RemoveAt(addIndex);
		}

		for (int i = 0; i < Mathf.Min(upgradableList.Count, waveUpgradeCards.Count); i++) {
			playerUpgrades.TryGetValue(upgradableList[i], out UpgradeNode n);
			if (n == null) {
				Debug.LogWarning("Upgrade not found");
				continue;
			}
			waveUpgradeCards[i].gameObject.SetActive(true);

			//setup button callbacks
			waveUpgradeCards[i].Init(n);
			waveUpgradeCards[i].UpdateCost(playerMoney);
		}
		//turn off unused
		for (int i = upgradableList.Count; i < waveUpgradeCards.Count; i++) {
			waveUpgradeCards[i].Init(null);
			waveUpgradeCards[i].gameObject.SetActive(false);
		}
	}

	//NOTE: called at the end of the wave to return up to 4 random upgrades that can be bought
	public void ShowPossibleUpgrades() {
		waveUpgradeUI.gameObject.SetActive(true);

		//if single player will pause
		UIController.instance.PauseGame();
	}
	public void CloseUpgrades() {
		DisableUpgradeUI();

		//if paused, resumes game
		UIController.instance.ResumeGame();
	}
	public void ScoreChanged(int newScore) {
		if (newScore > lastScore) {
			playerMoney += newScore - lastScore;
			MoneyChanged();
		}
		lastScore = newScore;
	}
	public void MoneyChanged() {
		UIController.instance.SetMoneyText(playerMoney);

		foreach (UpgradeCardButton c in waveUpgradeCards) {
			c.UpdateCost(playerMoney);
		}
	}
	public void PurchaseUpgrade(UpgradeCardButton sender, string upgradeName) {
		print($"tried to purchase {upgradeName}");

		//can't upgrade for one reason or another
		if (!playerUpgrades.ContainsKey(upgradeName) || playerUpgrades[upgradeName].unlocked ||
			playerUpgrades[upgradeName].cost > playerMoney) return;

		//charge player
		playerMoney -= playerUpgrades[upgradeName].cost;
		playerUpgrades[upgradeName].unlocked = true;

		//call PlayerInfo callback
		NetworkedEntity.playerInstance.PushUpgradeModi(sender.GetNode());

		sender.PurchaseSuccessful();
		MoneyChanged();

		ReRollUpgrades();
		RerenderUpgrades();
	}
	public UpgradeNode AddUpgrade(string id, string parentId, string displayName, int cost, int level = 0, InGameUpgradeInfo info = null, bool unlocked = false,
		bool replacePrior = false, List<string> mutuallyExclusiveUpgrades = null,
		List<string> hardRequirements = null, List<string> softRequirements = null) {

		UpgradeNode n = new(id, parentId, displayName, cost, info, level, info?.GetDescription(), unlocked, replacePrior,
			mutuallyExclusiveUpgrades, hardRequirements, softRequirements);

		//only add _level if level != 0
		playerUpgrades.Add(n.GetUpgradeId(), n);
		return n;
	}
	public void InitUpgrades() {
		foreach (UpgradeNode n in playerUpgrades.Values) {
			if (n.unlocked) {
				NetworkedEntity.playerInstance.PushUpgradeModi(n);
			}
			//init upgrade
			upgradeIconsDict = new();

			if (instance.iconScriptObj.TryGetIcon(n.GetIconId(), out Sprite i)) {

				if (upgradeIconsDict.ContainsKey(n.GetIconId())) {
					upgradeIconsDict[n.GetIconId()] = i;
				} else {
					upgradeIconsDict.Add(n.GetIconId(), i);
				}
			}
		}

		//foreach (UpgradeNode u in playerUpgrades.Values) {
		//	Debug.LogWarning("UpgradeInit : " + u.ToString());
		//}
		if (UIController.GetIsMobile()) PCHotkeyText.gameObject.SetActive(false);

		playerMoney = PlayerPrefs.GetInt("game_start_cash");

		ReRollUpgrades();
		RerenderUpgrades();

	}
	private void Awake() {
		instance = this;

		playerUpgrades = new();
		upgradeIconsDict = new();
	}

	private void Update() {
		bool canAfford = false;
		foreach (UpgradeCardButton b in waveUpgradeCards) {
			if (b.GetNode() == null) continue;
			if (b.GetNode().cost <= playerMoney) {
				canAfford = true;
				break;
			}
		}

		if (buyReminderIcon.activeInHierarchy != canAfford)
			buyReminderIcon.SetActive(canAfford);

		if (Input.GetKeyDown(KeyCode.U)) {
			if (debugUIPrefab != null && !debugUIPrefab.activeInHierarchy)
				ShowPossibleUpgrades();
		}
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.P)) {
			playerMoney += 1000;
			MoneyChanged();
		}
#endif
	}

	#endregion
}
