using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSV;
using CSV.Parsers;
using Abilities;

namespace Intializers {

	#region InventoryInit

	public static class InventoryInit {

		public static void UpdateInventory(this (List<(IAbility ability, bool isActivated)> actives,
					Dictionary<string, UpgradesCatalog.UpgradeNode> upgrades) inventory, NetworkedEntity entity) {


			Dictionary<string, Dictionary<string, int>> requested = PlayerDataHandler.instance.GetEquippedInfos();

			if (requested == null || requested.Count == 0) {
				LogError("No items requested to be equipped.", "InventoryHandler");
				return;
			}

			//*=======================| ACTIVES |=======================*//

			if (requested.ContainsKey(nameof(CSVType.ACTIVES))) {
				Dictionary<string, int> activeInfos = requested[nameof(CSVType.ACTIVES)];

				foreach (KeyValuePair<string, int> a in activeInfos) { //active inits
					inventory.actives.InitActive(a.Key, a.Value);
				}

			} else {
				LogWarning("No actives requested to be equipped.", "InventoryHandler");
			}
			UIController.instance.AbilitiesUpdated();

			//*=======================| GADGETS |=======================*//

			if (requested.ContainsKey(nameof(CSVType.GADGETS))) {
				requested[nameof(CSVType.GADGETS)].InitGadgets(entity);
			} else {
				LogWarning("No gadgets requested to be equipped.", "InventoryHandler");
			}

			//turret and hull stat inits.

		}

		private static void LogWarning(string error, string debugId) {
			DebugUIManager.instance?.LogOutput(debugId + ": " + error);
			Debug.LogWarning(debugId + " : " + error);
		}

		private static void LogError(string error, string debugId) {
			DebugUIManager.instance?.LogOutput(debugId + ": " + error);
			Debug.LogError(debugId + " : " + error);
		}
	}

	#endregion

	#region ActiveInit

	public static class AbilityInit {

		public static void InitActive(this List<(IAbility ability, bool isActivated)> actives, string activeId, int level) {
			Dictionary<string, InventoryInfo> infos = PlayerDataHandler.instance.GetActiveInfos();

			if (!infos.ContainsKey(activeId)) {
				DebugUIManager.instance?.LogError("No active info found for " + activeId, "ActiveInit");
				return;
			}

			switch (activeId) {
				//*?=======================| ACTIVE ABILITIES |=======================?*//

				case nameof(CSVId.HealActive): { actives.Add((new Heal(CSVId.HealActive, infos[activeId], level), false)); break; }

				case nameof(CSVId.RapidFireActive): { actives.Add((new RapidFire(CSVId.RapidFireActive, infos[activeId], level), false)); break; }

				case nameof(CSVId.SentryActive): { actives.Add((new Sentry(CSVId.SentryActive, infos[activeId], level), false)); break; }

				//TODO: Add more active abilities here ...

				//*?=======================| ACTIVE GADGETS |=======================?*//
				case nameof(CSVId.RegenerativeArmorGadget): { actives.Add((new RegenerativeArmorGadget(CSVId.RegenerativeArmorGadget, infos[activeId], level), false)); break; }

					//TODO: Add more active gadgets here ...
			}

			infos[activeId].GetInGameUpgrade().InitUpgrades();
		}
	}

	#endregion

	#region GadgetInit

	public static class GadgetInitExtensions {

