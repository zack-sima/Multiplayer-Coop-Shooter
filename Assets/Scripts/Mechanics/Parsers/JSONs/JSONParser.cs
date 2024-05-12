using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSV;
using Newtonsoft.Json;
using Unity.VisualScripting;

namespace JSON {
    public static class JSONParser {

        private enum JSONType {
            TurretInfos,
            HullInfos,
            UpgradeInfos
        }

        /* public static void PullAllInfosFromPersistent(this (Dictionary<string, GarageInfo> turretInfos, Dictionary<string, GarageInfo> hullInfos, Dictionary<string, UpgradeInfo> upgradeInfos) input) {
            input.turretInfos = PullTurretInfosFromPersistent();
            input.hullInfos = PullHullInfosFromPersistent();
            input.upgradeInfos = PullUpgradeInfosFromPersistent();
        }

        public static void ForceBlankInfos(this (Dictionary<string, GarageInfo> turretInfos, Dictionary<string, GarageInfo> hullInfos, Dictionary<string, UpgradeInfo> upgradeInfos) input) {
            RemoveAllInfosFromPersistent();
            input.Item1 = InitBlankHullInfo();
            input.Item2 = InitBlankTurretInfo();
            input.Item3 = InitBlankUpgradesInfo();
            //TODO: Pull and pushing onto the persistent
        }

        public static void PushAllInfosOntoPersistent((Dictionary<string, GarageInfo> turretInfos, Dictionary<string, GarageInfo> hullInfos, Dictionary<string, UpgradeInfo> upgradeInfos) input) {
            PushTurretInfosOntoPersistent(input.turretInfos);
            PushHullInfosOntoPersistent(input.hullInfos);
            PushUpgradeInfosOntoPersistent(input.upgradeInfos);
        }

        private static Dictionary<string, GarageInfo> PullTurretInfosFromPersistent() {
            if (PersistentDict.HasKey(nameof(JSONType.TurretInfos))) {
                return DeserializeGarageInfo(PersistentDict.GetString(nameof(JSONType.TurretInfos)));
            } return InitBlankTurretInfo();
        }

        private static void PushTurretInfosOntoPersistent(Dictionary<string, GarageInfo> turretInfos) {
            PersistentDict.SetString(nameof(JSONType.TurretInfos), SerializeGarageInfo(turretInfos), true);
        }

        private static Dictionary<string, GarageInfo> PullHullInfosFromPersistent() {
            if (PersistentDict.HasKey(nameof(JSONType.HullInfos))) {
                return DeserializeGarageInfo(PersistentDict.GetString(nameof(JSONType.HullInfos)));
            } return InitBlankHullInfo();
        }

        private static void PushHullInfosOntoPersistent(Dictionary<string, GarageInfo> hullInfos) {
            PersistentDict.SetString(nameof(JSONType.HullInfos), SerializeGarageInfo(hullInfos), true);
        }

        private static Dictionary<string, UpgradeInfo> PullUpgradeInfosFromPersistent() {
            if (PersistentDict.HasKey(nameof(JSONType.UpgradeInfos))) {
                return DeserializeUpgradeInfo(PersistentDict.GetString(nameof(JSONType.UpgradeInfos)));
            } return InitBlankUpgradesInfo();
        }

        public static void PushUpgradeInfosOntoPersistent(Dictionary<string, UpgradeInfo> upgradeInfos) {
            PersistentDict.SetString(nameof(JSONType.UpgradeInfos), SerializeUpgradeInfo(upgradeInfos), true);
        }

        private static void RemoveAllInfosFromPersistent() {
            if (PersistentDict.HasKey(nameof(JSONType.TurretInfos)))PersistentDict.DeleteKey(nameof(JSONType.TurretInfos));
            if (PersistentDict.HasKey(nameof(JSONType.HullInfos)))PersistentDict.DeleteKey(nameof(JSONType.HullInfos));
            if (PersistentDict.HasKey(nameof(JSONType.UpgradeInfos)))PersistentDict.DeleteKey(nameof(JSONType.UpgradeInfos));
        } */

        public static Dictionary<string, GarageInfo> InitBlankTurretInfo() {
            //Debug.LogWarning("InitBlankTurretInfo");
            Dictionary<string, GarageInfo> temp = new();
            temp.ParseTurretInfos(GarageManager.instance.turretCSVProps.text);
            return temp;
        }

        public static Dictionary<string, GarageInfo> InitBlankHullInfo() {
            //Debug.LogWarning("InitBlankHullInfo");
            Dictionary<string, GarageInfo> temp = new();
            temp.ParseHullInfos(GarageManager.instance.hullCSVProps.text);
            return temp;
        }

        public static Dictionary<string, UpgradeInfo> InitBlankUpgradesInfo() {
            //Debug.LogWarning("InitBlankUpgradesInfo");
            return new(); // TODO: Implement this
        }



        // Serialize Dictionary<string, GarageInfo> to JSON
        private static string SerializeGarageInfo(Dictionary<string, GarageInfo> dict) {
            return JsonConvert.SerializeObject(dict);
        }

        // Deserialize JSON to Dictionary<string, GarageInfo>
        private static Dictionary<string, GarageInfo> DeserializeGarageInfo(string json) {
            return JsonConvert.DeserializeObject<Dictionary<string, GarageInfo>>(json);
        }

        private static Dictionary<string, UpgradeInfo> DeserializeUpgradeInfo(string json) {
            return JsonConvert.DeserializeObject<Dictionary<string, UpgradeInfo>>(json);
        }

        private static string SerializeUpgradeInfo(Dictionary<string, UpgradeInfo> dict) {
            return JsonConvert.SerializeObject(dict);
        }
    }
}