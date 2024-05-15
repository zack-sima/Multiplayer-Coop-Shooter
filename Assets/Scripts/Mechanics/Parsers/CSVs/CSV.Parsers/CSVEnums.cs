using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSV.Parsers {

    public enum CSVId { // StringId

        // ! NEED TO BE NAMED THE SAME AS IN THE [----Id] SLOT !

        #region //*==| HULLS |==*//

        TankHull, SpiderHull,

        #endregion

        #region //*==| TURRETS |==*//

        Autocannon, Gatling, ExplosiveCannon, Mortar, Flamethrower, DoubleCannon,

        #endregion
        #region //*==| ACTIVES |==*//

        HealActive, RapidFireActive, SentryActive,

        #endregion

        #region //*==| GADGETS |==*//
        
        HardenedAmmoGadget, HealGadget, ArmorGadget,


        #endregion
    }

    public enum CSVMd { // MODI == $"{nameof(Md.ModiId)}", & parse for [unlocked = 3] (compiler stuff lol)
        /* TYPE */ StringId,
        /* IDs */ ActiveId, UpgradeId, GadgetId, HullId, TurretId,
        /* BASIC */ Tags, Description, Display, Dupe, IUpgrade, UPTags,
        /* OPERATIONAL */ Add, Max, 
        /* UPGRADES */ Level, UPCost,
        /* BASICS MODIS  */ XPCost, MoneyCost, GemCost, Above, Cooldown, 
        /* GENERAL MODIS */ Dmg, Reload, AmmoRegen, MaxHP, CritChance, CritDmg,
        /* MISC MODIS */ M1, M2, M3, M4, M5, M6, M7, M8, M9, M10,

        /* HULL MODIS */ Health,

    }
}