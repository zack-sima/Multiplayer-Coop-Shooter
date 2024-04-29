using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities {

    /// <summary>
    /// Parent Interface
    /// </summary>
    public interface IAbility { }

    public interface IActivatable : IAbility { 
        /// <summary>
        /// Called when ability is requested to be activated. 
        /// NEEDS to self-check whether or not ability CAN OR CANNOT be activated.
        /// </summary>
        public void Activate(NetworkedEntity entity);

        /// <summary>
        /// Return whether or not Active ability is, well active.
        /// </summary>
        public bool GetIsActive();
    }

    public interface IPassiveable : IAbility { }

    public interface ISysTickable {
        /// <summary>
        /// Called every update tick by PlayerInfo.instance. Treat as Update()
        /// </summary>
        public void SysTickCall();
    }

    public interface IButtonRechargable {
        /// <summary>
        /// Called by UIController to Set the Outline for updating. (!Circular button!)
        /// </summary>
        public void SetButtonOutlineProgressImage(UnityEngine.UI.Image outlineProgress);
    }

    public interface ITierable {
        /// <summary>
        /// Returns the current tier of the ability.
        /// </summary>
        public uint GetTier();

        
        public void SetTier(uint tier);

        public void IncrementTier();
    }
    
    public interface IIconable {
        // TODO: Callback returning ability Icon for the ability Button
        /// <summary>
        /// lmao self explanatory. Might want this to callback on each ability to a instance class that stores all the icons (ability info???)
        /// </summary>
        public UnityEngine.UI.Image GetIcon();
    }

    //Populate with more callbacks as needed Above.
    
}
