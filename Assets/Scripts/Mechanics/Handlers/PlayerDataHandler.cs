using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSV.Parsers;
using Newtonsoft.Json;
using TMPro;
using System.Linq;

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

    #endregion

    #region References

    public static PlayerDataHandler instance;

    [Header("Raw CSVs")]
    [SerializeField] public TextAsset activeRawCSV;
    [SerializeField] public TextAsset gadgetRawCSV, hullRawCSV, turretRawCSV;

    #endregion

    #region Members

    //*==========================================| INFOs |==========================================*//

    private Dictionary<string, InventoryInfo> activesInfo = new();
    private Dictionary<string, InventoryInfo> gadgetsInfo = new();
    private Dictionary<string, InventoryInfo> hullsInfo = new();
    private Dictionary<string, InventoryInfo> turretsInfo = new();
    private Dictionary<string, Dictionary<string, int>> equippedInfos = new();

    #endregion

    #region Getters & Setters

    public string GetActiveRawCSV() { return activeRawCSV.text; }
    public string GetGadgetRawCSV() { return gadgetRawCSV.text; }
    public string GetHullRawCSV() { return hullRawCSV.text; }
    public string GetTurretRawCSV() { return turretRawCSV.text; }
    public Dictionary<string, InventoryInfo> GetActiveInfos() { return activesInfo; }
    public Dictionary<string, InventoryInfo> GetGadgetInfos() { return gadgetsInfo; }
    public Dictionary<string, InventoryInfo> GetHullInfos() { return hullsInfo; }
    public Dictionary<string, InventoryInfo> GetTurretInfos() { return turretsInfo; }
    public List<string> GetActiveInfoKeys() { return activesInfo.Keys.ToList(); }
    public List<string> GetGadgetInfoKeys() { return gadgetsInfo.Keys.ToList(); }
    public List<string> GetHullInfoKeys() { return hullsInfo.Keys.ToList(); }
    public List<string> GetTurretInfoKeys() { return turretsInfo.Keys.ToList(); }

    /// <summary>
    /// II = Inventory Info. Used for DOUBLE value types.
    /// </summary>
    public bool TryGetIIModifierValue(CSVId itemKey, CSVMd modiKey, int level, out double modiVal) {
        if (TryGetActiveModi(itemKey.ToString(), modiKey.ToString(), level, out modiVal)) { return true; }
        if (TryGetGadgetModi(itemKey.ToString(), modiKey.ToString(), level, out modiVal)) { return true; }
        if (TryGetHullModi(itemKey.ToString(), modiKey.ToString(), level, out modiVal)) { return true; }
        if (TryGetTurretModi(itemKey.ToString(), modiKey.ToString(), level, out modiVal)) { return true; }
        return false;
    }
    public bool TryGetIIModifierValue(string itemKey, string modiKey, int level, out double modiVal) {
        if (TryGetActiveModi(itemKey, modiKey, level, out modiVal)) { return true; }
        if (TryGetGadgetModi(itemKey, modiKey, level, out modiVal)) { return true; }
        if (TryGetHullModi(itemKey, modiKey, level, out modiVal)) { return true; }
        if (TryGetTurretModi(itemKey, modiKey, level, out modiVal)) { return true; }
        return false;
    }

    /// <summary>
    /// II = Inventory Info. Used for STRING value types.
    /// </summary>
    public bool TryGetIIModifierValue(CSVId itemKey, CSVMd modiKey, out string modiString) {
        if (TryGetItemFromInfos(itemKey, out InventoryInfo info)) {
            modiString = info.GetStringModi(modiKey);
            return true;
        }
        modiString = "";
        return false;
    }
    public bool TryGetIIModifierValue(string itemKey, string modiKey, out string modiString) {
        if (TryGetItemFromInfos(itemKey, out InventoryInfo info)) {
            modiString = info.GetStringModi(modiKey);
            return true;
        }
        modiString = "";
        return false;
    }

    /// <summary>
    /// II = Inventory Info. Get Max Level.
    /// </summary>
    public bool TryGetIIMaxLevel(CSVId itemKey, out int maxLevel) {
        if (TryGetItemFromInfos(itemKey, out InventoryInfo info)) {
            maxLevel = info.GetMaxLevel();
            return true;
        }
        maxLevel = 0;
        return false;
    }
    public bool TryGetIIMaxLevel(string itemKey, out int maxLevel) {
        if (TryGetItemFromInfos(itemKey, out InventoryInfo info)) {
            maxLevel = info.GetMaxLevel();
            return true;
        }
        maxLevel = 0;
        return false;
    }

    public Dictionary<string, Dictionary<string, int>> GetEquippedInfos() {
        return equippedInfos;
    }

    #endregion

    #region Methods

    public void EquipInfo(string type, string id, int level) {
        if (equippedInfos.ContainsKey(type)) {
            if (equippedInfos[type].ContainsKey(id)) {
                equippedInfos[type][id] = level;
            } else {
                equippedInfos[type].Add(id, level);
            }
        } else {
            equippedInfos.Add(type, new Dictionary<string, int>() { { id, level } });
        }
    }
    public void EquipInfo(CSVType type, CSVId id, int level) {
        EquipInfo(type.ToString(), id.ToString(), level);
    }

    public void ClearEquipInfos() {
        equippedInfos.Clear();
    }

    private void TempEquipInfos() { 
        //EquipInfo(CSVType.ACTIVES, CSVId.HealActive, 1);
        EquipInfo(CSVType.ACTIVES, CSVId.SentryActive, 11);
        //EquipInfo(CSVType.ACTIVES, CSVId.SentryActive, 10);
        //EquipInfo(CSVType.ACTIVES, CSVId.RapidFireActive, 10);
        EquipInfo(CSVType.ACTIVES, CSVId.HealActive, 1);
        EquipInfo(CSVType.GADGETS, CSVId.HardenedAmmoGadget, 1);
        EquipInfo(CSVType.GADGETS, CSVId.RegenerativeArmorGadget, 1);

    }

    private bool TryGetItemFromInfos(CSVId itemKey, out InventoryInfo info) {
        if (activesInfo.TryGetValue(itemKey.ToString(), out info)) { return true; }
        if (gadgetsInfo.TryGetValue(itemKey.ToString(), out info)) { return true; }
        if (hullsInfo.TryGetValue(itemKey.ToString(), out info)) { return true; }
        if (turretsInfo.TryGetValue(itemKey.ToString(), out info)) { return true; }
        return false;
    }
    private bool TryGetItemFromInfos(string itemKey, out InventoryInfo info) {
        if (activesInfo.TryGetValue(itemKey, out info)) { return true; }
        if (gadgetsInfo.TryGetValue(itemKey, out info)) { return true; }
        if (hullsInfo.TryGetValue(itemKey, out info)) { return true; }
        if (turretsInfo.TryGetValue(itemKey, out info)) { return true; }
        return false;
    }

    private bool TryGetActiveModi(string itemKey, string modiKey, int level, out double modiVal) {
        if (activesInfo.TryGetValue(itemKey, out InventoryInfo info)) 
            if (info.TryGetModi(modiKey, level, out modiVal)) { return true; }
        modiVal = 0;
        return false;
    }

    private bool TryGetGadgetModi(string itemKey, string modiKey, int level, out double modiVal) {
        if (gadgetsInfo.TryGetValue(itemKey, out InventoryInfo info)) 
            if (info.TryGetModi(modiKey, level, out modiVal)) { return true; }
        modiVal = 0;
        return false;
    }

    private bool TryGetHullModi(string itemKey, string modiKey, int level, out double modiVal) {
        if (hullsInfo.TryGetValue(itemKey, out InventoryInfo info)) 
            if (info.TryGetModi(modiKey, level, out modiVal)) { return true; }
        modiVal = 0;
        return false;
    }

    private bool TryGetTurretModi(string itemKey, string modiKey, int level, out double modiVal) {
        if (turretsInfo.TryGetValue(itemKey, out InventoryInfo info)) 
            if (info.TryGetModi(modiKey, level, out modiVal)) { return true; }
        modiVal = 0;
        return false;
    }

    public bool ForceResetInfos(bool isDebug = false) {
        float startTime = Time.realtimeSinceStartup;
        bool returnBool = activesInfo.TryParse(activeRawCSV.text, isDebug, "Abilities") &&
               gadgetsInfo.TryParse(gadgetRawCSV.text, isDebug, "Gadgets") &&
               hullsInfo.TryParse(hullRawCSV.text, isDebug, "Hulls") &&
               turretsInfo.TryParse(turretRawCSV.text, isDebug, "Turrets");
        if (isDebug) {
            DebugUIManager.instance?.LogOutput("\n Total time Taken : " + ((Time.realtimeSinceStartup - startTime)*1000f).ToString() + "ms\n");
        }
        return returnBool;
    }

    #endregion

    #region UnityCallBacks

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        ForceResetInfos();
        TempEquipInfos(); // TODO: Remove this.
    }

    private void Update() {
    }

    #endregion
}

