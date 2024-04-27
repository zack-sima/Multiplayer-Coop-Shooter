using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

namespace Abilities {

    /// <summary>
    /// Parent Interface
    /// </summary>
    public interface IAbility { }

    public interface IActivatable : IAbility { 
        /// <summary>
        /// Called when activated. Returns
        /// </summary>
        public void Activate();

        public bool GetIsActive();
    }

    public interface IPassiveable : IAbility { }

    public interface ISysTickable {
        /// <summary>
        /// Called every update by networked entity.
        /// </summary>
        public void SysTickCall();
    }

    public interface IButtonRechargable {
        /// <summary>
        /// Called by UIController to Set the Outline for updating.
        /// </summary>
        public void SetButtonOutlineProgressImage(UnityEngine.UI.Image outlineProgress);
    }

    public interface IIconable {
        // TODO: Callback returning ability Icon for the ability Button
        /// <summary>
        /// lmao self explanatory. Might want this to callback on each ability to a instance class that stores all the icons (ability info???)
        /// </summary>
        public Image GetIcon();
    }

    //Populate with more callbacks as needed Above.
    //Below is redacted interfaces.

    // public interface ICooldownable {
    //     /// <summary>
    //     /// Return a float between 0-1 that returns percentage charged. return (cooldownTime - remainingCooldownTime) / cooldownTime;
    //     /// </summary>
    //     public float GetCooldownPercentage();
    // }
}
