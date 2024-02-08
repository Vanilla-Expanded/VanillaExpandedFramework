using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFECore;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(Need_Food), "MaxLevel", MethodType.Getter)]
    public static class Patch_FoodCapacity
    {
        [HarmonyPostfix]
        public static void FoodCapacity_Postfix(ref float __result, ref Need_Food __instance, ref Pawn ___pawn, ref float ___curLevelInt)
        {
            if (ScaleCache.GetScaleCache(___pawn) is SizeData data)
            {
                //float hungerCapStat = ___pawn.GetStatValue(StatDef.Named("VEF_FoodCapacityMultiplier"), cacheStaleAfterTicks: 100);

                __result *= data.percentChange * HungerCache.GetFoodCapMult(___pawn);
            }
        }

        /// <summary>
        /// Cache for the food capacity multiplier. Not sure if this is needed, but StatDef looksup are a bit slow, and
        /// I'm betting this will be called an awful lot since countless things check food-related stuff.
        /// </summary>
        public class HungerData : ICacheable
        {
            public CacheTimer Timer { get; set; } = new CacheTimer();
            public float hunger;
            readonly Pawn pawn;
            public HungerData(Pawn pawn) { this.pawn = pawn; }
            public void RegenerateCache() { hunger = pawn.GetStatValue(StatDef.Named("VEF_FoodCapacityMultiplier")); }
        }

        public class HungerCache : DictCache<Pawn, HungerData>
        {
            public static float GetFoodCapMult(Pawn pawn)
            {
                if (Scribe.mode != LoadSaveMode.Inactive || pawn == null || !pawn.RaceProps.Humanlike)
                    return 1;

                return GetCache(pawn).hunger;
            }
        }
    }
}
