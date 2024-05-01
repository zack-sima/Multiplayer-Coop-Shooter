using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSVParser.Init {
    public static class CSVParserInitExtensions {

        /// <summary>
        /// Init Upgrade stats and stuff from corresponding CSVs!
        /// </summary>
        /// <param name="dict"></param>
        public static void ParseUpgradesFromAllCSV(this Dictionary<string, UpgradeInfo> dict /*TODO: CSV selection for different combos of trees */) {
            dict.Clear();

            //Grab different CSVs!
            Dictionary<string, string> generalCSVs = PlayerInfo.instance.csv.GetGeneralCSV();
            if (generalCSVs.TryGetValue("Props", out string props) 
                    && generalCSVs.TryGetValue("Dependencies", out string dependencies)
                    && generalCSVs.TryGetValue("Descriptions", out string descriptions)) {
                dict.InitGeneralCSV(props, dependencies, descriptions);
            }

            //TODO: Parse based on turret, hull, abilty selection, etc!
            //EX: grab from csv parser.
            
        }

        private static void InitGeneralCSV(this Dictionary<string, UpgradeInfo> dict, string props, string dependencies, string descriptions) {
            //Init headers and rows.
            string[] rows = props.Split(new char[] { '\n' });
            string[] rawHeaders = rows[0].Split(',');
            //Init headers
            List<string> headers = new();
            for(int i = 0; i < rawHeaders.Length; i++) {
                if (rawHeaders[i] == "") continue;
                headers.Add(rawHeaders[i]);
            }

            /*==============| PROP INITS |==============*/
            bool hasHeaderAppeared = false;
            for(int i = 0; i < rows.Length; i++) { 

                if (!hasHeaderAppeared || rows[i] == "[Header]") {  // Check for [Header] Attribute
                    hasHeaderAppeared = true;
                    continue;
                }

                string[] modi = rows[i].Split(',');
                if (modi.Length < 2) continue;
                if (modi[0] == "" || modi[0] == "[Special]") continue;

                UpgradeInfo tempUpgrade = new();
                for(int j = 1; j < modi.Length; j++) {
                    if (modi[j] == "") continue;
                    //TryParse into float for the value. if cant CONTINUE!
                    //push modi onto tempUpgrade
                }

                dict.Add(modi[0], tempUpgrade);
            }
            foreach(string s in dict.Keys) {
                Debug.LogWarning(s);
            }
            

            /*==============| DEPENDENCY INITS |==============*/

            /*==============| DESCRIPTION INITS |==============*/

            /*==============| CATALOG INITS |==============*/
                //Loop through dict to push changes to the tree.
        }

        //TODO: Upgrade stat parser to apply said upgrades.
        //1.5 % == (1.5 + 1) * value
        // Need a dict of upgrades!
    }

    public class UpgradeInfo {
        private Dictionary<string, float> modi = new();
        private List<string> modiIds = new(); 
        public List<string> GetModiIds() { return modiIds; }
        public string description { get; set; } = "";
        public uint count = 1;
        

        public List<UpgradeInfo> softRequirements;
        public List<UpgradeInfo> hardRequirements;

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
    }
}
