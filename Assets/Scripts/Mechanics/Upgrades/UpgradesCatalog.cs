using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CSVParser;
using CSVParser.Init;

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
		public Sprite icon;
		public int cost;
		public int level;
		public bool unlocked;
		public readonly bool replacePrior;
		public UpgradeInfo info;
		public UpgradeNode prior;

		//if any tech in here was bought, this cannot be bought
		private List<string> mutuallyExclusiveUpgrades;

		//all parent upgrades must be unlocked to meet hard requirements (converging branches can use this)
		private List<string> hardRequirements;

		//any parent upgrade unlocked = req. met (mutually exclusive pre-branches use this)
		private List<string> softRequirements;

		//CONSTRUCTOR
		public UpgradeNode(
			string upgradeName, Sprite icon, int cost,  UpgradeInfo info, int level = 0, bool unlocked = false, bool replacePrior = false,
			List<string> mutuallyExclusiveUpgrades = null,
			List<string> hardRequirements = null,
			List<string> softRequirements = null) {

			this.mutuallyExclusiveUpgrades = mutuallyExclusiveUpgrades;
			this.hardRequirements = hardRequirements;
			this.softRequirements = softRequirements;

			//create new if lists are null
			this.mutuallyExclusiveUpgrades ??= new();
			this.hardRequirements ??= new();
			this.softRequirements ??= new();

			this.upgradeName = upgradeName;
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
				foreach(string s in softRequirements) {
					returnString += s + " ";
				}
			}
			if (hardRequirements.Count > 0) {
				returnString += " Hards: ";
				foreach(string s in hardRequirements) {
					returnString += s + " ";
				}
			}
			if (mutuallyExclusiveUpgrades.Count > 0) {
				returnString += " Mutuals: ";
				foreach(string s in mutuallyExclusiveUpgrades) {
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
		//NOTE: don't need large numbers, save computation time this way
		//if (number >= 1000) return "M" + ToRoman(number - 1000);
		//if (number >= 900) return "CM" + ToRoman(number - 900);
		//if (number >= 500) return "D" + ToRoman(number - 500);
		//if (number >= 400) return "CD" + ToRoman(number - 400);
		//if (number >= 100) return "C" + ToRoman(number - 100);
		//if (number >= 90) return "XC" + ToRoman(number - 90);
		//if (number >= 50) return "L" + ToRoman(number - 50);
		//if (number >= 40) return "XL" + ToRoman(number - 40);
		if (number >= 10) return "X" + ToRoman(number - 10);
		if (number >= 9) return "IX" + ToRoman(number - 9);
		if (number >= 5) return "V" + ToRoman(number - 5);
		if (number >= 4) return "IV" + ToRoman(number - 4);
		if (number >= 1) return "I" + ToRoman(number - 1);
		return "Unknown";
	}

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

	[SerializeField] private TMP_Text timerText;
	public void SetTimerText(string text) { timerText.text = text; }

	#endregion

	#region Members

	//get all the icon sprites
	[SerializeField] private List<UpgradeIcon> upgradeIcons;
	private Dictionary<string, Sprite> upgradeIconsDict;

	//the real list
	private Dictionary<string, UpgradeNode> playerUpgrades;

	//NOTE: player money
	private int playerMoney = 0;
	public int GetPlayerMoney() { return playerMoney; }

	//whenever score changes, compare with this to see how much money player should get
	private int lastScore = 0;

	private float closedUpgradeTimestamp = 0f;

	private List<string> upgradeIds = new();
	public List<string> GetUpgradeIds() { return upgradeIds; }

	#endregion

	#region Functions

	//NOTE: called at the end of the wave to return up to 4 random upgrades that can be bought
	public void ShowPossibleUpgrades() {
		//TODO: call this function at the end of the wave, etc
		//TODO: have a timer show seconds until next wave arrives

		waveUpgradeUI.gameObject.SetActive(true);

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
		}
		//turn off unused
		for (int i = upgradableList.Count; i < waveUpgradeCards.Count; i++) {
			waveUpgradeCards[i].gameObject.SetActive(false);
		}
	}
	public void CloseUpgrades() {
		DisableUpgradeUI();
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
		PlayerInfo.instance.PushUpgradeModi(sender.GetNode());

		sender.PurchaseSuccessful();
		MoneyChanged();
	}
	public UpgradeNode AddUpgrade(string name, int cost, UpgradeInfo info, int level = 0, bool unlocked = false, 
		bool replacePrior = false, List<string> mutuallyExclusiveUpgrades = null,
		List<string> hardRequirements = null, List<string> softRequirements = null) {

		UpgradeNode n = new(name, GetUpgradeIcon(name), cost, info, level, unlocked, replacePrior,
			mutuallyExclusiveUpgrades, hardRequirements, softRequirements);

		//only add _level if level != 0
		playerUpgrades.Add(n.GetUpgradeId(), n);

		return n;
	}
	private Sprite GetUpgradeIcon(string name) {
		if (upgradeIconsDict.ContainsKey(name)) return upgradeIconsDict[name];
		return null;
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

		CSVInit();
	}

	/*=================| CSV |=================*/
	private void CSVInit() { // TODO: Update with turret info, etc.
		this.ParseUpgradesFromAllCSV();
	}

	private void Start() {
		//make sure player starts with these abilities unlocked
		foreach (UpgradeNode n in playerUpgrades.Values) {
			if (n.unlocked) {
				PlayerInfo.instance.PushUpgradeModi(n);
			}
		}
		foreach (UpgradeNode u in playerUpgrades.Values) {
			Debug.LogWarning("UpgradeInit : " + u.ToString());
		}
		
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.U)) {
			playerMoney += 1000;
			MoneyChanged();
			ShowPossibleUpgrades();
		}
	}

	#endregion
}
