using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities {

     class Heal : IActivatable, ISysTickable, IButtonRechargable {
        public float cooldownPeriod, healAmount, healDuration, remainingCooldownTime; 
        private float remainingHealTime = 0;
        private bool isActive = false;
        private UnityEngine.UI.Image outline = null;
        
        public Heal() { this.UpdateAbility(); }

        public void Activate(NetworkedEntity entity) { //reset the timer and activate ability.
            if (isActive || remainingCooldownTime != 0) return;
            remainingCooldownTime = cooldownPeriod;
            remainingHealTime = healDuration;
            isActive = true;
            
        }

        public bool GetIsActive() { return isActive; }

        public float GetCooldownPercentage() {
            return (cooldownPeriod - remainingCooldownTime) / cooldownPeriod;
        }

        public void SysTickCall() {
            if (isActive) {
                remainingHealTime = Mathf.Max(0, remainingHealTime - Time.deltaTime);
                if (remainingHealTime == 0) isActive = false;
                if (outline != null) { // Show that the ability is currently active + cooldown bar for that.
                    outline.fillAmount = remainingHealTime / healDuration;
                }
            } else {
                remainingCooldownTime = Mathf.Max(0, remainingCooldownTime - Time.deltaTime);
                if (outline != null) { // update the outline.
                    outline.fillAmount = (cooldownPeriod - remainingCooldownTime) / cooldownPeriod;
                }
            }
        }

        public void SetButtonOutlineProgressImage(UnityEngine.UI.Image outlineProgress) {
            outline = outlineProgress;
        }
    }

}
