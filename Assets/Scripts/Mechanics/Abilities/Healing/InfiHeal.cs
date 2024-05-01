using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effects;

namespace Abilities {

     class InfiHeal : IActivatable, ISysTickable, IButtonRechargable {
        public float cooldownPeriod, healPerSec, healPeriod, remainingCooldownTime; 
        private float remainingHealTime = 0;
        private bool isActive = false;
        private UnityEngine.UI.Image outline = null;
        
        public InfiHeal() { this.UpdateAbility(); }

        public void Activate(NetworkedEntity entity, bool isOverride = false) { //reset the timer and activate ability.
            if (!isOverride && (isActive || remainingCooldownTime != 0)) return;
            remainingCooldownTime = cooldownPeriod;
            remainingHealTime = healPeriod;
            isActive = true;
            
            GameObject healEffect = entity.InitEffect(entity.GetEffect(UpgradeIndex.InfiHeal), healPeriod + 2f, 5f, UpgradeIndex.InfiHeal);
            if (healEffect.TryGetComponent(out Effect effect)) {
                effect.EnableDestroy(healPeriod);
                effect.EnableEarlyDestruct(5f);
            }
            if (healEffect.TryGetComponent(out ParticleSystem p)) {
                //For Particle effects.
            }
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
                    outline.fillAmount = remainingHealTime / healPeriod;
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
