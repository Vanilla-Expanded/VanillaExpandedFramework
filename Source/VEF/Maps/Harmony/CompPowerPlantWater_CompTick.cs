using HarmonyLib;
using UnityEngine;
using Verse;
using VEF.Things;
using RimWorld;

namespace VEF.Maps
{
    [HarmonyPatch(typeof(CompPowerPlantWater), "CompTick")]
    public static class VanillaExpandedFramework_CompPowerPlantWater_CompTick_Patch
    {

        public static float cachedGameConditionMultiplier = 1;

        [HarmonyPostfix]
        public static void PostFix(CompPowerPlantWater __instance)
        {
            if (__instance.parent.IsHashIntervalTick(2000))
            {
                cachedGameConditionMultiplier = 1;
                if (__instance.parent.Map != null)
                {
                    if (__instance.parent.Map.gameConditionManager.ActiveConditions.Count > 0)
                    {
                        foreach (GameCondition condition in __instance.parent.Map.gameConditionManager.ActiveConditions)
                        {
                            MapConditionExtension extension = condition.def.GetModExtension<MapConditionExtension>();
                            if (extension != null)
                            {
                                cachedGameConditionMultiplier *= extension.watermillStrengthMultiplier;
                            }
                        }
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(CompPowerPlantWater), "DesiredPowerOutput", MethodType.Getter)]
    public static class VanillaExpandedFramework_CompPowerPlantWater_DesiredPowerOutput_Patch
    {

        [HarmonyPostfix]
        public static void PostFix(ref float __result)
        {
            __result *= VanillaExpandedFramework_CompPowerPlantWater_CompTick_Patch.cachedGameConditionMultiplier;
        }
    }
}
