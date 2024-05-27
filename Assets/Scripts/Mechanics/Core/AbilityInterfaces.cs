using System.Collections;
using System.Collections.Generic;
using CSV.Parsers;
using CSV;
using UnityEngine;

namespace Abilities {

    /// <summary>
    /// Parent Interface
    /// </summary>
    public interface IAbility { 
        /// <summary>
        /// 
        /// </summary>
        public string GetId();

        /// <summary>
        /// Returns true if the passed in gadget was a valid upgrade.
        /// </summary>
        public bool TryPushUpgrade(string id, InGameUpgradeInfo info);
    }

    public interface IActivatable : IAbility { 
        /// <summary>
        /// Called when ability is requested to be activated. 
        /// NEEDS to self-check whether or not ability CAN OR CANNOT be activated.
        /// </summary>
        public void Activate(NetworkedEntity entity, bool isOverride = false);

        /// <summary>
        /// Return whether or not Active ability is, well active.
        /// </summary>
        public bool GetIsActive();
    }

    public interface ISysTickable {
        /// <summary>
        /// Called every update tick by PlayerInfo.instance. Treat as Update()
        /// </summary>
        public void SysTickCall(NetworkedEntity entity);
    }

    public interface ICooldownable {
        /// <summary>
        /// Returns the percentage of cooldown remaining.
        /// </summary>
        public float GetCooldownPercentage();
    }


    public static class Extensions {
        public static bool TryGetActive(CSVId id, out IAbility a) {
            var list = NetworkedEntity.playerInstance.GetAbilityList();
            if (list == null) { a = null; return false; }
            foreach(var i in list) {
                if (i.Item1.GetId() == id.ToString()) {
                    a = i.Item1;
                    return true;
                }
            }
            a = null; return false;
        }

        public static bool TryGetCombatEntity(out CombatEntity c) {
            c = NetworkedEntity.playerInstance.GetCombatEntity();
            if (c == null) return false;
            else return true;
        }

        public static bool TryGetTurret(out Turret t) {
            TryGetCombatEntity(out CombatEntity c);
            if (c == null) { t = null; return false; }
            t = c.GetTurret();
            if (t == null) return false;
            return true;
        }
    }

    
    
}
