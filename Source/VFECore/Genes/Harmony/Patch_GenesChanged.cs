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
    /// Update the cache when genes change.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_GeneTracker), "Notify_GenesChanged")]
    public static class Notify_GenesChanged_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(GeneDef addedOrRemovedGene, Pawn_GeneTracker __instance)
        {
            if (__instance?.pawn != null)
            {
                PawnDataCache.GetPawnDataCache(__instance.pawn, forceRefresh:true);
            }
        }
    }

    /// <summary>
    /// Update the cache when the pawn is finished being initialized.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostMapInit))]
    public static class Pawn_PostMapInit_Patch
    {
        public static void Postfix(Pawn __instance)
        {
            if (__instance != null)
            {
                PawnDataCache.GetPawnDataCache(__instance, forceRefresh: true);
            }
        }
    }
}