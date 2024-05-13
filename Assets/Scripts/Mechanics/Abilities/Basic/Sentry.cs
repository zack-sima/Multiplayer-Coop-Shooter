using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effects;

namespace Abilities {
    //TODO: turret ability hook up here!
    public class Sentry : IActivatable, ISysTickable, IButtonRechargable {
        public float cooldownPeriod, remainingCooldownTime;
        public int team, maxHealth, maxAmmo;
        public float ammoRegen, shootSpeed, shootSpread, dmgModi;
        public bool isFullAuto;

        private UnityEngine.UI.Image outline = null;

        public Sentry() { this.UpdateAbility(); }

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
            if (outline != null) { // update the outline.
                outline.fillAmount = (cooldownPeriod - remainingCooldownTime) / cooldownPeriod;
            }     
        }

        public void SetButtonOutlineProgressImage(UnityEngine.UI.Image outlineProgress) {
            outline = outlineProgress;
        }
    }
}
