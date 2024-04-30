using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abilities {
    public enum InflictionType {
        FlatHP,
        PercentHP
    }
    public static class InflictionHandler {
        public static void InflictionHandlerSysTick(this NetworkedEntity target, List<(InflictionType type, float param, float time)> inflicts) {
            //clean the list of inflictions.
            //Debug.Log("Systick ");
            for(int i = 0; i < inflicts.Count; i++) {
                if (inflicts[i].time - Time.deltaTime < 0) {
                    inflicts.RemoveAt(i);
                    i--;
                    continue;
                } 
                inflicts[i] = (inflicts[i].type, inflicts[i].param, inflicts[i].time - Time.deltaTime);
                target.InvokeInflictionHandler(inflicts[i].type, inflicts[i].param);
            }
        }

        public static void InvokeInflictionHandler(this NetworkedEntity target, InflictionType type, float param) {
            //Debug.Log("Invoke");
            switch(type) {
                case InflictionType.FlatHP:
                    //Debug.Log("FLATHP " + param);
                    if (param < 0) break;
                    target.HealthFlatNetworkEntityCall(param * Time.deltaTime);
                    break;
                case InflictionType.PercentHP:
                    if (param < 0) break;
                    target.HealthPercentNetworkEntityCall(param * Time.deltaTime);
                    break;
                
            }
        }

        public static void InitInfliction(this List<(InflictionType t, float, float)> inflicts, 
                InflictionType type, float param, float time) {
            for(int i = 0; i < inflicts.Count; i++) {
                //Debug.Log("Infliction Updated");
                if (inflicts[i].t == type) {
                    inflicts[i] = (inflicts[i].t, param, time);
                    return;
                }
            }
            //Debug.Log("Infliction Added");
            inflicts.Add((type, param, time));
        }
    }
}


