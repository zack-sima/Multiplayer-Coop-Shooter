using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities.Internal {

    /// <summary>
    /// Parent Interface
    /// </summary>
    public interface IAbility { }

    public interface IActiveAbility : IAbility { 
        /// <summary>
        /// Called when activated
        /// </summary>
        public void Activate();
    }

    public interface IPassiveAbility : IAbility { }

    public interface IDamageBased {
        /// <summary>
        /// Returns the percentage damage charge.
        /// </summary>
        public float GetChargeDamage();
    }

    public interface ITimeBased {
        /// <summary>
        /// Returns the max Charge time.
        /// </summary>
        public float GetChargeTime();

        /// <summary>
        /// Input the deltaTime.
        /// Returns the percentage charge.
        /// </summary>
        public float GetPercentCharged(float time);
    }

    //Populate with more callbacks as needed Below.
}
