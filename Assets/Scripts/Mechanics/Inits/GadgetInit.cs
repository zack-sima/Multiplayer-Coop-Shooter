using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSV.Parsers;
using Abilities;
using ExitGames.Client.Photon.StructWrapping;

public static class GadgetInitExtensions {
    public static void InitGadgets(this Dictionary<string, int> gadgets, NetworkedEntity entity) {
        Dictionary<string, InventoryInfo> infos = PlayerDataHandler.instance.GetGadgetInfos();

        if (infos == null || infos.Count == 0) {
            DebugUIManager.instance?.LogError("No gadgets found in inventory.", "GadgetInit");
            return;
        }

        foreach(string g in gadgets.Keys) {
            if (!infos.ContainsKey(g)) {
                DebugUIManager.instance?.LogError("No gadget info found for " + g, "GadgetInit");
                continue;
            }
            //Populate switch statement with gadget types just like in ActiveInitExtensions.cs
            //HardenedAmmoGadget, ImprovedLoaderGadget, HardenedArmorGadget, FireControlGadget, PolishedTriggerGadget, LaserSightGadget, BracedInternalsGadget
            switch(g) {
                //*?=======================| PASSIVE GADGETS |=======================?*//

                case nameof(CSVId.HardenedAmmoGadget): {
                    if (infos[g].TryGetModi(CSVMd.Damage, gadgets[g], out double dmg) && TryGetTurret(out Turret turret)) {
                        turret.SetBulletDmgModi(dmg += turret.GetBulletModi());
                    }
                    break; }
                case nameof(CSVId.ImprovedLoaderGadget): {
                    if (infos[g].TryGetModi(nameof(CSVMd.Reload), gadgets[g], out double reload) && infos[g].TryGetModi(nameof(CSVMd.AmmoRegen),gadgets[g], out double ammoRegen) && TryGetTurret(out Turret turret)) {
                        turret.SetShootSpeed(reload * turret.GetBaseShootSpeed() + turret.GetShootSpeed());
                        turret.SetAmmoRegenRate(ammoRegen * turret.GetBaseAmmoRegenRate() + turret.GetAmmoRegenSpeed());
                    }
                    break; }
                case nameof(CSVId.HardenedArmorGadget): {
                    if (infos[g].TryGetModi(nameof(CSVMd.MaxHP), gadgets[g], out double hp) && TryGetCombatEntity(out CombatEntity c)) {
                        c.SetMaxHealth((float)hp * c.GetBaseHealth());
                    }
                    break; }
                case nameof(CSVId.FireControlGadget): {
                    if (infos[g].TryGetModi(nameof(CSVMd.Reload), gadgets[g], out double reload) && TryGetTurret(out Turret turret)) {
                        turret.SetShootSpeed(reload * turret.GetBaseShootSpeed() + turret.GetShootSpeed());
                    }
                    break; }
                case nameof(CSVId.PolishedTriggerGadget): {
                    if (infos[g].TryGetModi(nameof(CSVMd.CritChance), gadgets[g], out double crit) && TryGetTurret(out Turret turret)) {
                        turret.SetCritChance((float)crit + turret.GetCritValues().Item1);
                    }
                    break; }
                case nameof(CSVId.LaserSightGadget): {
                    if (infos[g].TryGetModi(nameof(CSVMd.CritDamage), gadgets[g], out double critDmg) && TryGetTurret(out Turret turret)) {
                        turret.SetCritDamage((float)critDmg + turret.GetCritValues().Item2);
                    }
                    break; }
                case nameof(CSVId.BracedInternalsGadget): {
                    if (infos[g].TryGetModi(nameof(CSVMd.MaxHP), gadgets[g], out double hp) && TryGetCombatEntity(out CombatEntity c)) {
                        c.SetMaxHealth((float)hp * c.GetBaseHealth());
                    }
                    break; }
                     
                //TODO: Populate with more gadgets here ...
            }
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
