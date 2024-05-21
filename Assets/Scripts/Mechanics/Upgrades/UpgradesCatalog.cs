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

	[SerializeField] public CSVStorage csvStorage;

	[System.Serializable]
	public class UpgradeIcon {
		public string upgradeName;
		public Sprite icon;
	}
	public class UpgradeNode {
		public string upgradeName;
		public string displayName;
		public Sprite icon;
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
			string upgradeName, string displayName, Sprite icon, int cost, InGameUpgradeInfo info, int level = 0,
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
			this.icon = icon;
			this.cost = cost;
			this.unlocked = unlocked;
			this.level = level;
		}

		public string GetUpgradeId() {
			if (level == 0) return upgradeName;
			return upgradeName + " " + ToRoman(level);
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

	#endregion

	#region Members

	//get all the icon sprites
	[SerializeField] private List<UpgradeIcon> upgradeIcons;
	private Dictionary<string, Sprite> upgradeIconsDict;

	//don't make null so it doesn't look that bad
	[SerializeField] private Sprite fallbackSprite;

	//the real list
	private Dictionary<string, UpgradeNode> playerUpgrades;

	//NOTE: player money
	private int playerMoney = 250;
	public int GetPlayerMoney() { return playerMoney; }

	//whenever score changes, compare with this to see how much money player should get
	private int lastScore = 0;

	private float closedUpgradeTimestamp = 0f;

	private List<string> upgradeIds = new();
	public List<string> GetUpgradeIds() { return upgradeIds; }

	private List<GameObject> upgradeDisplays = new();

	#endregion

	#region Functions

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
			if (!highestUnlockedUpgrades.ContainsKey(n.upgradeName)) {
				highestUnlockedUpgrades.Add(n.upgradeName, n.level);
			} else if (highestUnlockedUpgrades[n.upgradeName] < n.level) {
				//put highest level in dict
				highestUnlockedUpgrades[n.upgradeName] = n.level;
			}
		}
		//look through dict
		foreach (KeyValuePair<string, int> kv in highestUnlockedUpgrades) {
			string levelText = kv.Value == 0 ? "" : ToRoman(kv.Value);
			Sprite sprite = GetUpgradeIcon(kv.Key);

			GameObject ins = Instantiate(upgradeDisplayPrefab, upgradeDisplayParent);
			upgradeDisplays.Add(ins);

			ins.GetComponent<Image>().sprite = sprite;
			ins.transform.GetChild(0).GetComponent<TMP_Text>().text = levelText;
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
	public UpgradeNode AddUpgrade(string id, string displayName, int cost, int level = 0, InGameUpgradeInfo info = null, bool unlocked = false,
		bool replacePrior = false, List<string> mutuallyExclusiveUpgrades = null,
		List<string> hardRequirements = null, List<string> softRequirements = null) {

		UpgradeNode n = new(id, displayName, GetUpgradeIcon(name), cost, info, level, info?.GetDescription(), unlocked, replacePrior,
			mutuallyExclusiveUpgrades, hardRequirements, softRequirements);

		//only add _level if level != 0
		playerUpgrades.Add(n.GetUpgradeId(), n);
		return n;
	}
	private Sprite GetUpgradeIcon(string name) {
		if (upgradeIconsDict.ContainsKey(name)) return upgradeIconsDict[name];
		Debug.LogWarning($"need sprite for `{name}`!");
		return fallbackSprite;
	}
	public void InitUpgrades() {
		foreach (UpgradeNode n in playerUpgrades.Values) {
			if (n.unlocked) {
				NetworkedEntity.playerInstance.PushUpgradeModi(n);
			}
		}
		ReRollUpgrades();
		RerenderUpgrades();
	}
	private void Awake() {
		instance = this;

		upgradeIconsDict = new();
		foreach (UpgradeIcon u in upgradeIcons) {
			upgradeIconsDict.Add(u.upgradeName, u.icon);
		}

		//NOTE: CREATE ALL ABILITY TREES HERE;
		//TODO: CREATE STATIC FUNCTIONS THAT CREATES A NEW DICT BASED ON CLASS
		//  SO THAT THE GARAGE CAN ACCESS TREE INFO
		playerUpgrades = new();

		//starting upgrades
		// UpgradeNode rapid1 = AddUpgrade("Rapid Fire", cost: 0, level: 1, unlocked: true);
		// UpgradeNode heal1 = AddUpgrade("Heal", cost: 0, level: 1, unlocked: true);

		// UpgradeNode rapid2 = AddUpgrade("Rapid Fire", cost: 10, level: 2, hardRequirements: new() { rapid1.GetUpgradeId() });
		// UpgradeNode heal2 = AddUpgrade("Heal", cost: 10, level: 2, hardRequirements: new() { heal1.GetUpgradeId() });

		// UpgradeNode rapid3 = AddUpgrade("Rapid Fire", cost: 10, level: 3, hardRequirements: new() { rapid2.GetUpgradeId() });
		// UpgradeNode heal3 = AddUpgrade("Heal", cost: 10, level: 3, hardRequirements: new() { heal2.GetUpgradeId() });

		// for (int i = 0; i < 10; i++) AddUpgrade($"Camp {i + 1}", 10 + i);


	}

	private void Start() {

		
		//foreach (UpgradeNode u in playerUpgrades.Values) {
		//	Debug.LogWarning("UpgradeInit : " + u.ToString());
		//}
		if (UIController.GetIsMobile()) PCHotkeyText.gameObject.SetActive(false);

		playerMoney = PlayerPrefs.GetInt("game_start_cash");
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
