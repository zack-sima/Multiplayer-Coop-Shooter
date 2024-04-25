using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities.Internal {

    /// <summary>
    /// Parent Interface
    /// </summary>
    interface IAbility { }

    interface IActiveAbility : IAbility { }

    interface IPassiveAbility : IAbility { }

    interface IDamageBased {
        /// <summary>
        /// Returns the percentage damage charge.
        /// </summary>
        public float GetChargeDamage();
    }

    interface ITimeBased {
        /// <summary>
        /// Returns the percentage time charge.
        /// </summary>
        public float GetChargeTime();
    }

    //Populate with more callbacks as needed Below.
}