		public static void InitGadgets(this Dictionary<string, int> gadgets, NetworkedEntity entity) {
			Dictionary<string, InventoryInfo> infos = PlayerDataHandler.instance.GetGadgetInfos();

			if (infos == null || infos.Count == 0) {
				DebugUIManager.instance?.LogError("No gadgets found in inventory.", "GadgetInit");
				return;
			}

			foreach (string g in gadgets.Keys) {
				if (!infos.ContainsKey(g)) {
					DebugUIManager.instance?.LogError("No gadget info found for " + g, "GadgetInit");
					continue;
				}
				//Populate switch statement with gadget types just like in ActiveInitExtensions.cs
				//HardenedAmmoGadget, ImprovedLoaderGadget, HardenedArmorGadget, FireControlGadget, PolishedTriggerGadget, LaserSightGadget, BracedInternalsGadget
				// switch(g) {
				//     //*?=======================| PASSIVE GADGETS |=======================?*//

				//     case nameof(CSVId.HardenedAmmoGadget): {
				//         if (infos[g].TryGetModi(CSVMd.Damage, gadgets[g], out double dmg) && TryGetTurret(out Turret turret)) {
				//             turret.SetBulletDmgModi(dmg += turret.GetBulletModi());
				//         }
				//         break; }
				//     case nameof(CSVId.ImprovedLoaderGadget): {
				//         if (infos[g].TryGetModi(nameof(CSVMd.Reload), gadgets[g], out double reload) && infos[g].TryGetModi(nameof(CSVMd.AmmoRegen),gadgets[g], out double ammoRegen) && TryGetTurret(out Turret turret)) {
				//             turret.SetShootSpeed(reload * turret.GetBaseShootSpeed() + turret.GetShootSpeed());
				//             turret.SetAmmoRegenRate(ammoRegen * turret.GetBaseAmmoRegenRate() + turret.GetAmmoRegenSpeed());
				//         }
				//         break; }
				//     case nameof(CSVId.HardenedArmorGadget): {
				//         if (infos[g].TryGetModi(nameof(CSVMd.MaxHP), gadgets[g], out double hp) && TryGetCombatEntity(out CombatEntity c)) {
				//             c.SetMaxHealth((float)hp * c.GetBaseHealth());
				//         }
				//         break; }
				//     case nameof(CSVId.FireControlGadget): {
				//         if (infos[g].TryGetModi(nameof(CSVMd.Reload), gadgets[g], out double reload) && TryGetTurret(out Turret turret)) {
				//             turret.SetShootSpeed(reload * turret.GetBaseShootSpeed() + turret.GetShootSpeed());
				//         }
				//         break; }
				//     case nameof(CSVId.PolishedTriggerGadget): {
				//         if (infos[g].TryGetModi(nameof(CSVMd.CritChance), gadgets[g], out double crit) && TryGetTurret(out Turret turret)) {
				//             turret.SetCritChance((float)crit + turret.GetCritValues().Item1);
				//         }
				//         break; }
				//     case nameof(CSVId.LaserSightGadget): {
				//         if (infos[g].TryGetModi(nameof(CSVMd.CritDamage), gadgets[g], out double critDmg) && TryGetTurret(out Turret turret)) {
				//             turret.SetCritDamage((float)critDmg + turret.GetCritValues().Item2);
				//         }
				//         break; }
				//     case nameof(CSVId.BracedInternalsGadget): {
				//         if (infos[g].TryGetModi(nameof(CSVMd.MaxHP), gadgets[g], out double hp) && TryGetCombatEntity(out CombatEntity c)) {
				//             c.SetMaxHealth((float)hp * c.GetBaseHealth());
				//         }
				//         break; }

				//     //TODO: Populate with more gadgets here ...
				// }
				infos[g].GetInGameUpgrade().InitUpgrades();
			}
		}

		private static bool TryGetCombatEntity(out CombatEntity c) {
			c = NetworkedEntity.playerInstance.GetCombatEntity();
			if (c == null) return false;
			else return true;
		}

		private static bool TryGetTurret(out Turret t) {
			TryGetCombatEntity(out CombatEntity c);
			if (c == null) { t = null; return false; }
			t = c.GetTurret();
			if (t == null) return false;
			return true;
		}
	}

	#endregion

	#region UpgradeInit

	public static class UpgradeInit {
		public static void InitUpgrades(this List<InGameUpgradeInfo> upgrades) {

			//TODO: Dependency stacking, though idk if its needed ...
			foreach (InGameUpgradeInfo i in upgrades) {
				if (i.TryGetModi(nameof(CSVMd.UPCost), out double upCost) && i.TryGetModi(nameof(CSVMd.Level), out double lvl)) {
					if (lvl <= 1) { // Only one upgrade of it.
						UpgradesCatalog.instance.AddUpgrade(id: i.id, parentId: i.parentId, displayName: i.displayName, cost: (int)upCost, info: i);
						continue;
					} else {
						UpgradesCatalog.UpgradeNode priorNode = UpgradesCatalog.instance.AddUpgrade(id: i.id, parentId: i.parentId, displayName: i.displayName, cost: (int)upCost, info: i, level: 1);
						for (int j = 2; j < lvl + 1; j++) {
							List<string> hards = new() { priorNode.GetUpgradeId() };
							UpgradesCatalog.UpgradeNode currentNode = UpgradesCatalog.instance.AddUpgrade(id: i.id, parentId: i.parentId, displayName: i.displayName, cost: (int)(upCost * j),
									info: i, level: j, hardRequirements: hards);
							currentNode.prior = priorNode;
							priorNode = currentNode;
						}
					}
				} else {
					DebugUIManager.instance?.LogError("Upgrade " + i.id + " has no cost or level.", "UpgradeInit");
				}
			}


		}

