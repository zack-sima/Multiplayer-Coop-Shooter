
using UnityEngine;
using CSV.Parsers;

namespace Abilities {
    public class RegenerativeArmorGadget : IPassivable, IStatSysTickable, IInitable {

        public float hpPercentPerSec = 0;
        private string id;

        public RegenerativeArmorGadget(CSVId id) {
            this.id = id.ToString();
        }

        public string GetId() {
            throw new System.NotImplementedException();
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