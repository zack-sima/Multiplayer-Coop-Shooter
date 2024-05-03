using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CSVParser.Init {
    public static class CSVParserInitExtensions {

        // * Important Tag
        // ! Issue tag
        // ? Question
        // TODO: Todos

        /// <summary>
        /// Init Upgrade stats and stuff from corresponding CSVs!
        /// * IMPORTANT: ASSOCIATED ATTRIBUTES:
        /// [Special] < Used in ---.Props.CSV && .Descriptions.CSV to skip line.
        /// [Soft] < Used in ---.Dependencies.CSV to mark as Soft Requirements.
        /// [Hard] < Used in ---.Dependencies.CSV to mark as Hard Requirements.
        /// [Mutual] < Used in ---.Dependencies.CSV to mark as Mutual Requirements.
        /// </summary>
        public static void ParseUpgradesFromAllCSV(this UpgradesCatalog catalog/*TODO: CSV selection for different combos of trees */) {
            Dictionary<string, UpgradeInfo> dict = new();

            //Grab different CSVs!
            Dictionary<string, string> generalCSVs = UpgradesCatalog.instance.csvStorage.GetGeneralCSV();
            if (generalCSVs.TryGetValue("Props", out string props) 
                    && generalCSVs.TryGetValue("Dependencies", out string dependencies)
                    && generalCSVs.TryGetValue("Descriptions", out string descriptions)) {
                dict.InitCSV(props, dependencies, descriptions);
            }

            //TODO: Parse based on turret, hull, abilty selection, etc!
            //EX: grab from csv parser.
            
        }

        private static void InitCSV(this Dictionary<string, UpgradeInfo> dict, string props, string dependencies, string descriptions) {
            //Init headers and rows.
            string[] rows = props.Split(new char[] { '\n' });
            string[] rawHeaders = rows[0].Split(',');
            //Init headers
            List<string> headers = new();
            for(int i = 0; i < rawHeaders.Length; i++) {
                if (rawHeaders[i] == "") continue;
                headers.Add(rawHeaders[i]);
            }

            /*==============| PROPERTY INITS |==============*/
            bool hasHeaderAppeared = false;
            for(int i = 0; i < rows.Length; i++) { 

                if (!hasHeaderAppeared || rows[i] == "[Header]") {  // Check for [Header] Attribute
                    hasHeaderAppeared = true;
                    continue;
                }

                string[] modi = rows[i].Split(',');
                if (modi.Length < 2) continue;
                if (modi[0] == "" || modi[0] == "[Special]") continue;

                UpgradeInfo tempUpgrade = new UpgradeInfo(modi[0]);
                for(int j = 1; j < modi.Length; j++) {
                    if (modi[j] == "") continue;
                    
                    if (float.TryParse(modi[j], out float result)) { //TryParse into float for the value. if cant CONTINUE!
                        if (headers.Count > j) {
                            tempUpgrade.PushModi(headers[j], result); //push modi onto tempUpgrade
                        }
                    }
                    
                }

                dict.Add(modi[0], tempUpgrade);
            }

            /*==============| DEPENDENCY INITS |==============*/
            rows = dependencies.Split(new char[] { '\n' });
            string lastHeader = "";
            for(int i = 0; i < rows.Length; i++) {
                string[] depens = rows[i].Split(',');
                string targetId = "";
                for (int j = 0; j < depens.Length; j++) {
                    if (depens[j].Contains("[Soft]")) { lastHeader = "[Soft]"; break; }
                    else if (depens[j].Contains("[Hard]")) { lastHeader = "[Hard]"; break; }
                    else if (depens[j].Contains("[Mutual]")) { lastHeader = "[Mutual]"; break;}

                    if (j == 0 && depens[j] == "") continue; // Empty checks
                    else if (depens[j] == "") continue;

                    if (j == 0) { targetId = depens[j]; continue; } // Target init

                    if (dict.TryGetValue(targetId, out UpgradeInfo target) && dict.ContainsKey(depens[j])) {
                        if (lastHeader == "[Soft]") target.softRequirements.Add(depens[j]);
                        else if (lastHeader == "[Hard]") target.hardRequirements.Add(depens[j]);
                        else if (lastHeader == "[Mutual]") target.mutualRequirements.Add(depens[j]);
                    }
                }
                if(dict.TryGetValue(targetId, out UpgradeInfo t)) {
                    t.softRequirements.StackDuplicateDependencies();
                    t.hardRequirements.StackDuplicateDependencies();
                    t.mutualRequirements.StackDuplicateDependencies();
                }
            }

            /*==============| DESCRIPTION INITS |==============*/
            rows = descriptions.Split(new char[] { '\n' });
            for (int i = 1; i < rows.Length; i++) { // SKIP the first line.
                string[] descripts = rows[i].Split(',');
                if (descripts.Length <= 0) break; // i dont think this is possible ? but whatever
                if (descripts[0] == "" || descripts[0] == "[Special]") continue;

                if (dict.TryGetValue(descripts[0], out UpgradeInfo target)) {
                    if (descripts.Length > 1) { target.description = descripts[1]; }
                    if (descripts.Length > 2) { target.type = descripts[2]; }
                }
            }

            foreach(KeyValuePair<string, UpgradeInfo> s in dict) {
                Debug.LogWarning(s.Key + s.Value.ToString());
            }

            /*==============| CATALOG INITS |==============*/
            foreach(KeyValuePair<string, UpgradeInfo> pair in dict) {
                if (pair.Value.TryGetModi("Cost", out float cost) && pair.Value.TryGetModi("Lvls", out float lvl)) {
                    if (lvl <= 1) { // Only one upgrade of it.
                        UpgradesCatalog.instance.AddUpgrade(pair.Key, cost: (int)cost, info: pair.Value,
                            softRequirements: pair.Value.softRequirements.StackDuplicateDependencies(),
                            hardRequirements: pair.Value.hardRequirements.StackDuplicateDependencies(),
                            mutuallyExclusiveUpgrades: pair.Value.mutualRequirements.StackDuplicateDependencies());
                        continue;
                    }
                    UpgradesCatalog.UpgradeNode priorNode = UpgradesCatalog.instance.AddUpgrade(pair.Key, cost: (int)cost,
                        info: pair.Value,level: 1, 
                        softRequirements: pair.Value.softRequirements.StackDuplicateDependencies(),
                        hardRequirements: pair.Value.hardRequirements.StackDuplicateDependencies(),
                        mutuallyExclusiveUpgrades: pair.Value.mutualRequirements.StackDuplicateDependencies());
                    for(int i = 2; i < lvl + 1; i++) {
                        List<string> hards = pair.Value.hardRequirements.StackDuplicateDependencies();
                        hards.Add(priorNode.GetUpgradeId());
                        UpgradesCatalog.UpgradeNode currentNode = UpgradesCatalog.instance.AddUpgrade(
                            pair.Key, cost: (int)cost, info: pair.Value, level: i, replacePrior: true,
                            softRequirements: pair.Value.softRequirements.StackDuplicateDependencies(),
                            hardRequirements: hards,
                            mutuallyExclusiveUpgrades: pair.Value.mutualRequirements.StackDuplicateDependencies());
                        currentNode.prior = priorNode;
                        priorNode = currentNode;
                    }
                } 
            }
            //TODO: Test this shit bruh.
        }

        //TODO: Fix the dependency thing. 
        private static List<string> StackDuplicateDependencies(this List<string> depens) {
            List<string> temp = new(depens);
            foreach(string s in temp) {
                int dupeCount = -1;
                for(int i = 0; i < depens.Count; i++) {
                    if (depens[i] == s) { 
                        dupeCount++;
                        depens.RemoveAt(i);
                        i--;
                    }
                }
                if (dupeCount > 0)
                    depens.Add(s + " " + UpgradesCatalog.ToRoman(dupeCount));
                else depens.Add(s);
            }
            if (depens.Count <= 0) return new();
            return depens;
        }

        //TODO: Upgrade stat parser to apply said upgrades.
        //1.5 dmg% == 150% == (1.5 + 1) * value 
        // Need a dict of upgrades!
    }

    public class UpgradeInfo {
        private Dictionary<string, float> modi = new();
        private List<string> modiIds = new(); 
        public List<string> GetModiIds() { return modiIds; }
        public string description { get; set; } = "";
        public string type = "";
        public uint count = 1;

        public readonly string upgradeId;

        public UpgradeInfo(string id) { upgradeId = id; }
        

        public List<string> softRequirements = new(), hardRequirements = new(), mutualRequirements = new();

        public void PushModi(string id, float input) {
            if (modi.ContainsKey(id)) modi[id] = input;
            else modi.Add(id, input);
        }
        
        public bool TryGetModi(string id, out float output) { 
            output = 0;
            if (modi.ContainsKey(id)) {
                output = modi[id];
                return true;
            } 
            return false;
        }

        public override string ToString() {
            string s = "";
            foreach(KeyValuePair<string, float> p in modi) {
                s += " " + p.Key + " : " + p.Value + ";";
            }
            s += " Description: " + description + ";";
            s += " Softs: ";
            foreach(string ss in softRequirements) {
                s += ss + " ";
            }
            return s;
        }
    }
}
