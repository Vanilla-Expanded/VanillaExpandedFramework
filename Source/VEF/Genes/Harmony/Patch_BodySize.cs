using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VEF;
using VEF.AestheticScaling;

namespace VEF.Genes
{
    
    
    [HarmonyPatch(typeof(Pawn), "BodySize", MethodType.Getter)]
    public static class VanillaExpandedFramework_Pawn_BodySize
    {
        public struct BodySizeCache
        {
            public Pawn pawn;
            public CachedPawnData cache;
            public uint tick;  // Ensure it does eventually update even if there is only a single pawn.
        }
        static BodySizeCache sizeCache;
        public static void Postfix(ref float __result, Pawn __instance)
        {
            CachedPawnData cache;
            if (sizeCache.pawn != __instance || sizeCache.tick != CachedPawnDataSlowUpdate.Tick10)
            {
                sizeCache.cache = __instance.GetCachePrePatched();
                sizeCache.pawn = __instance;
                sizeCache.tick = CachedPawnDataSlowUpdate.Tick10;
                cache = sizeCache.cache;
            }
            else
            {
                cache = sizeCache.cache;
            }

            if (cache != null)
            {
                __result += cache.bodySizeOffset;
                if (__result < 0.05f)
                {
                    __result = 0.05f;
                }
            }
        }
    }
}