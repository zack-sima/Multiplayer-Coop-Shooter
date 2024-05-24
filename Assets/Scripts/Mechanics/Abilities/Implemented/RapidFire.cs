using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effects;
using CSV.Parsers;
using UnityEngine.UI;

namespace Abilities {

    class RapidFire : IActivatable, ISysTickable, IButtonRechargable, IInitable {
        public float cooldownPeriod, remainingCooldownTime, firingPeriod, remainingFiringTime;
        private bool isActive = false;
        private Image outline = null;
        private Image abilityIcon = null;
        private Sprite active, regular;
        private string id;
        
        
        public RapidFire(CSVId id) { this.id = id.ToString(); remainingCooldownTime = 0;}

        public void Activate(NetworkedEntity entity, bool isOverride = false) { //reset the timer and activate ability.
            if (!isOverride && (isActive || remainingCooldownTime != 0)) return;
            remainingCooldownTime = cooldownPeriod;
            remainingFiringTime = firingPeriod;
            isActive = true;
            abilityIcon.sprite = active;
            
            //Effect
            GameObject fireEffect = entity.InitEffect(firingPeriod + 3f, 5f, UpgradeIndex.RapidFire);
            if (fireEffect == null) return;
            if (fireEffect.TryGetComponent(out Effect e)) {
                e.EnableDestroy(firingPeriod + 3f);
                e.EnableEarlyDestruct(5f);
            }
            
        }

        public bool GetIsActive() { return isActive; }

        public float GetCooldownPercentage() {
            return (cooldownPeriod - remainingCooldownTime) / cooldownPeriod;
        }

        public void SysTickCall() {
            if (isActive) {
                remainingFiringTime = Mathf.Max(0, remainingFiringTime - Time.deltaTime);
                if (remainingFiringTime == 0) isActive = false;
                if (outline != null) { // Show that the ability is currently active + cooldown bar for that.
                    outline.fillAmount = remainingFiringTime / firingPeriod;
                }
            } else {
                remainingCooldownTime = Mathf.Max(0, remainingCooldownTime - Time.deltaTime);
                if (outline != null) { // update the outline.
                    outline.fillAmount = (cooldownPeriod - remainingCooldownTime) / cooldownPeriod;
                    if (remainingCooldownTime != 0 && abilityIcon.sprite != regular) abilityIcon.sprite = regular;
                    else if (remainingCooldownTime == 0 && abilityIcon.sprite != active) abilityIcon.sprite = active;
                }
            }
        }

        public void SetButtonOutlineProgressImage(UnityEngine.UI.Image outlineProgress) {
            outline = outlineProgress;
        }

        public string GetId() {
            return id;
        }

        public void Init(InventoryInfo info, int level) {
            if (info.TryGetModi(CSVMd.RapidDuration, level, out double h)) {
                firingPeriod = remainingFiringTime = (float)h;
            } else DebugUIManager.instance?.LogError("No RapidDuration found.", "RapidFireActive");
            if (info.TryGetModi(CSVMd.Cooldown, level, out double hd)) {
                cooldownPeriod = remainingCooldownTime = (float)hd;
            } else DebugUIManager.instance?.LogError("No Cooldown found.", "RapidFireActive");
            remainingCooldownTime = cooldownPeriod;
        }

        public void SetIconImage(Image iconImage) {
            if(PlayerDataHandler.instance.TryGetUIIcon(nameof(CSVId.RapidFireActive), out (Sprite active, Sprite regular) s)) {
                abilityIcon = iconImage;
                iconImage.sprite = s.regular;
                active = s.active;
                regular = s.regular;
            } else DebugUIManager.instance?.LogError("No icon found for : ", "RapidFireActive");
        }
    }
}