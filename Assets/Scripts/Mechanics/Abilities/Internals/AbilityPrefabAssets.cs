using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Ability Prefab Assets")]
public class AbilityPrefabAssets : ScriptableObject {

    //[Tooltip("baka")]

    [Header("Healing Based")]
    
    [SerializeField] public GameObject healEffectPrefab;
    [SerializeField] public GameObject areaHealEffectPrefab;

    [Header("Basic")]

    [SerializeField] public GameObject fireEffectPrefab;

}
