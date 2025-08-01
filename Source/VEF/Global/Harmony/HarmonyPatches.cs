﻿using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using VEF.Factions;
using VEF.Apparels;
using VEF.Hediffs;

namespace VEF
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            //Harmony.DEBUG = true;
            // PawnApparelGenerator.PossibleApparelSet.CoatButNoShirt
            VEF_Mod.harmonyInstance.Patch(
                typeof(PawnApparelGenerator).GetNestedType("PossibleApparelSet", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetMethod("CoatButNoShirt", BindingFlags.Public | BindingFlags.Instance),
                transpiler: new HarmonyMethod(typeof(Patch_PawnApparelGenerator.PossibleApparelSet.manual_CoatButNoShirt), "Transpiler"));

            VEF_Mod.harmonyInstance.Patch(AccessTools.Method(typeof(PawnApparelGenerator), nameof(PawnApparelGenerator.GenerateStartingApparelFor)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(Patch_PawnApparelGenerator), nameof(Patch_PawnApparelGenerator.GenerateStartingApparelFor_Postfix))));

            PhasingPatches.Do(VEF_Mod.harmonyInstance);

            // Dual Wield
            if (ModCompatibilityCheck.DualWield)
            {
                var addHumanlikeOrdersPatch = GenTypes.GetTypeInAnyAssembly("DualWield.HarmonyInstance.FloatMenuMakerMap_AddHumanlikeOrders");
                if (addHumanlikeOrdersPatch != null)
                    VEF_Mod.harmonyInstance.Patch(AccessTools.Method(addHumanlikeOrdersPatch, "Postfix"),
                        transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_DualWield_Harmony_FloatMenuMakerMap_AddHumanlikeOrders.manual_Postfix), "Transpiler"));
                else
                    Log.Error("Could not find type DualWield.HarmonyInstance.FloatMenuMakerMap_AddHumanlikeOrders in Dual Wield");

                // Taranchuk: no idea how to handle this
                //var extEquipmentTracker = GenTypes.GetTypeInAnyAssembly("DualWield.Ext_Pawn_EquipmentTracker", "DualWield");
                //if (extEquipmentTracker != null)
                //    VEF.harmonyInstance.Patch(AccessTools.Method(extEquipmentTracker, "MakeRoomForOffHand"),
                //        postfix: new HarmonyMethod(typeof(Patch_DualWield_Ext_Pawn_EquipmentTracker.manual_MakeRoomForOffHand), "Postfix"));
                //else
                //    Log.Error("Could not find type DualWield.Ext_Pawn_EquipmentTracker in Dual Wield");
            }

            // RimCities
            if (ModCompatibilityCheck.RimCities)
            {
                var genCity = GenTypes.GetTypeInAnyAssembly("Cities.GenCity", "Cities");
                if (genCity != null)
                    VEF_Mod.harmonyInstance.Patch(
                        genCity.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance)
                            .First(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Any(m => m.Name.Contains("RandomCityFaction") && m.ReturnType == typeof(bool)))
                            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(m => m.Name.Contains("RandomCityFaction")),
                        postfix: new HarmonyMethod(typeof(Patch_Cities_GenCity.manual_RandomCityFaction_predicate), "Postfix"));
                else
                    Log.Error("Could not find type Cities.GenCity in RimCities");
            }

            // RPG Style Inventory
            if (ModCompatibilityCheck.RPGStyleInventory)
            {
                var detailedRPGGearTab = GenTypes.GetTypeInAnyAssembly("Sandy_Detailed_RPG_Inventory.Sandy_Detailed_RPG_GearTab", "Sandy_Detailed_RPG_Inventory");
                if (detailedRPGGearTab != null)
                {
                    Patch_RPG_GearTab.DetailedRPGGearTab = detailedRPGGearTab;
                    VEF_Mod.harmonyInstance.Patch(AccessTools.Method(detailedRPGGearTab, "TryDrawOverallArmor"),
                        transpiler: new HarmonyMethod(typeof(Patch_RPG_GearTab), "TryDrawOverallArmor_Transpiler"));
                    VEF_Mod.harmonyInstance.Patch(AccessTools.Method(detailedRPGGearTab, "TryDrawOverallArmor1"),
                        transpiler: new HarmonyMethod(typeof(Patch_RPG_GearTab), "TryDrawOverallArmor1_Transpiler"));
                }
                else
                {
                    Log.Error("Could not find type Sandy_Detailed_RPG_Inventory.Sandy_Detailed_RPG_GearTab in RPG Style Inventory");
                }
            }

            // RPG Style Inventory Revamped
            if (ModCompatibilityCheck.RPGStyleInventoryRevamped)
            {
                var detailedRPGGearTab = GenTypes.GetTypeInAnyAssembly("Sandy_Detailed_RPG_Inventory.Sandy_Detailed_RPG_GearTab", "Sandy_Detailed_RPG_Inventory");
                if (detailedRPGGearTab != null)
                {
                    Patch_RPG_GearTab.DetailedRPGGearTabRevamped = detailedRPGGearTab;
                    VEF_Mod.harmonyInstance.Patch(AccessTools.Method(detailedRPGGearTab, "TryDrawOverallArmor"),
                        transpiler: new HarmonyMethod(typeof(Patch_RPG_GearTab), "TryDrawOverallArmor_Revamped_Transpiler"));
                    VEF_Mod.harmonyInstance.Patch(AccessTools.Method(detailedRPGGearTab, "TryDrawOverallArmor1"),
                        transpiler: new HarmonyMethod(typeof(Patch_RPG_GearTab), "TryDrawOverallArmor1_Revamped_Transpiler"));
                }
                else
                {
                    Log.Error("Could not find type Sandy_Detailed_RPG_Inventory.Sandy_Detailed_RPG_GearTab in RPG Style Inventory");
                }
            }

            // Compatibility with Run and Gun
            if (ModCompatibilityCheck.RunAndGun)
            {
                var patchVerbTryCastNextBurstShot = GenTypes.GetTypeInAnyAssembly("RunAndGun.Harmony.Verb_TryCastNextBurstShot", "RunAndGun.Harmony");
                if (patchVerbTryCastNextBurstShot != null)
                    VEF_Mod.harmonyInstance.Patch(AccessTools.Method(patchVerbTryCastNextBurstShot, "SetStanceRunAndGun"),
                        transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_RunAndGun_Harmony_Verb_TryCastNextBurstShot.manual_SetStanceRunAndGun), "Transpiler"));
                else
                    Log.Error("Could not find type RunAndGun.Harmony.Verb_TryCastNextBurstShot in RunAndGun");
            }

            // Disable Faction Discovery
            if (ModCompatibilityCheck.FactionDiscovery)
            {
                var patchModBase = GenTypes.GetTypeInAnyAssembly("FactionDiscovery.ModBase", "FactionDiscovery");
                if (patchModBase != null)
                    VEF_Mod.harmonyInstance.Patch(AccessTools.Method(patchModBase, "RunCheck"),
                        new HarmonyMethod(typeof(VanillaExpandedFramework_FactionDiscovery_ModBase_Patch.manual_RunCheck), "Prefix"));
                else
                    Log.Error("Could not find type RunAndGun.Harmony.Verb_TryCastNextBurstShot in RunAndGun");
            }
        }
    }
}