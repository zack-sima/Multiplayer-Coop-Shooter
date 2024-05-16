using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities {

	public static class AbilityInitExtensions {

		public static void UpdateAbilityList(this List<(IAbility, bool)> input) {

			//TODO: Link with garage / in game tech tree.
			if (input.Count > 0) return;

			input.Clear();

			RapidFire h = new();
			input.Add((h, false));
			Heal f = new();
			input.Add((f, false));

			AbilityUIManagerExtensions.OnAbilityListChange(); // callback for ui update. REQUIRED, o.w. no abilities will show.
		}

		public static void UpdateAbility(this IAbility i) {

			//TODO: Link with garage / in game tree.
			switch (i) {
				//==== HEALING ====//
				case Heal:
					((Heal)i).cooldownPeriod = ((Heal)i).remainingCooldownTime = 15f;
					((Heal)i).healAmount = .8f; // percentage heal.
					((Heal)i).healDuration = 1f;
					break;
				case AreaHeal:
					((AreaHeal)i).cooldownPeriod = ((AreaHeal)i).remainingCooldownTime = 15f;
					((AreaHeal)i).healAmount = .8f; // percentage heal.
					((AreaHeal)i).healPeriod = 5f;
					((AreaHeal)i).healRadius = 3f;
					break;
				case InfiHeal:
					((InfiHeal)i).cooldownPeriod = ((InfiHeal)i).remainingCooldownTime = 15f;
					((InfiHeal)i).healPerSec = 500f;
					((InfiHeal)i).healPeriod = 15f;
					break;
				case HPSteal:
					((HPSteal)i).cooldownPeriod = ((HPSteal)i).remainingCooldownTime = 15f;
					((HPSteal)i).stealAmount = 2000f; //FLAT hp stolen
					((HPSteal)i).stealPeriod = 2f;
					((HPSteal)i).stealRadius = 5f;
					break;
				case OverHeal:
					((OverHeal)i).cooldownPeriod = ((OverHeal)i).remainingCooldownTime = 15f;
					((OverHeal)i).healAmount = .8f; // percentage heal.
					((OverHeal)i).healPeriod = 5f;
					((OverHeal)i).maxOverhealAmount = 1000f; // Needs to go down again ?
					break;
				case Sentry:
					((Sentry)i).cooldownPeriod = ((Sentry)i).remainingCooldownTime = 2f;
					((Sentry)i).maxHealth = 2000;
					((Sentry)i).maxAmmo = 35;
					((Sentry)i).ammoRegen = 7.5f;
					((Sentry)i).shootSpeed = 0.08f;
					((Sentry)i).shootSpread = 0.1f;
					((Sentry)i).isFullAuto = true;
					((Sentry)i).dmgModi = 1f;

					break;
				//==== DAMAGE ====//
				case RapidFire:
					((RapidFire)i).cooldownPeriod = ((RapidFire)i).remainingCooldownTime = 15f;
					((RapidFire)i).firingPeriod = ((RapidFire)i).remainingFiringTime = 5f;
					break;
			}
		}
	}
}