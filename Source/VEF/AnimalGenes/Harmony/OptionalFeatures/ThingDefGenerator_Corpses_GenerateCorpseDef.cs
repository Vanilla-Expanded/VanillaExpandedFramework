using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using VEF.AnimalGenes;
using Verse;

namespace VEF.AnimalGenes
{
    // This Harmony patch will only be patched if AnimalGenesMechanics is added via XML to a mod using OptionalFeatures


    public static class VEF_AnimalGenes_ThingDefGenerator_Corpses_GenerateCorpseDef_Patch
    {
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