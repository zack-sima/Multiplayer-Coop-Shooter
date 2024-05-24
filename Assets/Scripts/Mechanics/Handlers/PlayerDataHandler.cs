using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSV.Parsers;
using System.Linq;

[System.Serializable]
public class PlayerDataHandler : MonoBehaviour {

    #region Nested

    private enum PersistentDictKeys {
        ActivesRawCSV, GadgetsRawCSV, HullsRawCSV, TurretsRawCSV
    }

    public enum IconType {
        Active
    }

    [System.Serializable]
    public class IconKeyValuePair {
        public string id;
        public Sprite icon;
    }

    [System.Serializable]
    public class IconBundle {
        public string id;
        public Sprite icon;
        //public IconKeyValuePair parent;
        public Sprite regularUiIcon, activeUiIcon;
        public List<IconKeyValuePair> children;
    }

    #endregion

    #region References

    public static PlayerDataHandler instance;

    [Header("Raw CSVs")]
    [SerializeField] public TextAsset activeRawCSV;
    [SerializeField] public TextAsset gadgetRawCSV, hullRawCSV, turretRawCSV;

    [Header("Icons")]
    [SerializeField] private List<IconBundle> icons;

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

    public Dictionary<string, int> GetEquippedInfos(string type) {
        if (equippedInfos.TryGetValue(type, out Dictionary<string, int> dict)) {
            return dict;
        }
        return new();
    }
    public Dictionary<string, int> GetEquippedInfos(CSVType type) {
        return GetEquippedInfos(type.ToString());
    }
    public bool TryGetGeneralIcon(string id, out Sprite s) {
        foreach(IconBundle icon in icons) {
            if (icon.id == id) {
                s = icon.icon;
                return true;
            }
        }
        s = null;
        return false;
    }
    public bool TryGetUIIcon(string id, out (Sprite active, Sprite regular) s) {
        foreach(IconBundle icon in icons) {
            if (icon.id == id) {
                s = (icon.activeUiIcon, icon.regularUiIcon);
                return true;
            }
        }
        s = (null, null);
        return false;
    }
    public bool TryGetIcon(string id, out IconKeyValuePair i) {
        foreach(IconBundle icon in icons) {
            if (icon.id == id) {
                IconKeyValuePair pair = new IconKeyValuePair {
                    id = icon.id,
                    icon = icon.icon
                };
                i = pair;
                return true;
            }

            foreach(IconKeyValuePair child in icon.children) {
                if (child.id == id) {
                    i = child;
                    return true;
                }
            }
        }
        i = null;
        return false;
    }
    public List<IconBundle> GetIcons() {
        return icons;
    }
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
    private void SetIconChildrenIds() {
        for(int i = 0; i < icons.Count; i++) {
            for(int j = 0; j < icons[i].children.Count; j++) {
                icons[i].children[j].id = icons[i].id + icons[i].children[j].id;
            }
        }
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
        // EquipInfo(CSVType.ACTIVES, CSVId.SentryActive, 11);
        EquipInfo(CSVType.ACTIVES, CSVId.SentryActive, 10);
        EquipInfo(CSVType.ACTIVES, CSVId.RapidFireActive, 10);
        // EquipInfo(CSVType.ACTIVES, CSVId.HealActive, 1);
        // EquipInfo(CSVType.GADGETS, CSVId.HardenedAmmoGadget, 1);
        // EquipInfo(CSVType.GADGETS, CSVId.RegenerativeArmorGadget, 1);

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
            SetIconChildrenIds();
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