		//Old dependency stacking code.
		// foreach(KeyValuePair<string, UpgradeInfo> pair in dict) {
		//     if (pair.Value.TryGetModi("Cost", out float cost) && pair.Value.TryGetModi("Lvls", out float lvl)) {
		//         if (lvl <= 1) { // Only one upgrade of it.
		//             UpgradesCatalog.instance.AddUpgrade(pair.Key, cost: (int)cost, info: pair.Value,
		//                 softRequirements: pair.Value.softRequirements.StackDuplicateDependencies(dict),
		//                 hardRequirements: pair.Value.hardRequirements.StackDuplicateDependencies(dict),
		//                 mutuallyExclusiveUpgrades: pair.Value.mutualRequirements.StackDuplicateDependencies(dict));
		//             continue;
		//         }
		//         UpgradesCatalog.UpgradeNode priorNode = UpgradesCatalog.instance.AddUpgrade(pair.Key, cost: (int)cost,
		//             info: pair.Value,level: 1, 
		//             softRequirements: pair.Value.softRequirements.StackDuplicateDependencies(dict),
		//             hardRequirements: pair.Value.hardRequirements.StackDuplicateDependencies(dict),
		//             mutuallyExclusiveUpgrades: pair.Value.mutualRequirements.StackDuplicateDependencies(dict));
		//         for(int i = 2; i < lvl + 1; i++) {
		//             List<string> hards = pair.Value.hardRequirements.StackDuplicateDependencies(dict);
		//             hards.Add(priorNode.GetUpgradeId());
		//             UpgradesCatalog.UpgradeNode currentNode = UpgradesCatalog.instance.AddUpgrade(
		//                 pair.Key, cost: (int)(cost * i), info: pair.Value, level: i, replacePrior: true,
		//                 softRequirements: pair.Value.softRequirements.StackDuplicateDependencies(dict),
		//                 hardRequirements: hards,
		//                 mutuallyExclusiveUpgrades: pair.Value.mutualRequirements.StackDuplicateDependencies(dict));
		//             currentNode.prior = priorNode;
		//             priorNode = currentNode;
		//         }
		//     } 
		// }

		// private static List<string> StackDuplicateDependencies(this List<string> depens, Dictionary<string, UpgradeInfo> dict) {
		//     List<string> temp = new(depens);
		//     foreach(string s in temp) {
		//         int dupeCount = 0;
		//         for(int i = 0; i < depens.Count; i++) {
		//             if (depens[i] == s) { 
		//                 dupeCount++;
		//                 depens.RemoveAt(i);
		//                 i--;
		//             }
		//         }
		//         if (dict.TryGetValue(s, out UpgradeInfo value) && value.TryGetModi(nameof(ModiName.Lvls), out float level)) {
		//             if ((int)level > 0) { depens.Add(s + " " + UpgradesCatalog.ToRoman(dupeCount)); }
		//         } else depens.Add(s);
		//     }
		//     if (depens.Count <= 0) return new();
		//     return depens;
		// }

	}

	#endregion

	#region DescriptionInit

	public static class DescriptionInitExtensions {

		public static string DescriptionInit(this double desc, string md) {
			switch (md) {
				case nameof(CSVMd.Cooldown):

					return desc.ToString() + "s";

				case nameof(CSVMd.Health):
				case nameof(CSVMd.HealAmount):
				case nameof(CSVMd.CritChance):
				case nameof(CSVMd.Damage):
				case nameof(CSVMd.SentryDamage):
				case nameof(CSVMd.MaxHP):

					return (desc * 100d).ToString() + "%";

				default:
					return desc.ToString();
			}
		}
	}

	#endregion


}