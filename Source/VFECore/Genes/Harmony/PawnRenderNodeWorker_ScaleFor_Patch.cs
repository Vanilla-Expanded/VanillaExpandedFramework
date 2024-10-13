using System.Linq;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse.Noise;
using VFECore;
using System;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(PawnRenderNodeWorker), nameof(PawnRenderNodeWorker.ScaleFor))]
    public static class PawnRenderNodeWorker_ScaleFor_Patch
    {
        public struct PerThreadMiniCache
        {
            public Pawn pawn;
            public CachedPawnData cache;
        }
        [ThreadStatic]
        static PerThreadMiniCache threadStaticCache;
        public static void Postfix(ref Vector3 __result, PawnRenderNode node, PawnDrawParms parms)
        {
            var pawn = parms.pawn;
            if (pawn == null) return;
            if (threadStaticCache.pawn != pawn)
            {
                threadStaticCache.cache = PawnDataCache.GetPawnDataCache(pawn, canRefresh: false);
                threadStaticCache.pawn = pawn;
            }
            var cache = threadStaticCache.cache;
            if (threadStaticCache.cache.isHumanlike)
            {
                if (node is PawnRenderNode_Body)
                {
                    __result = new Vector3(cache.vCosmeticScale.x * __result.x, __result.y, cache.vCosmeticScale.z * __result.z);
                }
                else if (node is PawnRenderNode_Head)
                {
                    __result *= cache.headRenderSize;
                }
            }
            else
            {
                __result = new Vector3(cache.vCosmeticScale.x * __result.x, __result.y, cache.vCosmeticScale.z * __result.z);
            }
        }
    }
}
