using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using System;

namespace VEF.Plants
{

    [HarmonyPatch(typeof(JobDriver_Replant))]
    [HarmonyPatch("Duration", MethodType.Getter)]
    public static class VanillaExpandedFramework_JobDriver_Replant_Duration_Patch
    {
        [HarmonyPrefix]
        public static bool AvoidError(JobDriver_Replant __instance, ref int __result)
        {
            if(__instance.ThingToCarry is MinifiedFlower)
            {
                __result = 200;
                return false;
            }
            return true;


        }


    }


}











