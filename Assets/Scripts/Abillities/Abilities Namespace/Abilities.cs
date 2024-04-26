using System.Collections;
using System.Collections.Generic;
using Abilities.Internal;
using Abilities.Manager;
using UnityEngine;

namespace Abilities {

    //TODO: Need to like rethink this thing out again. prob wanna like make it cleaner, and have this class ONLY store the raw values 
    //and remaining durations, while abilitymanager actually deals with the on and offs.
    class RapidHeal : IActiveAbility, ITimeBased {
        private float cooldownPeriod;
        private float remainingTime = 0f;
        private float healRate;
        private float healDuration;
        private float remainingHealTime = 0;
        private bool isActive = false;
        
        public RapidHeal() { this.InitAbility(); }
        public void SetParameters((float cooldownPeriod, float healRate, float healDuration) input) {
            cooldownPeriod = input.cooldownPeriod; 
            healRate = input.healRate;
            healDuration = input.healDuration;
        }
        public void Activate() {
            //reset the timer.
        }
        public float GetChargeTime() { return 1 - remainingTime / cooldownPeriod; }
        public float GetPercentCharged(float deltaTime) {
            remainingTime = remainingTime - deltaTime > 0 ? remainingTime - deltaTime : 0;
            return remainingTime / cooldownPeriod;
        }
    }

    class RapidFire : IActiveAbility, ITimeBased {
        private float cooldownPeriod;
        private float remainingTime = 0f;
        public RapidFire(float cooldownPeriod) { 
            this.cooldownPeriod = cooldownPeriod; 
            this.InitAbility();
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