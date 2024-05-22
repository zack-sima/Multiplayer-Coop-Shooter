using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effects;
using CSV.Parsers;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Abilities {
    //TODO: turret ability hook up here!
    public class Sentry : IActivatable, ISysTickable, IButtonRechargable, IInitable {
        public float cooldownPeriod, remainingCooldownTime;
        public int team, maxHealth, maxAmmo;
        public float ammoRegen, shootSpeed, shootSpread, dmgModi;
        public bool isFullAuto;
        private string id;

        private Image outline = null;
        private Image abilityIcon = null;
        private Sprite active, regular;

        public Sentry(CSVId id) { this.id = id.ToString(); }

        public void Activate(NetworkedEntity entity, bool isOverride = false) { //reset the timer and activate ability.
            if (!isOverride && remainingCooldownTime != 0) return;
            remainingCooldownTime = cooldownPeriod;
            NetworkedEntity sentry = entity.SpawnSentry();

            sentry.SetSentryStats(entity.GetTeam(), maxHealth, maxAmmo, ammoRegen, shootSpeed, shootSpread, dmgModi, isFullAuto);
            outline.fillAmount = 0f;

            GameObject sentryEffect = sentry.InitEffect(1, 0f, UpgradeIndex.Sentry); //Effect
            if (sentryEffect == null) return;
            if (sentryEffect.TryGetComponent(out Effect e)) {
                e.EnableDestroy(1f);
            }
            // if (sentryEffect.TryGetComponent(out ParticleSystem p)) {
            //     //For Particle effects.
            // }

        }

        public bool GetIsActive() { return false; }

        public float GetCooldownPercentage() {
            return (cooldownPeriod - remainingCooldownTime) / cooldownPeriod;
        }

        public void SysTickCall() {
            remainingCooldownTime = Mathf.Max(0, remainingCooldownTime - Time.deltaTime);
            if (remainingCooldownTime == 0) {
                if (abilityIcon.sprite != active) abilityIcon.sprite = active;
            } else if (abilityIcon.sprite != regular) abilityIcon.sprite = regular;
            if (outline != null) { // update the outline.
                outline.fillAmount = (cooldownPeriod - remainingCooldownTime) / cooldownPeriod;
            }     
        }

        public void SetButtonOutlineProgressImage(UnityEngine.UI.Image outlineProgress) {
            outline = outlineProgress;
        }

        public string GetId() {
            return id;
        }

        public void Init(InventoryInfo info, int level) {
            if (info.TryGetModi(CSVMd.Cooldown, level, out double cooldown)) cooldownPeriod = (float)cooldown; else DebugUIManager.instance?.LogError("No cooldown found for " + id, "SentryInit");
            if (info.TryGetModi(CSVMd.SentryHealth, level, out double health)) maxHealth = (int)health; else DebugUIManager.instance?.LogError("No health found for " + id, "SentryInit");
            if (info.TryGetModi(CSVMd.SentryMaxAmmo, level, out double ammo)) maxAmmo = (int)ammo; else DebugUIManager.instance?.LogError("No ammo found for " + id, "SentryInit"); 
            if (info.TryGetModi(CSVMd.SentryAmmoRegen, level, out double regen)) ammoRegen = (float)regen; else DebugUIManager.instance?.LogError("No regen found for " + id, "SentryInit");
            if (info.TryGetModi(CSVMd.SentryShootSpeed, level, out double fireRate)) shootSpeed = (float)fireRate; else DebugUIManager.instance?.LogError("No fire rate found for " + id, "SentryInit");
            if (info.TryGetModi(CSVMd.SentryShootSpread, level, out double spread)) shootSpread = (float)spread; else DebugUIManager.instance?.LogError("No spread found for " + id, "SentryInit");
            if (info.TryGetModi(CSVMd.SentryDamage, level, out double damage)) dmgModi = (float)damage; else DebugUIManager.instance?.LogError("No damage found for " + id, "SentryInit");
            remainingCooldownTime = cooldownPeriod;
        }

        public void SetIconImage(Image iconImage) {
            if(PlayerDataHandler.instance.TryGetUIIcon(nameof(CSVId.SentryActive), out (Sprite active, Sprite regular) s)) {
                abilityIcon = iconImage;
                iconImage.sprite = s.regular;
                active = s.active;
                regular = s.regular;
            } else DebugUIManager.instance?.LogError("No icon found for : ", "RapidFireActive");
        }
    }
}
