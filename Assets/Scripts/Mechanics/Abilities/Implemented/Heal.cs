using CSV.Parsers;
using Effects;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abilities {

     class Heal : IActivatable, ISysTickable, IButtonRechargable, IInitable {
        public float cooldownPeriod, healAmount, healDuration, remainingCooldownTime; 
        private float remainingHealTime = 0;
        private bool isActive = false;
        private UnityEngine.UI.Image outline = null;
        private readonly string id;

        public string GetId() { return id; }

        public Heal(CSVId id) { this.id = id.ToString(); }

        public void Activate(NetworkedEntity entity, bool isOverride = false) { //reset the timer and activate ability.
            if (!isOverride && (isActive || remainingCooldownTime != 0)) return;
            remainingCooldownTime = cooldownPeriod;
            remainingHealTime = healDuration;
            isActive = true;

            //Effect
            GameObject healEffect = entity.InitEffect(healDuration, 0f, UpgradeIndex.Heal);
            if (healEffect == null) return;
            if (healEffect.TryGetComponent(out Effect e)) {
                e.EnableDestroy(healDuration);
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

        public void Init(InventoryInfo info, int level) {
            if (info.TryGetModi(CSVMd.HealAmount, level, out double h)) {
                healAmount = (float)h;
            } else DebugUIManager.instance?.LogError("No HealAmount found.", "HealActive");
            if (info.TryGetModi(CSVMd.HealDuration, level, out double hd)) {
                healDuration = (float)hd;
            } else DebugUIManager.instance?.LogError("No HealDuration found.", "HealActive");
            if (info.TryGetModi(CSVMd.Cooldown, level, out double cd)) {
                cooldownPeriod = (float)cd;
            } else DebugUIManager.instance?.LogError("No Cooldown found.", "HealActive");
        }
    }

}
