using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFECore
{

    public static class Patch_FactionGenerator
    {

        [HarmonyPatch(typeof(FactionGenerator), nameof(FactionGenerator.NewGeneratedFaction), new Type[] { typeof(FactionDef) })]
        public static class NewGeneratedFaction
        {

            [HarmonyBefore("net.rainbeau.rimworld.mod.realisticplanets")]
            public static void Prefix(ref FactionDef facDef)
            {
                if (!facDef.isPlayer && !facDef.hidden && !CustomStorytellerUtility.TechLevelAllowed(facDef.techLevel))
                {
                    //while (facDef.isPlayer || facDef.hidden || !CustomStorytellerUtility.TechLevelAllowed(facDef.techLevel))
                    //    facDef = DefDatabase<FactionDef>.GetRandom();
                    facDef = DefDatabase<FactionDef>.AllDefsListForReading.RandomElementByWeight(f => FactionGenerationWeight(f));
                }
            }

            private static float FactionGenerationWeight(FactionDef facDef)
            {
                if (facDef.isPlayer || facDef.hidden || !CustomStorytellerUtility.TechLevelAllowed(facDef.techLevel))
                    return 0;

                var factionCount = Find.FactionManager.AllFactionsVisible.Count(f => f.def == facDef);
                return 1f / factionCount;
            }

        }

    }

}
