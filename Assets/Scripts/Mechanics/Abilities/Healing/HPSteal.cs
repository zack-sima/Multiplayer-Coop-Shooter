using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities {

     class HPSteal : IActivatable, ISysTickable, IButtonRechargable {
        public float cooldownPeriod, stealAmount, stealPeriod, stealRadius, totalHPStolen = 0; 
        private float remainingHealTime = 0, remainingCooldownTime = 0;
        private bool isActive = false;
        private UnityEngine.UI.Image outline = null;
        
        public HPSteal() { this.UpdateAbility(); }

        public void Activate(NetworkedEntity entity, bool isOverride = false) { //reset the timer and activate ability.
            if (!isOverride && (isActive || remainingCooldownTime != 0)) return;
            remainingCooldownTime = cooldownPeriod;
            remainingHealTime = stealPeriod;
            isActive = true;

            totalHPStolen = 0;

            //Effect all enemies around u
            foreach(CombatEntity e in EntityController.instance.GetCombatEntities()) {
                if (e.GetNetworker() == null) continue;
                if (e.GetNetworker() == entity) continue;
                if (e.GetNetworker().GetTeam() == entity.GetTeam()) continue;
                if (Vector3.Distance(e.transform.position, entity.transform.position) !< stealRadius) continue;
                totalHPStolen += stealAmount / stealPeriod;
               //e.GetNetworker().HealthFlatNetworkEntityCall(500f * Time.deltaTime);
                e.GetNetworker().LocalApplyInfliction(InflictionType.FlatHP, -stealAmount / stealPeriod, stealPeriod);
                e.GetNetworker().RPCApplyInfliction(InflictionType.FlatHP, -stealAmount / stealPeriod, stealPeriod);
                //e.GetNetworker().HealthFlatNetworkEntityCall(((Heal)a).healAmount * entity.GetEntity().GetMaxHealth() * Time.deltaTime / ((Heal)a).healDuration);
            }

            //TODO: Steal Animation.
            //TODO: Steal infliction.
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
                    outline.fillAmount = remainingHealTime / stealPeriod;
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
