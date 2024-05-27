using UnityEngine;
using Effects;
using CSV;
using CSV.Parsers;

namespace Abilities {
	
	public class Sentry : IActivatable, ISysTickable, ICooldownable {
		public float cooldownPeriod, remainingCooldownTime;
		public int team, maxHealth, maxAmmo;
		public float ammoRegen, shootSpeed, shootSpread, dmgModi;
		public bool isFullAuto;
		private string id;

		public bool GetIsActive() { return false; }
		public float GetCooldownPercentage() { return (cooldownPeriod - remainingCooldownTime) / cooldownPeriod; }
		public string GetId() { return id; }

		public Sentry(CSVId id, InventoryInfo info = null, int level = 1) { 
			this.id = id.ToString(); 
			
			if (info == null && PlayerDataHandler.instance.TryGetInfo(id.ToString(), out InventoryInfo i)) 
				info = i;
			
			if (info == null) { DebugUIManager.instance?.LogError("No info found for " + id, "SentryInit"); return; }

			//Init stats
			if (info.TryGetModi(CSVMd.Cooldown, level, out double cooldown)) cooldownPeriod = (float)cooldown;
			else DebugUIManager.instance?.LogError("No cooldown found for " + id, "SentryInit");

			if (info.TryGetModi(CSVMd.SentryHealth, level, out double health)) maxHealth = (int)health;
			else DebugUIManager.instance?.LogError("No health found for " + id, "SentryInit");

			if (info.TryGetModi(CSVMd.SentryMaxAmmo, level, out double ammo)) maxAmmo = (int)ammo;
			else DebugUIManager.instance?.LogError("No ammo found for " + id, "SentryInit");

			if (info.TryGetModi(CSVMd.SentryAmmoRegen, level, out double regen)) ammoRegen = (float)regen;
			else DebugUIManager.instance?.LogError("No regen found for " + id, "SentryInit");

			if (info.TryGetModi(CSVMd.SentryShootSpeed, level, out double fireRate)) shootSpeed = (float)fireRate;
			else DebugUIManager.instance?.LogError("No fire rate found for " + id, "SentryInit");

			if (info.TryGetModi(CSVMd.SentryShootSpread, level, out double spread)) shootSpread = (float)spread;
			else DebugUIManager.instance?.LogError("No spread found for " + id, "SentryInit");

			if (info.TryGetModi(CSVMd.SentryDamage, level, out double damage)) dmgModi = (float)damage;
			else DebugUIManager.instance?.LogError("No damage found for " + id, "SentryInit");

			remainingCooldownTime = cooldownPeriod;
		}

		public void Activate(NetworkedEntity entity, bool isOverride = false) { //reset the timer and activate ability.
			if (!isOverride && remainingCooldownTime != 0) return;
			remainingCooldownTime = cooldownPeriod;
			NetworkedEntity sentry = entity.SpawnSentry();

			sentry.SetSentryStats(entity.GetTeam(), maxHealth, maxAmmo, ammoRegen, shootSpeed, shootSpread, dmgModi, isFullAuto);

			GameObject sentryEffect = sentry.InitEffect(1, 0f, CSVId.SentryActive); //Effect
			if (sentryEffect == null) return;
			if (sentryEffect.TryGetComponent(out Effect e)) {
				e.EnableDestroy(1f);
			}
		}

		/// >>>>>>>>>>>>>>>>>>>> TODO: BECAUSE ALL ABILITIES HAVE ONLY TWO MODES OF COOLDOWN --
		/// DOWNWARDS-CONSUMPTION OR INSTANT USE -- CHANGE THIS SO THAT IT IS HANDLED CENTRALLY AND ONLY
		/// ONE BOOLEAN/ENUM NEEDS TO BE CALLED
		public void SysTickCall(NetworkedEntity entity) {
			remainingCooldownTime = Mathf.Max(0, remainingCooldownTime - Time.deltaTime);
		}

		public bool TryPushUpgrade(string id, InGameUpgradeInfo info) {
			if (id == nameof(CSVId.SentryActive) + "ReinforcedSentry") {
				if (info.TryGetModi(CSVMd.SentryHealth, out double hp)) maxHealth += (int)hp;
				else DebugUIManager.instance?.LogError("No cooldown found for " + id, nameof(CSVId.SentryActive) + "Reinforced Sentry");
				
			} else if (id == nameof(CSVId.SentryActive) + "LargerCaliber") {
				if (info.TryGetModi(CSVMd.SentryDamage, out double damage)) dmgModi += (float)damage;
				else DebugUIManager.instance?.LogError("No damage found for " + id, nameof(CSVId.SentryActive) + "Larger Caliber");

			} else return false;
			return true;
		}
	}
}
