using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using VEF.Maps;
using VEF.Weapons;
using Verse;

namespace VEF.AnimalGenes
{

    public static class OptionalFeatures_AnimalGenesMechanics
    {
        public static void ApplyFeature(Harmony harm)
        {

            harm.Patch(AccessTools.Method(typeof(CompEggLayer), "ProduceEgg"),
                postfix: new HarmonyMethod(typeof(VEF_AnimalGenes_CompEggLayer_ProduceEgg_Patch), "ModifyEggs"));

            harm.Patch(AccessTools.Method(typeof(CompHatcher), "CompTick"),
                postfix: new HarmonyMethod(typeof(VEF_AnimalGenes_CompHatcher_CompTick_Patch), "RemoveIfRuined"));

            harm.Patch(AccessTools.Property(typeof(Pawn), "DescriptionHyperlinks").GetMethod,
               postfix: new HarmonyMethod(typeof(VEF_AnimalGenes_Pawn_DescriptionHyperlinks_Patch), "ModifyHyperlinks"));

            harm.Patch(AccessTools.Method(typeof(StatWorker), "GetOffsetsAndFactorsExplanation"),
               postfix: new HarmonyMethod(typeof(VEF_AnimalGenes_StatWorker_GetOffsetsAndFactorsExplanation_Patch), "ApplyStatModifiersExplanations"));

            harm.Patch(AccessTools.Method(typeof(StatWorker), "GetValueUnfinalized"),
               postfix: new HarmonyMethod(typeof(VEF_AnimalGenes_StatWorker_GetValueUnfinalized_Patch), "ApplyStatModifiers"));

            harm.Patch(AccessTools.Method(typeof(ThingDefGenerator_Corpses), "GenerateCorpseDef"),
              postfix: new HarmonyMethod(typeof(VEF_AnimalGenes_ThingDefGenerator_Corpses_GenerateCorpseDef_Patch), "HandleCorpses"));

            harm.Patch(AccessTools.Method(typeof(Widgets), "DefIcon"),
             postfix: new HarmonyMethod(typeof(VEF_AnimalGenes_Widgets_DefIcon_Patch), "DoGeneDefIcon"));

            harm.Patch(AccessTools.Method(typeof(Hediff_Pregnant), "DoBirthSpawn"),
                transpiler: new HarmonyMethod(typeof(VEF_AnimalGenes_Hediff_Pregnant_DoBirthSpawn_Patch), "AddCrossbreedGenes"));


        }
    }
}
