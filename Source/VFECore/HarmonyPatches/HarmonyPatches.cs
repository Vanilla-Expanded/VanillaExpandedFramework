using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFECore
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            //HarmonyInstance.DEBUG = true;
            VFECore.harmonyInstance.PatchAll();

            // PawnApparelGenerator.PossibleApparelSet.CoatButNoShirt
            VFECore.harmonyInstance.Patch(typeof(PawnApparelGenerator).GetNestedType("PossibleApparelSet", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("CoatButNoShirt", BindingFlags.Public | BindingFlags.Instance),
                transpiler: new HarmonyMethod(typeof(Patch_PawnApparelGenerator.PossibleApparelSet.manual_CoatButNoShirt), "Transpiler"));

            // Dual Wield
            if (ModCompatibilityCheck.DualWield)
            {
                var addHumanlikeOrdersPatch = GenTypes.GetTypeInAnyAssemblyNew("DualWield.Harmony.FloatMenuMakerMap_AddHumanlikeOrders", "DualWield.Harmony");
                if (addHumanlikeOrdersPatch != null)
                    VFECore.harmonyInstance.Patch(AccessTools.Method(addHumanlikeOrdersPatch, "Postfix"),
                        transpiler: new HarmonyMethod(typeof(Patch_DualWield_Harmony_FloatMenuMakerMap_AddHumanlikeOrders.manual_Postfix), "Transpiler"));
                else
                    Log.Error("Could not find type DualWield.Harmony.FloatMenuMakerMap_AddHumanlikeOrders in Dual Wield");

                var extEquipmentTracker = GenTypes.GetTypeInAnyAssemblyNew("DualWield.Ext_Pawn_EquipmentTracker", "DualWield");
                if (extEquipmentTracker != null)
                    VFECore.harmonyInstance.Patch(AccessTools.Method(extEquipmentTracker, "MakeRoomForOffHand"),
                        postfix: new HarmonyMethod(typeof(Patch_DualWield_Ext_Pawn_EquipmentTracker.manual_MakeRoomForOffHand), "Postfix"));
                else
                    Log.Error("Could not find type DualWield.Ext_Pawn_EquipmentTracker in Dual Wield");
            }

            // Facial Stuff
            if (ModCompatibilityCheck.FacialStuff)
            {
                var humanBipedDrawer = GenTypes.GetTypeInAnyAssemblyNew("FacialStuff.HumanBipedDrawer", "FacialStuff");
                if (humanBipedDrawer != null)
                    VFECore.harmonyInstance.Patch(AccessTools.Method(humanBipedDrawer, "DrawApparel"), transpiler: new HarmonyMethod(typeof(Patch_PawnRenderer.RenderPawnInternal), "Transpiler"));
                else
                    Log.Error("Could not find type FacialStuff.HumanBipedDrawer in Facial Stuff");
            }

            // Research Tree
            if (ModCompatibilityCheck.ResearchTree)
            {
                var researchNode = GenTypes.GetTypeInAnyAssemblyNew("FluffyResearchTree.ResearchNode", "FluffyResearchTree");
                if (researchNode != null)
                {
                    Patch_FluffyResearchTree_ResearchNode.instanceType = researchNode;
                    VFECore.harmonyInstance.Patch(AccessTools.Property(researchNode, "Available").GetGetMethod(), postfix: new HarmonyMethod(typeof(Patch_FluffyResearchTree_ResearchNode.manual_get_Available), "Postfix"));
                    VFECore.harmonyInstance.Patch(AccessTools.Method(researchNode, "Draw"), transpiler: new HarmonyMethod(typeof(Patch_FluffyResearchTree_ResearchNode.manual_Draw), "Transpiler"));
                }
                else
                    Log.Error("Could not find type FluffyResearchTree.ResearchNode in Research Tree");
            }

            // ResearchPal (pretty much identical code-wise to research tree as far as we are concerned, other than namespace)
            if (ModCompatibilityCheck.ResearchPal)
            {
                var researchNode = GenTypes.GetTypeInAnyAssemblyNew("ResearchPal.ResearchNode", "ResearchPal");
                if (researchNode != null)
                {
                    Patch_FluffyResearchTree_ResearchNode.instanceType = researchNode;
                    VFECore.harmonyInstance.Patch(AccessTools.Property(researchNode, "Available").GetGetMethod(), postfix: new HarmonyMethod(typeof(Patch_FluffyResearchTree_ResearchNode.manual_get_Available), "Postfix"));
                    VFECore.harmonyInstance.Patch(AccessTools.Method(researchNode, "Draw"), transpiler: new HarmonyMethod(typeof(Patch_FluffyResearchTree_ResearchNode.manual_Draw), "Transpiler"));
                }
                else
                    Log.Error("Could not find type ResearchPal.ResearchNode in ResearchPal");
            }

            // RimCities
            if (ModCompatibilityCheck.RimCities)
            {
                var genCity = GenTypes.GetTypeInAnyAssemblyNew("Cities.GenCity", "Cities");
                if (genCity != null)
                    VFECore.harmonyInstance.Patch(genCity.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).
                        First(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Any(m => m.Name.Contains("RandomCityFaction") && m.ReturnType == typeof(bool))).
                        GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(m => m.Name.Contains("RandomCityFaction")),
                        postfix: new HarmonyMethod(typeof(Patch_Cities_GenCity.manual_RandomCityFaction_predicate), "Postfix"));
                else
                    Log.Error("Could not find type Cities.GenCity in RimCities");

                var worldGenStepCities = GenTypes.GetTypeInAnyAssemblyNew("Cities.WorldGenStep_Cities", "Cities");
                if (worldGenStepCities != null)
                    VFECore.harmonyInstance.Patch(AccessTools.Method(worldGenStepCities, "GenerateFresh"), new HarmonyMethod(typeof(Patch_Cities_WorldGenStep_Cities.manual_GenerateFresh), "Prefix"));
            }

            // RPG Style Inventory
            if (ModCompatibilityCheck.RPGStyleInventory)
            {
                var detailedRPGGearTab = GenTypes.GetTypeInAnyAssemblyNew("Sandy_Detailed_RPG_Inventory.Sandy_Detailed_RPG_GearTab", "Sandy_Detailed_RPG_Inventory");
                if (detailedRPGGearTab != null)
                {
                    Patch_Sandy_Detailed_RPG_GearTab_Sandy_Detailed_RPG_Inventory.manual_TryDrawOverallArmor.DetailedRPGGearTab = detailedRPGGearTab;
                    VFECore.harmonyInstance.Patch(AccessTools.Method(detailedRPGGearTab, "TryDrawOverallArmor"),
                        transpiler: new HarmonyMethod(typeof(Patch_Sandy_Detailed_RPG_GearTab_Sandy_Detailed_RPG_Inventory.manual_TryDrawOverallArmor), "Transpiler"));
                    VFECore.harmonyInstance.Patch(AccessTools.Method(detailedRPGGearTab, "TryDrawOverallArmor1"),
                        transpiler: new HarmonyMethod(typeof(Patch_Sandy_Detailed_RPG_GearTab_Sandy_Detailed_RPG_Inventory.manual_TryDrawOverallArmor1), "Transpiler"));
                }
                    
                else
                    Log.Error("Could not find type Sandy_Detailed_RPG_Inventory.Sandy_Detailed_RPG_GearTab in RPG Style Inventory");
            }

            // Compatibility with Run and Gun
            if (ModCompatibilityCheck.RunAndGun)
            {
                var patchVerbTryCastNextBurstShot = GenTypes.GetTypeInAnyAssemblyNew("RunAndGun.Harmony.Verb_TryCastNextBurstShot", "RunAndGun.Harmony");
                if (patchVerbTryCastNextBurstShot != null)
                    VFECore.harmonyInstance.Patch(AccessTools.Method(patchVerbTryCastNextBurstShot, "SetStanceRunAndGun"), transpiler: new HarmonyMethod(typeof(Patch_RunAndGun_Harmony_Verb_TryCastNextBurstShot.manual_SetStanceRunAndGun), "Transpiler"));
                else
                    Log.Error("Could not find type RunAndGun.Harmony.Verb_TryCastNextBurstShot in RunAndGun");
            }
        }

    }

}
