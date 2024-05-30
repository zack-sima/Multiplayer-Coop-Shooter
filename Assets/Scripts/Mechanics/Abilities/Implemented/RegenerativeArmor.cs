
using UnityEngine;
using CSV;
using CSV.Parsers;
using Handlers;

namespace Abilities {
    public class RegenerativeArmorGadget : IAbility, ISysTickable {

        public float hpPercentPerSec = 0;
        private string id;
        public string GetId() { return id; }

        public RegenerativeArmorGadget(CSVId id, InventoryInfo info = null, int level = 1) {
            this.id = id.ToString();
            if (info == null && PlayerDataHandler.instance.TryGetInfo(id.ToString(), out InventoryInfo i))
                info = i;
            if (info == null) {
                DebugUIManager.instance?.LogError("No info found for " + id, "RegenerativeArmorGadget");
                return;
            }
            if (info.TryGetModi(nameof(CSVMd.HealAmount), level, out double hpPercentPerSec)) {
                this.hpPercentPerSec = (float)hpPercentPerSec;
            } else {
                DebugUIManager.instance?.LogError("No HPRegen found for " + id, "RegenerativeArmorGadget");
            }
        }

        public bool TryPushUpgrade(string id, InGameUpgradeInfo info) {
            if (id == nameof(CSVId.RegenerativeArmorGadget) + "EnhancedHealing") {
                if (info.TryGetModi(nameof(CSVMd.HealAmount), out double hpPercentPerSec))
                    this.hpPercentPerSec += (float)hpPercentPerSec;
            } else return false;
            return true;
        }

        public void SysTickCall(NetworkedEntity entity) {
            StatModifier stat = new StatModifier();
            stat.healthFlatModifier += hpPercentPerSec * entity.GetEntity().GetBaseHealth() * Time.deltaTime;
            entity.PushStatChanges(stat);
        }
    }
}