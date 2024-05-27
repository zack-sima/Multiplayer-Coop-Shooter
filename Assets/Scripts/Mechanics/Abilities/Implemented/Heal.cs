using CSV;
using CSV.Parsers;
using Handlers;
using Effects;
using UnityEngine;
using UnityEngine.UI;

namespace Abilities {

     class Heal : IActivatable, ISysTickable, ICooldownable {
        public float cooldownPeriod, healAmount, healDuration, remainingCooldownTime; 
        private float remainingHealTime = 0;
        private bool isActive = false;
        private readonly string id;

        public string GetId() { return id; }
        public bool GetIsActive() { return isActive; }
        public float GetCooldownPercentage() { return isActive ? remainingHealTime / healDuration : (cooldownPeriod - remainingCooldownTime) / cooldownPeriod; }

        public Heal(CSVId id, InventoryInfo info = null, int level = 1) { 
            this.id = id.ToString(); 

            if (info == null && PlayerDataHandler.instance.TryGetInfo(id.ToString(), out InventoryInfo i)) 
                info = i;
            
            if (info == null) { DebugUIManager.instance?.LogError("No info found for " + id, "HealActive"); return; }

            //Init stats
            if (info.TryGetModi(CSVMd.HealAmount, level, out double h)) {
                healAmount = (float)h;
            } else DebugUIManager.instance?.LogError("No HealAmount found.", "HealActive");
            if (info.TryGetModi(CSVMd.HealDuration, level, out double hd)) {
                healDuration = (float)hd;
            } else DebugUIManager.instance?.LogError("No HealDuration found.", "HealActive");
            if (info.TryGetModi(CSVMd.Cooldown, level, out double cd)) {
                cooldownPeriod = (float)cd;
            } else DebugUIManager.instance?.LogError("No Cooldown found.", "HealActive");
            remainingCooldownTime = cooldownPeriod;
        }

        public void Activate(NetworkedEntity entity, bool isOverride = false) { //reset the timer and activate ability.
            if (!isOverride && (isActive || remainingCooldownTime != 0)) return;
            remainingCooldownTime = cooldownPeriod;
            remainingHealTime = healDuration;
            isActive = true;

            //Effect
            GameObject healEffect = entity.InitEffect(healDuration, 0f, CSVId.HealActive);
            if (healEffect == null) return;
            if (healEffect.TryGetComponent(out Effect e)) {
                e.EnableDestroy(healDuration);
            }
            if (healEffect.TryGetComponent(out ParticleSystem p)) {
                //For Particle effects.
            }
        }

        public void SysTickCall(NetworkedEntity entity) {
            if (isActive) {
                remainingHealTime = Mathf.Max(0, remainingHealTime - Time.deltaTime);
                if (remainingHealTime == 0) isActive = false;
                StatModifier stats = new();
                stats.healthFlatModifier += healAmount * entity.GetEntity().GetMaxHealth() * Time.deltaTime / healDuration; 
                entity.PushStatChanges(stats);
            } else {
                remainingCooldownTime = Mathf.Max(0, remainingCooldownTime - Time.deltaTime);
            }
        }

        public bool TryPushUpgrade(string id, InGameUpgradeInfo info) {
            if (id == nameof(CSVId.HealActive) + "FasterHeals") {
                if (info.TryGetModi(nameof(CSVMd.Cooldown), out double cooldown)) 
                    cooldownPeriod -= (float)cooldown;
            } else if (id == nameof(CSVId.HealActive) + "BiggerHeals") {
                if (info.TryGetModi(nameof(CSVMd.HealAmount), out double heal)) 
                    healAmount += (float)heal;
            } else return false;
            return true;
        }
    }

}
