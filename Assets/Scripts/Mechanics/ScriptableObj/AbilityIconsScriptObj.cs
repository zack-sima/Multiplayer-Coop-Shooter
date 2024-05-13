using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

namespace Abilities {

    //[CreateAssetMenu(menuName = "AbilityIconScriptObj")]
    public class AbilityIconsScriptObj : ScriptableObject {
        [Header("Abilities")]
        [SerializeField] public Image healAbilityIcon;
        [SerializeField] public Image areaHealAbilityIcon;

        [Header("Basics")]
        [SerializeField] public Image sentryAbilityIcon;
        [SerializeField] public Image rapidFireAbilityIcon;
        
    }
}
