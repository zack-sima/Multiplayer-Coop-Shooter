using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSV.Parsers {
    public static partial class CSVParserExtensions {
        public enum Id { // StringId

            // ! NEED TO BE NAMED THE SAME AS IN THE [----Id] SLOT !

            #region //*==| HULLS |==*//

            #endregion

            #region //*==| TURRETS |==*//


            #endregion
            #region //*==| ACTIVES |==*//

            HealActive, RapidFireActive, SentryActive,

            #endregion

            #region //*==| GADGETS |==*//
            HardenedAmmoGadget,
            HealGadget,
            ArmorGadget,


            #endregion
        }

        public enum Md { // MODI == $"{nameof(Md.ModiId)}"
            /* TYPE */ ACTIVE, GADGET, HULL, TURRET,
            /* BASIC */ Tags,
            /* OPERATIONAL */ Add, Max, Unlocked, Locked, LockedCash, LockedGem, LockedXP,
            /* BASICSMODIS  */ XPCost, MoneyCost, GemCost, Above,

        }
    }
}