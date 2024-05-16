
// using UnityEngine;

// namespace Abilities {
//     public class RegenerativeArmor : IPassivable, IStatSysTickable {

//         public float hpPercentPerSec = 0;

//         public RegenerativeArmor(float hpPercentPerSec) {
//             this.hpPercentPerSec = hpPercentPerSec;
//         }

//         public void SysTickCall(NetworkedEntity entity, StatModifier stat) {
//             stat.healthFlatModifier += hpPercentPerSec * entity.GetEntity().GetBaseHealth() * Time.deltaTime;
//         }
//     }
// }