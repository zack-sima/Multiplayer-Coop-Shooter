using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Effects {
    // [CreateAssetMenu(menuName = "EffectStorage")]
    public class EffectStorage : ScriptableObject {
        
        [SerializeField]
        public AudioSource critSound;
    }
}
