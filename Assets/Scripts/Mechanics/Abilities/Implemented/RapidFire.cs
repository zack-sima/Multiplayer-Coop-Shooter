using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effects;
using CSV;
using CSV.Parsers;
using UnityEngine.UI;

namespace Abilities {

    class RapidFire : IActivatable, ISysTickable, ICooldownable {
        public float cooldownPeriod, remainingCooldownTime, firingPeriod, remainingFiringTime;
        private bool isActive = false;
        private string id;

        public bool GetIsActive() { return isActive; }
        public float GetCooldownPercentage() { return isActive ? remainingFiringTime / firingPeriod : (cooldownPeriod - remainingCooldownTime) / cooldownPeriod; }
        public string GetId() { return id; }
        
        public RapidFire(CSVId id, InventoryInfo info = null, int level = 1) { 
            this.id = id.ToString(); 
            remainingCooldownTime = 0;

            if (info == null && PlayerDataHandler.instance.TryGetInfo(id.ToString(), out InventoryInfo i)) 
                info = i;
            
            if (info == null) { DebugUIManager.instance?.LogError("No info found for " + id, "RapidFireActive"); return; }

            //Init stats
            if (info.TryGetModi(CSVMd.RapidDuration, level, out double h)) {
                firingPeriod = remainingFiringTime = (float)h;
            } else DebugUIManager.instance?.LogError("No RapidDuration found.", "RapidFireActive");
            if (info.TryGetModi(CSVMd.Cooldown, level, out double hd)) {
                cooldownPeriod = remainingCooldownTime = (float)hd;
            } else DebugUIManager.instance?.LogError("No Cooldown found.", "RapidFireActive");
            remainingCooldownTime = cooldownPeriod;
        
        }

        public void Activate(NetworkedEntity entity, bool isOverride = false) { //reset the timer and activate ability.
            if (!isOverride && (isActive || remainingCooldownTime != 0)) return;
            remainingCooldownTime = cooldownPeriod;
            remainingFiringTime = firingPeriod;
            isActive = true;
            
            //Effect
            GameObject fireEffect = entity.InitEffect(firingPeriod + 3f, 5f, CSVId.RapidFireActive);
            if (fireEffect == null) return;
            if (fireEffect.TryGetComponent(out Effect e)) {
                e.EnableDestroy(firingPeriod + 3f);
                e.EnableEarlyDestruct(5f);
            }
            
        }

        public void SysTickCall(NetworkedEntity entity) {
            if (isActive) {
                remainingFiringTime = Mathf.Max(0, remainingFiringTime - Time.deltaTime);
                if (remainingFiringTime == 0) isActive = false;
                entity.OverClockNetworkEntityCall();    
            } else {
                remainingCooldownTime = Mathf.Max(0, remainingCooldownTime - Time.deltaTime);
            }
        }

        public bool TryPushUpgrade(string id, InGameUpgradeInfo info) {
            if (id == nameof(CSVId.RapidFireActive) + "Faster Fire") {
                if (info.TryGetModi(nameof(CSVMd.Cooldown), out double cooldown)) {
                    cooldownPeriod -= (float)cooldown;
                } else DebugUIManager.instance?.LogError("No Cooldown found.", "RapidFireActive");
            } else return false;
            return true;
        }
    }
}