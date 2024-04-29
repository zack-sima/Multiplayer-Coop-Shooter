using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Ability Prefab Assets")]
public class AbilityPrefabAssets : ScriptableObject {

    [Serializable]
    public class AreaHealPrefab {
        public float tempPlaceholder;
    }

    [SerializeField] 
    public AreaHealPrefab areaHealPrefab;

}
