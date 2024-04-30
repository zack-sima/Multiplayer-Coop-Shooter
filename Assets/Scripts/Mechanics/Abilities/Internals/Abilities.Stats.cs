

using System.Collections.Generic;

namespace Abilities {

    public enum UpgradeIndex {
        /*========| HEAL |========*/
        Heal, AreaHeal, InfiHeal, HPSteal, RapidFire

        /*========| UPGRADES |========*/


        /*========| BALLS |========*/
    }

    public static class AbilityBase {
        
        public static Dictionary<UpgradeIndex, IAbility> baseUpgradeValues = new() {
            /*========| HEAL |========*/
            { UpgradeIndex.Heal, new Heal() }

        };
    }
}
