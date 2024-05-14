using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSV.Parsers {

    public enum CSVId { // StringId

        // ! NEED TO BE NAMED THE SAME AS IN THE [----Id] SLOT !

        #region //*==| HULLS |==*//

        #endregion

        #region //*==| TURRETS |==*//


        #endregion
        #region //*==| ACTIVES |==*//

        HealActive, RapidFireActive, SentryActive,

        #endregion

        #region //*==| GADGETS |==*//
        
        HardenedAmmoGadget, HealGadget, ArmorGadget,


        #endregion
    }

    public enum CSVMd { // MODI == $"{nameof(Md.ModiId)}", & parse for [unlocked = 3] (compiler stuff lol)
        /* TYPE */ ACTIVE, GADGET, HULL, TURRET, 
        /* IDs */ ActiveId, UpgradeId, GadgetId, HullId, TurretId,
        /* BASIC */ Tags, Description, Dupe, IUpgrade, UPTags,
        /* OPERATIONAL */ Add, Max, Unlocked, Locked, LockedCash, LockedGem, LockedXP, 
        /* UPGRADES */ Level, UPCost,
        /* BASICSMODIS  */ XPCost, MoneyCost, GemCost, Above,

    }
}