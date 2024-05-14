using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSV.Parsers;

namespace CSV {
    public class GaragePrefs {
        //TODO: Where the basics stuff like loadouts, number of loadout slots, player level, player xp, player money, player gems, player cash, player name, player id, player email, player password, player last login, player last logout, player last played, player last played level, player last played mode, player last played difficulty, player last played time, player last played score, player last played kills, player last played deaths, player last played assists, player last played damage dealt, player last played damage taken, player last played healing done, player last played healing taken, player last played damage blocked, player last played damage absorbed, player last played damage reflected, player last played damage dodged, player last played damage crit, player last played damage crit taken, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last played damage crit dealt, player last played damage crit blocked, player last played damage crit absorbed, player last played damage crit reflected, player last played damage crit dodged, player last
        //Copilot wrote the above bruh ^
        //Store this on the persistent dict 
        //Store all upgradeinfos and the equivalent on this.

        #region Nested

        public struct LoadoutInfo {
            public CSVId hullId, turretId;
            public List<CSVId> activeIds, gadgetIds;
        }

        #endregion

        #region Members

        public Dictionary<int, LoadoutInfo> loadouts;
        private string activesRawJSON, gadgetsRawJSON, hullsRawJSON, turretsRawJSON;

        #endregion
    }
}
