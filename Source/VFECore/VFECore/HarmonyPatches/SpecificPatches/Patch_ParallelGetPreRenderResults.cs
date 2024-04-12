using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace VFECore
{
    [HarmonyPatch(typeof(PawnRenderer), "ParallelGetPreRenderResults")]
    public static class ParallelGetPreRenderResults_Patch
    {
        public static void Prefix(PawnRenderer __instance, ref Vector3 drawLoc, Rot4 rotOverride, bool neverAimWeapon, ref bool disableCache, Pawn ___pawn)
        {
            if (___pawn == null || ___pawn?.Spawned == false) return;

            // If caching disabled...
            if (!disableCache)
            {
                // For the sake of testing:
                disableCache = VFEGlobal.settings.disableCaching;
                if (PawnDataCache.GetPawnDataCache(___pawn) is CachedPawnData data)
                {
                    if (data.bodySizeOffset > 0 || data.percentChange > 1 || data.renderCacheOff)
                    {
                        disableCache = true;
                    }
                }
            }

        }
    }
}