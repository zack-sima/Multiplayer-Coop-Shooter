using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {

    class RapidFire { // temp rn
        private float cooldownPeriod;
        private float remainingTime = 0f;
        public RapidFire(float cooldownPeriod) { 
            this.cooldownPeriod = cooldownPeriod; 
            //this.InitAbility();
        }
        public void SetParameters(float cooldownPeriod) {
            this.cooldownPeriod = cooldownPeriod; 
            remainingTime = 0;
        }
        public void Activate() {
            //reset the timer.
        }
        public float GetChargeTime() { return 1 - remainingTime / cooldownPeriod; }
        public float GetPercentCharged(float time) {
            return time;
        }
    }

    //populate with different ability stubs.
}