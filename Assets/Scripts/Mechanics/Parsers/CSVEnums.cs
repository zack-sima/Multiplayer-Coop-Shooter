using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSV.Parsers {

	public enum CSVType {
		ACTIVES, TURRETS, GADGETS, HULLS,
	}

	public enum CSVId { // StringId

		// ! NEED TO BE NAMED THE SAME AS IN THE [----Id] SLOT !

		#region //*==| HULLS |==*//

		TankHull, SpiderHull,

		#endregion

		#region //*==| TURRETS |==*//

		Autocannon, Gatling, ExplosiveCannon, Mortar, Flamethrower, DoubleCannon,

		#endregion

		#region //*==| ACTIVES |==*//

		HealActive, RapidFireActive, SentryActive,

		#endregion

		#region //*==| GADGETS |==*//

		HardenedAmmoGadget, ImprovedLoaderGadget, HardenedArmorGadget, FireControlGadget, PolishedTriggerGadget, LaserSightGadget, BracedInternalsGadget,
		RegenerativeArmorGadget,

		#endregion
	}

	/// <summary>
	/// Enum for CSV keys
	/// </summary>
	public enum CSVMd { // MODI == $"{nameof(Md.ModiId)}", & parse for [unlocked = 3] (compiler stuff lol)
		/* TYPE */
		StringId,
		/* BASIC */
		Tags, Description, Display, IUpgrade, UPTags,
		/* OPERATIONAL */
		Add, Max,
		/* UPGRADE */
		Level, UPCost,
		/* BASIC */
		XPCost, MoneyCost, Above, Cooldown,
		/* GENERAL */
		Damage, Reload, AmmoRegen, MaxHP, CritChance, CritDamage,

		/* HULL */
		Health,
		/* TURRET */ /*Damage*/
		FireRate, MaxAmmo,

		/* HEALTH */
		HealAmount,

		/* Durations */
		RapidDuration, HealDuration,

		/* Sentries */
		SentryDamage, SentryHealth, SentryShootSpeed, SentryMaxAmmo, SentryAmmoRegen, SentryShootSpread, SentryCritChance, SentryCritDamage,

	}
}