using UnityEngine.UI;
using UnityEngine;

namespace Abilities {

    //[CreateAssetMenu(menuName = "AbilityIconScriptObj")]
    public class AbilityIconsScriptObj : ScriptableObject {
        [Header("Actives")]
        [SerializeField] public Sprite healActiveIcon;
        [SerializeField] public Sprite rapidFireActiveIcon;
        [SerializeField] public Sprite sentryActiveIcon;

        [Header("Gadgets")]
        [SerializeField] public Sprite hardenedAmmoGadgetIcon;
        [SerializeField] public Sprite healGadgetIcon;
        [SerializeField] public Sprite armorGadgetIcon;
        
    }
}
