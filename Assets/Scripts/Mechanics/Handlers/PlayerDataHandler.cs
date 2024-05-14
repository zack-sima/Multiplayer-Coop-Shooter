using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSV.Parsers;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class PlayerDataHandler : MonoBehaviour {
    //TODO: Where the basics stuff like loadouts, number of loadout slots, player level, player xp, player money, player gems, player cash, player name, player id, player email, player password, player last login, player last logout, player last played, player last played level, player last played mode, player last played difficulty, player last played time, player last played score, player last played kills, player last played deaths, player last played assists, player last played damage dealt, player last played damage taken, player last played healing done, player last played healing taken, player last played damage blocked, player last played damage absorbed, player last played damage reflected, player last played damage dodged, player last played damage crit, player last played damage crit taken, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last
    //Copilot wrote the above bruh ^
    //Store this on the persistent dict 
    //Store all upgradeinfos and the equivalent on this.

    #region Nested

    private enum PersistentDictKeys {
        ActivesRawCSV, GadgetsRawCSV, HullsRawCSV, TurretsRawCSV
    }

    [System.Serializable]
    public class InventoryInfo {
        public readonly string id;
        public readonly string description;
        public readonly int maxLevel;
        private Dictionary<(string, int), float> modiByLevel = new();
        private Dictionary<string, InGameUpgradeInfo> inGameUpgrades = new();
        private int currentLevel = 0; // 0 == locked.

        public InventoryInfo(string id, string description, int maxLevel) {
            this.id = id;
            this.description = description;
            this.maxLevel = maxLevel;
        }

        public bool CanUpgrade() { return currentLevel < maxLevel; }

        public void PushInventoryModi(string id, float input, int level) {
            if (modiByLevel.ContainsKey((id, level))) {
                modiByLevel[(id, level)] = input;
            } else {
                modiByLevel.Add((id, level), input);
            }
        }

        public void PushInGameUpgrade(InGameUpgradeInfo input) {
            if (!inGameUpgrades.ContainsKey(input.id)) inGameUpgrades.Add(input.id, input);
            else inGameUpgrades[input.id] = input;
        }

        public bool TryUpgrading() {
            if (CanUpgrade()) {
                currentLevel++;
                return true;
            }
            return false;
        }

        public bool TryGetModi(string id, int level, out float output) {
            output = 0;
            if (modiByLevel.ContainsKey((id, level))) {
                output = modiByLevel[(id, level)];
                return true;
            }
            return false;
        }
    }

    [System.Serializable]
    public class InGameUpgradeInfo {
        public readonly string id;
        public List<string> softRequirements = new(), hardRequirements = new(), mutualRequirements = new();
        public Dictionary<(string, int), float> modi = new();
        public int maxLevel = 1;
        public InGameUpgradeInfo(string id) { this.id = id;} 
        
    }

    #endregion

    #region References

    public static PlayerDataHandler instance;

    [Header("Raw CSVs")]
    [SerializeField] public TextAsset activeRawCSV;
    [SerializeField] public TextAsset gadgetRawCSV, hullRawCSV, turretRawCSV;

    [Header("Debug UI")]
    [SerializeField] public GameObject debugScreen;
    [SerializeField] public TextMeshProUGUI debugConsoleText;
    [SerializeField] public TextMeshProUGUI debugInputText;
    [SerializeField] public GameObject debugInputField;


    #endregion

    #region Members

    //*==========================================| INFOs |==========================================*//

    private Dictionary<string, InventoryInfo> activesInfo = new();
    private Dictionary<string, InventoryInfo> gadgetsInfo = new();
    private Dictionary<string, InventoryInfo> hullsInfo = new();
    private Dictionary<string, InventoryInfo> turretsInfo = new();

    #endregion

    #region Methods

    #region //*==| INFO HANDLERS |==*//

    public void ForceResetInfos(bool debug = false) {
        activesInfo.TryParseActive(activeRawCSV.text, debug);
        //TODO: Populate with other inits.
    }

    #endregion


    #endregion

    #region UnityCallBacks

    private void Awake() {
        instance = this;
    }

    private void Update() {
        
    }

    #endregion
}

