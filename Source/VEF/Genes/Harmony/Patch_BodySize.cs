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
        public static void Postfix(ref float __result, Pawn __instance)
        {
            if (PawnDataCache.GetCacheUltraSpeed(__instance) is CachedPawnData data)
            {
                __result += data.bodySizeOffset;

                if (__result < 0.05f)
                {
                    __result = 0.05f;
                }
            }
        }
    }
}