
using UnityEngine;
using CSV;
using CSV.Parsers;
using Handlers;

namespace Abilities {
    public class RegenerativeArmorGadget : IAbility {

        public float hpPercentPerSec = 0;
        private string id;
        public string GetId() { return id; }

        public RegenerativeArmorGadget(CSVId id) {
            this.id = id.ToString();
        }

        public bool TryPushUpgrade(string id, InGameUpgradeInfo info) {
            if (id == nameof(CSVId.RegenerativeArmorGadget) + "Enhanced Healing") {
                if (info.TryGetModi(nameof(CSVMd.HealAmount), out double hpPercentPerSec))
                    this.hpPercentPerSec += (float)hpPercentPerSec;
            } else return false;
            return true;
        }
        

        public void Init(InventoryInfo info, int level) {
            if (info.TryGetModi(nameof(CSVMd.HealAmount), level, out double hpPercentPerSec)) {
                this.hpPercentPerSec = (float)hpPercentPerSec;
            } else {
                DebugUIManager.instance?.LogError("No HPRegen found for " + id, "RegenerativeArmorGadget");
            }
        }

        public void SysTickCall(NetworkedEntity entity, StatModifier stat) {
            stat.healthFlatModifier += hpPercentPerSec * entity.GetEntity().GetBaseHealth() * Time.deltaTime;
        }
    }
}