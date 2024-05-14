using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using CSV;

namespace CSV.Parsers {
    
    public static partial class CSVParserExtensions {
        public static bool TryParseActive(this Dictionary<string, PlayerDataHandler.InventoryInfo> dict, string csv, bool debug) {

            string[] firstCut = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            string[] masterHeaders = firstCut[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            firstCut[0] = "";

            int row = 0;
            foreach(string s in firstCut) {
                string[] secondCut = s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                bool hasMetTags, hasMetUPTags;
                if (secondCut.Length < 1) continue;
                if (TryParseModi(secondCut[0], out string id)) {
                    if (id == nameof(CSVMd.ActiveId)) {
                        hasMetTags = hasMetUPTags = false;
                        
                        //dict.PushToDict()

                    }
                }
                foreach(string s2 in secondCut) {

                    if (debug) Debug.Log($"Row {row}: {s2}");

                }
            }
            

            //[Active ID]
            //[Tags]

            //levels

            //[UPTags]

            //[IUpgrades]










            return false;
        }
        private static void PushToDict(Dictionary<string, PlayerDataHandler.InventoryInfo> dict, string id, PlayerDataHandler.InventoryInfo info) {
            if (!dict.ContainsKey(id)) {
                dict.Add(id, info);
            } else {
                dict[id] = info;
            }
        }
        private static bool TryParseModi(string input, out string id) {
            id = "";
            string s = ExtractStringId(input)?.Replace(" ", "");
            if (s == null) return false;
            id = s;
            return true;
        }
        private static string ExtractStringId(string input) {
            var match = Regex.Match(input, @"\[(.*?)\]");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
    }
}