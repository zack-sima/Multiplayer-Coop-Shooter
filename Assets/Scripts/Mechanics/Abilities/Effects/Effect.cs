using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Effects {

    public class Effect : MonoBehaviour {

        private float totalTime;

        public void EnableDestroy(float duration) {
            totalTime = duration;
            Destroy(gameObject, duration);
        }

        public void EnableEarlyDestruct(float priorTime) {
            StartCoroutine(EarlyDestructTimer(priorTime));
        }

        private IEnumerator EarlyDestructTimer(float priorTime) {
            float timeTotal = totalTime;
            while(timeTotal > priorTime) {
                timeTotal -= Time.deltaTime;
                yield return null;
            }
            
            //Disable the looping!
            foreach(ParticleSystem p in gameObject.GetComponentsInChildren<ParticleSystem>()) {
                var main = p.main;
                main.loop = false;
            }

        }
        
        private void Start() {

            //Audio stuff?
            
        }
    }
    /// <summary>
    /// TODO: Add sounds and stuff for abilities.
    /// </summary>
    public interface IAudioable {

    }
}


