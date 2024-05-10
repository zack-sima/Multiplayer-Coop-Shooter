using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSV {
    //[CreateAssetMenu(menuName = "CSVStorage")]
    public class CSVStorage : ScriptableObject {
        [Header("General CSV")]
        [Tooltip("Requires a .CSV to function. Auto-Parses and generates in-game tree.")]
        [SerializeField] private TextAsset generalCSVProps;
        [SerializeField] private TextAsset generalCSVDependencies;
        [SerializeField] private TextAsset generalCSVDescriptions;

        // [Header("Auto-Cannon CSVs")]
        // [SerializeField] private TextAsset autoCannonCSVProps;
        // [SerializeField] private TextAsset autoCannonCSVDependencies;
        // [SerializeField] private TextAsset autoCannonCSVDescriptions;

        //TODO: Populate with more CSVs for different trees and stuff.

        public Dictionary<string, string> GetCSVs() { 
            return new Dictionary<string, string>() { 
                {"General.Props", generalCSVProps.text}, 
                {"General.Dependencies", generalCSVDependencies.text},
                {"General.Descriptions", generalCSVDescriptions.text}
            };
        }
    }
}   
