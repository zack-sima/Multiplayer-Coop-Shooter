using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSV;

namespace CSV.Parsers {
    
    public static partial class CSVParserExtensions {
        public static bool TryParseActive(this Dictionary<string, PlayerDataHandler.InventoryInfo> dict, string csv, bool debug) {

            string[] firstCut = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (debug) {
                int row = 0;
                foreach(string s in firstCut) {
                    DebugUIManager.instance.LogOutput(s + " : " + row);
                    row++;
                }
            }
            

            //[Active ID]
            //[Tags]

            //levels

            //[UPTags]

            //[IUpgrades]










            return false;
        }
    }
}