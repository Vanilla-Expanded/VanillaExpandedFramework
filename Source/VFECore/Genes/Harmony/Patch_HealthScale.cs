using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFECore;

namespace VanillaGenesExpanded
{
    /// <summary>
    /// This patch gets called an ungodly number of times. Absolutely anything here is expensive as heck.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "HealthScale", MethodType.Getter)]
    public static class Pawn_HealthScale
    {
        [HarmonyPostfix]
        public static void HealthScale_Postfix(ref float __result, Pawn __instance)
        {
            if(PawnDataCache.GetPawnDataCache(__instance) is CachedPawnData data)
            {
                __result *= data.healthMultiplier;
            }
        }
    }
}
