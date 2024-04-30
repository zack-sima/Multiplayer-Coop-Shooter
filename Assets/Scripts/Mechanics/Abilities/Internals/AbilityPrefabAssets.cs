using System;
using System.Collections;
using System.Collections.Generic;
using Abilities;
using UnityEngine;

namespace Effects {

    //[CreateAssetMenu(menuName = "Ability Prefab Assets")]
    public class AbilityPrefabAssets : ScriptableObject {

        //[Tooltip("baka")]

        [Header("Healing Based")]
        
        [SerializeField] public GameObject healEffectPrefab;
        [SerializeField] public GameObject areaHealEffectPrefab;
        [SerializeField] public GameObject infiHealEffectPrefab;
        [SerializeField] public GameObject hpStealEffectPrefab;

        [Header("Basic")]

        [SerializeField] public GameObject fireEffectPrefab;

    }

    public static class EffectHandler {
        public static GameObject GetEffect(this NetworkedEntity entity, UpgradeIndex i) {
            switch(i) {
                case UpgradeIndex.Heal:
                    return entity.effectPrefabs.healEffectPrefab;
                case UpgradeIndex.AreaHeal:
                    return entity.effectPrefabs.areaHealEffectPrefab;
                case UpgradeIndex.InfiHeal:
                    return entity.effectPrefabs.infiHealEffectPrefab;
                case UpgradeIndex.HPSteal:
                    return entity.effectPrefabs.hpStealEffectPrefab;
                case UpgradeIndex.RapidFire:
                    return entity.effectPrefabs.fireEffectPrefab;
            }
            return null;  
        }
    }
}
