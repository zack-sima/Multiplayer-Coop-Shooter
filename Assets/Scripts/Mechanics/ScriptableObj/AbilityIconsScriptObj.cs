using UnityEngine.UI;
using UnityEngine;

namespace CSV.Parsers {

    //[CreateAssetMenu(menuName = "AbilityIconScriptObj")]
    public class AbilityIconsScriptObj : ScriptableObject {

        [Header("Defaults")]
        [SerializeField] public Sprite defaultActiveIcon;
        [SerializeField] public Sprite defaultGadgetIcon;

        [Header("Actives")]
        [SerializeField] public Sprite healActiveIcon;
        [SerializeField] public Sprite rapidFireActiveIcon;
        [SerializeField] public Sprite sentryActiveIcon;

        [Header("Gadgets")]
        [SerializeField] public Sprite hardenedAmmoGadgetIcon;
        [SerializeField] public Sprite healGadgetIcon;
        [SerializeField] public Sprite armorGadgetIcon;


        public Sprite GetIcon(CSVId id) {
            switch(id) {
                case CSVId.HealActive:
                    return healActiveIcon;
                case CSVId.RapidFireActive:
                    return rapidFireActiveIcon;
                case CSVId.SentryActive:
                    return sentryActiveIcon;
                case CSVId.HardenedAmmoGadget:
                    return hardenedAmmoGadgetIcon;
                case CSVId.HealGadget:
                    return healGadgetIcon;
                case CSVId.ArmorGadget:
                    return armorGadgetIcon;
                default:
                    return null;
            }
        }
        
    }
}
