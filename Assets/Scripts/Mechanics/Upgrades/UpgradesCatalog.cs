using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradesCatalog : MonoBehaviour {

	#region Statics & Consts & Member Classes

	public static UpgradesCatalog instance;

	[System.Serializable]
	public class UpgradeIcon {
		public string upgradeName;
		public Sprite icon;
	}
	public class UpgradeNode {
		public string upgradeName;
		public Sprite icon;
		public int cost;
		public bool unlocked;

		//if any tech in here was bought, this cannot be bought
		private List<string> mutuallyExclusiveUpgrades;

		//all parent upgrades must be unlocked to meet hard requirements (converging branches can use this)
		private List<string> hardRequirements;

		//any parent upgrade unlocked = req. met (mutually exclusive pre-branches use this)
		private List<string> softRequirements;

		//CONSTRUCTOR
		public UpgradeNode(
			string upgradeName, Sprite icon, int cost, bool unlocked = false,
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
			//failed soft requirement
			return false;
		}
	}

	#endregion

	#region References

	//actually shows the player upgrade stuff
	[SerializeField] private RectTransform waveUpgradeUI;
	[SerializeField] private List<RectTransform> waveUpgradeCards;
	[SerializeField] private List<Image> waveUpgradeImages;
	[SerializeField] private List<TMP_Text> waveUpgradeNameTexts, waveUpgradeCostTexts;

	#endregion

	#region Members

	//get all the icon sprites
	[SerializeField] private List<UpgradeIcon> upgradeIcons;
	private Dictionary<string, Sprite> upgradeIconsDict;

	//the real list
	private Dictionary<string, UpgradeNode> playerUpgrades;

	//player money
	private int playerMoney = 0;
	public int GetPlayerMoney() { return playerMoney; }

	//whenever score changes, compare with this to see how much money player should get
	private int lastScore = 0;

	#endregion

	#region Functions

	//NOTE: called at the end of the wave to return up to 4 random upgrades that can be bought
	public void ShowPossibleUpgrades() {
		//TODO: call this function at the end of the wave, etc
		//TODO: have a timer show seconds until next wave arrives

		List<string> totalUpgradableList = new();
		foreach (UpgradeNode n in playerUpgrades.Values) {
			if (n.CanUnlock(playerUpgrades)) totalUpgradableList.Add(n.upgradeName);
		}

		List<string> upgradableList = new();

		//pick up to 4 random upgradable cards
		for (int i = 0; i < Mathf.Min(totalUpgradableList.Count, 4); i++) {
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
			waveUpgradeCostTexts[i].text = n.cost.ToString();
			waveUpgradeImages[i].sprite = n.icon;
			waveUpgradeNameTexts[i].text = n.upgradeName;
		}
		//turn off unused
		for (int i = upgradableList.Count; i < waveUpgradeCards.Count; i++) {
			waveUpgradeCards[i].gameObject.SetActive(false);
		}
	}
	public void ScoreChanged(int newScore) {
		if (newScore > lastScore) {
			playerMoney += newScore - lastScore;
		}
		lastScore = newScore;
	}
	public void PurchaseUpgrade(string upgradeName) {
		//can't upgrade for one reason or another
		if (!playerUpgrades.ContainsKey(upgradeName) || playerUpgrades[upgradeName].unlocked ||
			playerUpgrades[upgradeName].cost > playerMoney) return;

		//charge player
		playerMoney -= playerUpgrades[upgradeName].cost;
		playerUpgrades[upgradeName].unlocked = true;
	}
	private void AddUpgrade(string name, int cost, bool unlocked = false, List<string> mutuallyExclusiveUpgrades = null,
		List<string> hardRequirements = null, List<string> softRequirements = null) {

		UpgradeNode n = new(name, GetUpgradeIcon(name), cost, unlocked,
			mutuallyExclusiveUpgrades, hardRequirements, softRequirements);

		playerUpgrades.Add(n.upgradeName, n);
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
		AddUpgrade("Rapid Fire", 0, unlocked: true);
		AddUpgrade("Heal", 0, unlocked: true);
	}

	private void Start() { }

	private void Update() { }

	#endregion
}
