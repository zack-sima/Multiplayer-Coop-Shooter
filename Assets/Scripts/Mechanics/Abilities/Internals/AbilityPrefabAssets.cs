using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Effects {
    public enum EffectIndex {
        Heal, RapidFire
    }

    //[CreateAssetMenu(menuName = "Ability Prefab Assets")]
    public class AbilityPrefabAssets : ScriptableObject {

        //[Tooltip("baka")]

        

        [Header("Healing Based")]
        
        [SerializeField] public GameObject healEffectPrefab;
        [SerializeField] public GameObject areaHealEffectPrefab;

        [Header("Basic")]

        [SerializeField] public GameObject fireEffectPrefab;

    }

    public static class EffectHandler {
        public static GameObject GetEffect(this NetworkedEntity entity, EffectIndex i) {
            switch(i) {
                case EffectIndex.Heal:
                    return entity.effectPrefabs.healEffectPrefab;
                case EffectIndex.RapidFire:
                    return entity.effectPrefabs.fireEffectPrefab;
            }
            return null;  
        }
    }
}
