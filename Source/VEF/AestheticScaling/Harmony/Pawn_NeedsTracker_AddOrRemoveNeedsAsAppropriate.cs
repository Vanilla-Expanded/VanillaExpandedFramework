using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VEF.AestheticScaling
{

    [HarmonyPatch(typeof(Pawn_NeedsTracker), nameof(Pawn_NeedsTracker.AddOrRemoveNeedsAsAppropriate))]
    public static class VanillaExpandedFramework_Pawn_NeedsTracker_AddOrRemoveNeedsAsAppropriate_Patch
    {
        public static void Prefix()
        {
            CachedPawnData.cacheCanBeRecalculated = false;
        }

        public static void Postfix()
        {
            CachedPawnData.cacheCanBeRecalculated = true;
        }
    }

}
