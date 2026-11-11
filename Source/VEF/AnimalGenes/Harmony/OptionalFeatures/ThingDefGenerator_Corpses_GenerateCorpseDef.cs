using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using VEF.AnimalGenes;
using Verse;

namespace VEF.AnimalGenes
{
    // This one is not in OptionalFeatures or it won't work

    [HarmonyPatch(typeof(ThingDefGenerator_Corpses), "GenerateCorpseDef")]
    public static class VEF_AnimalGenes_ThingDefGenerator_Corpses_GenerateCorpseDef_Patch
    {
        [HarmonyPostfix]
        public static void HandleCorpses(ThingDef pawnDef, ThingDef __result)
        {
            if (pawnDef.race?.Animal != true)
                return;

            if (!__result.inspectorTabs.Contains(typeof(ITab_AnimalGenes)))
            {
                __result.inspectorTabs.Add(typeof(ITab_AnimalGenes));
            }
        }
    }


}