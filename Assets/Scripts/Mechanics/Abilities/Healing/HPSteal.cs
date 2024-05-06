using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effects;

namespace Abilities {

	class HPSteal : IActivatable, ISysTickable, IButtonRechargable {
		public float cooldownPeriod, stealAmount, stealPeriod, stealRadius, remainingCooldownTime, totalHPStolen = 0;
		private float remainingHealTime = 0;
		private bool isActive = false;
		private UnityEngine.UI.Image outline = null;
		private NetworkedEntity entity;

		public HPSteal() { this.UpdateAbility(); }

		public void Activate(NetworkedEntity entity, bool isOverride = false) { //reset the timer and activate ability.
			if (!isOverride && (isActive || remainingCooldownTime != 0)) return;
			remainingCooldownTime = cooldownPeriod;
			remainingHealTime = stealPeriod;
			isActive = true;

			totalHPStolen = 0;
			this.entity = entity;

			//Effect
			GameObject healEffect = entity.InitEffect(entity.GetEffect(UpgradeIndex.HPSteal), stealPeriod + 2f, 5f, UpgradeIndex.HPSteal);
			if (healEffect.TryGetComponent(out Effect effect)) {
				effect.EnableDestroy(stealPeriod);
				effect.EnableEarlyDestruct(5f);
			}
			if (healEffect.TryGetComponent(out ParticleSystem p)) {
				//For Particle effects.
			}
		}

		public bool GetIsActive() { return isActive; }

		public float GetCooldownPercentage() {
			return (cooldownPeriod - remainingCooldownTime) / cooldownPeriod;
		}

		private int tickAmount;
		private float totalDelta;

		public void SysTickCall() {
			if (isActive) {
				remainingHealTime = Mathf.Max(0, remainingHealTime - Time.deltaTime);
				if (remainingHealTime == 0) isActive = false;
				if (outline != null) { // Show that the ability is currently active + cooldown bar for that.
					outline.fillAmount = remainingHealTime / stealPeriod;
				}
				//Effect all enemies around u
				if (tickAmount > 10) {
					foreach (CombatEntity e in new List<CombatEntity>(EntityController.instance.GetCombatEntities())) {
						if (e.GetNetworker() == null) continue;
						if (e.GetNetworker() == entity) continue;
						if (e.GetNetworker().GetTeam() == entity.GetTeam()) continue;
						if (Vector3.Distance(e.GetNetworker().transform.position, entity.transform.position) > stealRadius) continue;
						totalHPStolen += stealAmount / stealPeriod;
						e.GetNetworker().RPC_TakeDamage(e.GetNetworker().Object, stealAmount * totalDelta / stealPeriod, 0);
					}
					tickAmount = 0;
					totalDelta = 0;
				} else {
					tickAmount++;
					totalDelta += Time.deltaTime;
				}

			} else {
				remainingCooldownTime = Mathf.Max(0, remainingCooldownTime - Time.deltaTime);
				if (outline != null) { // update the outline.
					outline.fillAmount = (cooldownPeriod - remainingCooldownTime) / cooldownPeriod;
				}
			}
		}

		public void SetButtonOutlineProgressImage(UnityEngine.UI.Image outlineProgress) {
			outline = outlineProgress;
		}
	}

}
