
using UnityEngine;
using CSV;

namespace Effects {

    //[CreateAssetMenu(menuName = "Ability Prefab Assets")]
    public class AbilityPrefabAssets : ScriptableObject {

        //[Tooltip("baka")]

        [Header("Healing Based")]
        
        [SerializeField] public GameObject healEffectPrefab;
        [SerializeField] public GameObject areaHealEffectPrefab;
        [SerializeField] public GameObject infiHealEffectPrefab;
        [SerializeField] public GameObject hpStealEffectPrefab;

        //! RMBR TO ATTACH A EFFECT SCRIPT TO THE PREFAB !

        [Header("Basic")]

        [SerializeField] public GameObject fireEffectPrefab;
        [SerializeField] public GameObject sentryEffectPrefab;

        //! RMBR TO ATTACH A EFFECT SCRIPT TO THE PREFAB !

    }

    public static class EffectHandler {
        public static GameObject GetEffect(this NetworkedEntity entity, CSVId i) {
            switch(i) {
                case CSVId.HealActive:
                    return entity.effectPrefabs.healEffectPrefab;
                case CSVId.RapidFireActive:
                    return entity.effectPrefabs.fireEffectPrefab;
                case CSVId.SentryActive:
                    return entity.effectPrefabs.sentryEffectPrefab;
                // case UpgradeIndex.AreaHeal:
                //     return entity.effectPrefabs.areaHealEffectPrefab;
                // case UpgradeIndex.InfiHeal:
                //     return entity.effectPrefabs.infiHealEffectPrefab;
                // case UpgradeIndex.HPSteal:
                //     return entity.effectPrefabs.hpStealEffectPrefab;
                //! RMBR TO ATTACH A EFFECT SCRIPT TO THE PREFAB !
                default:
                    return null;
            }
        }
    }
}
