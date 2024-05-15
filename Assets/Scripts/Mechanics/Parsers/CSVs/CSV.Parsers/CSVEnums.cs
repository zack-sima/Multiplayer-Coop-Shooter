using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSV.Parsers {

    public enum CSVId { // StringId

        // ! NEED TO BE NAMED THE SAME AS IN THE [----Id] SLOT !

        ACTIVES, TURRETS, GADGETS, HULLS,

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
        /* BASIC */ Tags, Description, Display, IUpgrade, UPTags,
        /* OPERATIONAL */ Add, Max, 
        /* UPGRADES */ Level, UPCost,
        /* BASICS MODIS  */ XPCost, MoneyCost, Above, Cooldown, 
        /* GENERAL MODIS */ Damage, Reload, AmmoRegen, MaxHP, CritChance, CritDamage,

        /* HULL MODIS */ Health,
        /* TURRET MODIS */ /*Damage*/ FireRate, MaxAmmo,

    }
}