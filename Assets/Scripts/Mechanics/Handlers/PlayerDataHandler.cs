using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSV.Parsers;
using Newtonsoft.Json;
using TMPro;

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
    private Dictionary<CSVId, List<string>> equippedInfos = new();

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
    public List<string> GetActiveInfoKeys() {
        List<string> keys = new();
        foreach (var item in activesInfo) { keys.Add(item.Key); }
        return keys;
    }
    public List<string> GetGadgetInfoKeys() {
        List<string> keys = new();
        foreach (var item in gadgetsInfo) { keys.Add(item.Key); }
        return keys;
    }
    public List<string> GetHullInfoKeys() {
        List<string> keys = new();
        foreach (var item in hullsInfo) { keys.Add(item.Key); }
        return keys;
    }
    public List<string> GetTurretInfoKeys() {
        List<string> keys = new();
        foreach (var item in turretsInfo) { keys.Add(item.Key); }
        return keys;
    }

    /// <summary>
    /// II = Inventory Info. Used for DOUBLE value types.
    /// </summary>
    public bool TryGetIIModifierValue(CSVId itemKey, CSVMd modiKey, int level, out double modiVal) {
        if (TryGetActiveModi(nameof(itemKey), nameof(modiKey), level, out modiVal)) { return true; }
        if (TryGetGadgetModi(nameof(itemKey), nameof(modiKey), level, out modiVal)) { return true; }
        if (TryGetHullModi(nameof(itemKey), nameof(modiKey), level, out modiVal)) { return true; }
        if (TryGetTurretModi(nameof(itemKey), nameof(modiKey), level, out modiVal)) { return true; }
        return false;
    }

    /// <summary>
    /// II = Inventory Info. Used for STRING value types.
    /// </summary>
    // public bool TryGetIIModifierValue(CSVId itemKey, CSVMd modiKey, out string modiString) {
    //     if (TryGetItemFromInfos(itemKey, out InventoryInfo info)) {
    //         if (info.TryGetModi(modiKey, out modiString)) { return true; }
    //     }
    // }

    #endregion

    #region Methods

    private bool TryGetItemFromInfos(CSVId itemKey, out InventoryInfo info) {
        if (activesInfo.TryGetValue(nameof(itemKey), out info)) { return true; }
        if (gadgetsInfo.TryGetValue(nameof(itemKey), out info)) { return true; }
        if (hullsInfo.TryGetValue(nameof(itemKey), out info)) { return true; }
        if (turretsInfo.TryGetValue(nameof(itemKey), out info)) { return true; }
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

    private void Update() {
    }

    #endregion
}

