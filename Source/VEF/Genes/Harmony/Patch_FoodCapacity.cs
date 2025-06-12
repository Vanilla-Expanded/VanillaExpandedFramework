using HarmonyLib;
using RimWorld;
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
    [HarmonyPatch(typeof(Need_Food), "MaxLevel", MethodType.Getter)]
    public static class VanillaExpandedFramework_Patch_FoodCapacity
    {
        [HarmonyPostfix]
        public static void FoodCapacity_Postfix(ref float __result, ref Need_Food __instance, ref Pawn ___pawn, ref float ___curLevelInt)
        {
            if (PawnDataCache.GetPawnDataCache(___pawn) is CachedPawnData data)
            {
                __result *= data.percentChange * data.foodCapacityMult;
            }
        }
    }
}
