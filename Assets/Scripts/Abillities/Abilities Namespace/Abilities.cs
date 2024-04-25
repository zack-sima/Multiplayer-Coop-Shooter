using System.Collections;
using System.Collections.Generic;
using Abilities.Internal;
using UnityEngine;

namespace Abilities {
    class AbilityInfo { }

    class RapidHeal : AbilityInfo, IActiveAbility, ITimeBased {
        private float cooldownPeriod;
        public RapidHeal(float cooldownPeriod) { this.cooldownPeriod = cooldownPeriod; }
        public float GetChargeTime() { return cooldownPeriod; }
    }

    class RapidFire : AbilityInfo, IActiveAbility, ITimeBased {
        private float cooldownPeriod;
        public RapidFire(float cooldownPeriod) { 
            this.cooldownPeriod = cooldownPeriod; 
        }
        public float GetChargeTime() { return cooldownPeriod; }
    }

    //populate with different ability stubs.
}