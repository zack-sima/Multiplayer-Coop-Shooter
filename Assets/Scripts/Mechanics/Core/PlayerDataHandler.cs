using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSV.Parsers;
using CSV;
using System.Linq;
using Abilities;

[System.Serializable]
public class PlayerDataHandler : MonoBehaviour {

    #region Nested

    private enum PersistentDictKeys {
        ActivesRawCSV, GadgetsRawCSV, HullsRawCSV, TurretsRawCSV
    }

    #endregion

    #region References

    public static PlayerDataHandler instance;

    [Header("Raw CSVs")]
    [SerializeField] public TextAsset activeRawCSV;
    [SerializeField] public TextAsset gadgetRawCSV, hullRawCSV, turretRawCSV, superRawCSV;

    #endregion

    #region Members

    //*==========================================| INFOs |==========================================*//

    private Dictionary<string, InventoryInfo> activesInfo = new();
    private Dictionary<string, InventoryInfo> gadgetsInfo = new();
    private Dictionary<string, InventoryInfo> hullsInfo = new();
    private Dictionary<string, InventoryInfo> turretsInfo = new();
    private Dictionary<string, InventoryInfo> superInfos = new();
    private Dictionary<string, Dictionary<string, int>> equippedInfos = new();

    #endregion

    #region Getters & Setters

    public Dictionary<string, int> GetEquippedInfos(string type) {
        if (equippedInfos.TryGetValue(type, out Dictionary<string, int> dict)) {
            return dict;
        }
        return new();
    }
    public Dictionary<string, int> GetEquippedInfos(CSVType type) {
        return GetEquippedInfos(type.ToString());
    }
    public string GetActiveRawCSV() { return activeRawCSV.text; }
    public string GetGadgetRawCSV() { return gadgetRawCSV.text; }
    public string GetHullRawCSV() { return hullRawCSV.text; }
    public string GetTurretRawCSV() { return turretRawCSV.text; }
    public string GetSuperRawCSV() { return superRawCSV.text; }  
    public Dictionary<string, InventoryInfo> GetActiveInfos() { return activesInfo; }
    public Dictionary<string, InventoryInfo> GetGadgetInfos() { return gadgetsInfo; }
    public Dictionary<string, InventoryInfo> GetHullInfos() { return hullsInfo; }
    public Dictionary<string, InventoryInfo> GetTurretInfos() { return turretsInfo; }
    public Dictionary<string, InventoryInfo> GetSuperInfos() { return superInfos; }
    public List<string> GetActiveInfoKeys() { return activesInfo.Keys.ToList(); }
    public List<string> GetGadgetInfoKeys() { return gadgetsInfo.Keys.ToList(); }
    public List<string> GetHullInfoKeys() { return hullsInfo.Keys.ToList(); }
    public List<string> GetTurretInfoKeys() { return turretsInfo.Keys.ToList(); }
    public List<string> GetSuperInfoKeys() { return superInfos.Keys.ToList(); }
    /// <summary>
    /// II = Inventory Info. Used for DOUBLE value types.
    /// </summary>
    public bool TryGetIIModifierValue(CSVId itemKey, CSVMd modiKey, int level, out double modiVal) {
        return TryGetIIModifierValue(itemKey.ToString(), modiKey.ToString(), level, out modiVal);
    }
    public bool TryGetIIModifierValue(string itemKey, string modiKey, int level, out double modiVal) {
        if (TryGetActiveModi(itemKey, modiKey, level, out modiVal)) { return true; }
        if (TryGetGadgetModi(itemKey, modiKey, level, out modiVal)) { return true; }
        if (TryGetHullModi(itemKey, modiKey, level, out modiVal)) { return true; }
        if (TryGetTurretModi(itemKey, modiKey, level, out modiVal)) { return true; }
        if (TryGetSuperModi(itemKey, modiKey, level, out modiVal)) { return true; }
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
    private bool TryGetItemFromInfos(CSVId itemKey, out InventoryInfo info) {
        return TryGetItemFromInfos(itemKey.ToString(), out info);
    }
    private bool TryGetItemFromInfos(string itemKey, out InventoryInfo info) {
        if (activesInfo.TryGetValue(itemKey, out info)) { return true; }
        if (gadgetsInfo.TryGetValue(itemKey, out info)) { return true; }
        if (hullsInfo.TryGetValue(itemKey, out info)) { return true; }
        if (turretsInfo.TryGetValue(itemKey, out info)) { return true; }
        if (superInfos.TryGetValue(itemKey, out info)) { return true; }
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
    private bool TryGetSuperModi(string itemKey, string modiKey, int level, out double modiVal) {
        if (superInfos.TryGetValue(itemKey, out InventoryInfo info)) 
            if (info.TryGetModi(modiKey, level, out modiVal)) { return true; }
        modiVal = 0;
        return false;
    }

    public bool TryGetInfo(string id, out InventoryInfo info) {
        if (activesInfo.TryGetValue(id, out info)) { return true; }
        if (gadgetsInfo.TryGetValue(id, out info)) { return true; }
        if (hullsInfo.TryGetValue(id, out info)) { return true; }
        if (turretsInfo.TryGetValue(id, out info)) { return true; }
        if (superInfos.TryGetValue(id, out info)) { return true; }
        return false;
    }

    #endregion

    #region Methods
    public void EquipInfo(string id, int lvl = 1) {
        if (activesInfo.ContainsKey(id)) {
            EquipInfo(nameof(CSVType.ACTIVES), id, lvl);
        } else if (gadgetsInfo.ContainsKey(id)) {
            EquipInfo(nameof(CSVType.GADGETS), id, lvl);
        } else if (hullsInfo.ContainsKey(id)) {
            EquipInfo(nameof(CSVType.HULLS), id, lvl);
        } else if (turretsInfo.ContainsKey(id)) {
            EquipInfo(nameof(CSVType.TURRETS), id, lvl);
        }
    }
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
        //equip hull and turret. TODO: Make this do something lol
        EquipInfo(CSVType.HULLS, CSVId.TankHull, 1);
        EquipInfo(CSVType.TURRETS, CSVId.Autocannon, 1);

        //All actives
        EquipInfo(CSVType.ACTIVES, CSVId.HealActive, 11);
        EquipInfo(CSVType.ACTIVES, CSVId.SentryActive, 11);
        EquipInfo(CSVType.ACTIVES, CSVId.RapidFireActive, 11);
        EquipInfo(CSVType.ACTIVES, CSVId.RegenerativeArmorGadget, 11);
        

        //All gadgets
        EquipInfo(CSVType.GADGETS, CSVId.HardenedAmmoGadget, 11);
        EquipInfo(CSVType.GADGETS, CSVId.ImprovedLoaderGadget, 11);
        EquipInfo(CSVType.GADGETS, CSVId.HardenedArmorGadget, 11);
        EquipInfo(CSVType.GADGETS, CSVId.FireControlGadget, 11);
        EquipInfo(CSVType.GADGETS, CSVId.PolishedTriggerGadget, 11);
        EquipInfo(CSVType.GADGETS, CSVId.LaserSightGadget, 11);
        EquipInfo(CSVType.GADGETS, CSVId.BracedInternalsGadget, 11);
        


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

