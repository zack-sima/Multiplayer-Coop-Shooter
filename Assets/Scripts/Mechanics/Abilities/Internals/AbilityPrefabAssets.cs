using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Effects {
    public enum EffectIndex {
        Heal, AreaHeal, InfiHeal, HPSteal, RapidFire
    }

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
        public static GameObject GetEffect(this NetworkedEntity entity, EffectIndex i) {
            switch(i) {
                case EffectIndex.Heal:
                    return entity.effectPrefabs.healEffectPrefab;
                case EffectIndex.AreaHeal:
                    return entity.effectPrefabs.areaHealEffectPrefab;
                case EffectIndex.InfiHeal:
                    return entity.effectPrefabs.infiHealEffectPrefab;
                case EffectIndex.HPSteal:
                    return entity.effectPrefabs.hpStealEffectPrefab;
                case EffectIndex.RapidFire:
                    return entity.effectPrefabs.fireEffectPrefab;
            }
            return null;  
        }
    }
}
