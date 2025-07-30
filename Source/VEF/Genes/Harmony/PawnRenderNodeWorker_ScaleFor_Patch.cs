using System.Linq;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse.Noise;
using VEF;
using System;
using VEF.AestheticScaling;

namespace VEF.Genes
{
    [HarmonyPatch(typeof(PawnRenderNodeWorker), nameof(PawnRenderNodeWorker.ScaleFor))]
    public static class VanillaExpandedFramework_PawnRenderNodeWorker_ScaleFor_Patch
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
            CachedPawnData cache;
            if (CachedPawnDataExtensions.prepatched)
            {
                cache = pawn.GetCachePrePatched();
            }
            else
            {
                if (threadStaticCache.pawn != pawn)
                {
                    threadStaticCache.cache = pawn.GetCachePrePatched();
                    threadStaticCache.pawn = pawn;
                }
                cache = threadStaticCache.cache;
            }

            // Tiny performance win because Unity Casts all float multiplications to double.
            double bodyRenderSizeX = cache.vCosmeticScale.x;
            double bodyRenderSizeZ = cache.vCosmeticScale.z;
            double resultX = __result.x;
            double resultZ = __result.z;
            if (cache.isHumanlike)
            {
                if (node is PawnRenderNode_Body)
                {
                    __result.x = (float)(resultX * bodyRenderSizeX);
                    __result.z = (float)(resultZ * bodyRenderSizeZ);
                }
                else if (node is PawnRenderNode_Head)
                {
                    double headRenderSizeD = cache.headRenderSize;
                    __result.x = (float)(resultX * headRenderSizeD);
                    __result.z = (float)(resultZ * headRenderSizeD);
                }
            }
            else
            {
                __result.x = (float)(resultX * bodyRenderSizeX);
                __result.z = (float)(resultZ * bodyRenderSizeZ);
            }
        }
    }
}
