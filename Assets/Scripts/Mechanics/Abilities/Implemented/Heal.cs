using CSV.Parsers;
using Effects;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Abilities {

     class Heal : IActivatable, ISysTickable, IButtonRechargable, IInitable {
        public float cooldownPeriod, healAmount, healDuration, remainingCooldownTime; 
        private float remainingHealTime = 0;
        private bool isActive = false;
        private Image outline = null;
        private Image abilityIcon = null;
        private Sprite active, regular;
        private readonly string id;

        public string GetId() { return id; }

        public Heal(CSVId id) { this.id = id.ToString(); }

        public void Activate(NetworkedEntity entity, bool isOverride = false) { //reset the timer and activate ability.
            if (!isOverride && (isActive || remainingCooldownTime != 0)) return;
            remainingCooldownTime = cooldownPeriod;
            remainingHealTime = healDuration;
            isActive = true;
            abilityIcon.sprite = active;

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
                    if (abilityIcon.sprite != regular) abilityIcon.sprite = regular;
                }
            }
        }

        public void SetButtonOutlineProgressImage(Image outlineProgress) {
            outline = outlineProgress;
            outline.color = Color.green;
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

        public void SetIconImage(Image iconImage) {
            if(PlayerDataHandler.instance.TryGetUIIcon(nameof(CSVId.HealActive), out (Sprite active, Sprite regular) s)) {
                abilityIcon = iconImage;
                iconImage.sprite = s.regular;
                active = s.active;
                regular = s.regular;
            } else DebugUIManager.instance?.LogError("No icon found for : ", "RapidFireActive");
        }
    }

}
