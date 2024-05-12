using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effects;

namespace Abilities {

    class RapidFire : IActivatable, ISysTickable, IButtonRechargable {
    public float cooldownPeriod, remainingCooldownTime, firingPeriod, remainingFiringTime;
    private bool isActive = false;
    private UnityEngine.UI.Image outline = null;
    
    public RapidFire() { this.UpdateAbility(); }

    public void Activate(NetworkedEntity entity, bool isOverride = false) { //reset the timer and activate ability.
        if (!isOverride && (isActive || remainingCooldownTime != 0)) return;
        remainingCooldownTime = cooldownPeriod;
        remainingFiringTime = firingPeriod;
        isActive = true;
        
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
            }
        }
    }

    public void SetButtonOutlineProgressImage(UnityEngine.UI.Image outlineProgress) {
        outline = outlineProgress;
    }
}
}