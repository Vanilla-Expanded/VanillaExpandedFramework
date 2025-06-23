using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace VEF.AestheticScaling
{
    [HarmonyPatch(typeof(Pawn_DrawTracker), nameof(Pawn_DrawTracker.DrawPos), MethodType.Getter)]
    public static class VanillaExpandedFramework_Pawn_DrawTracker_DrawPos_Patch
    {
        public struct PGPRRCache // Special little cache to save us from running GetPosture().
        {
            public Pawn pawn;
            public CachedPawnData cache;
            public bool doOffset;
        }
        [ThreadStatic]
        static PGPRRCache threadStaticCache;

        public static bool skipOffset = false;
        [HarmonyPostfix]
        public static void Postfix(ref Vector3 __result, Pawn ___pawn)
        {
            if (___pawn == null || skipOffset) return;

            if (threadStaticCache.pawn != ___pawn)
            {
                threadStaticCache.cache = ___pawn.GetCachePrePatched();
                threadStaticCache.pawn = ___pawn;
                threadStaticCache.doOffset = ___pawn.GetPosture() == PawnPosture.Standing;
            }

            if (threadStaticCache.cache != null)
            {
                if (threadStaticCache.doOffset)
                {
                    __result.z += threadStaticCache.cache.renderPosOffset;
                }
            }
        }
    }

  
}
