using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
    public static class PawnGenerator_GenerateGenes_Patch
    {
        public static void Postfix(Pawn pawn)
        {
            List<Gene> genes = pawn.genes.GenesListForReading;
            foreach (Gene gene in genes)
            {
                if (gene.Active)
                {
                    Gene_PostAdd_Patch.ApplyGeneEffects(gene);
                }
            }
        }
    }
    [HarmonyPatch(typeof(Gene), "PostAdd")]
    public static class Gene_PostAdd_Patch
    {
        public static void Postfix(Gene __instance)
        {
            if (PawnGenerator.IsBeingGenerated(__instance.pawn) is false && __instance.Active)
            {
                ApplyGeneEffects(__instance);
            }
        }
        public static void ApplyGeneEffects(Gene __instance)
        {
            GeneExtension extension = __instance.def.GetModExtension<GeneExtension>();
            if (extension != null)
            {
                if (extension.forceFemale == true)
                {
                    __instance.pawn.gender = Gender.Female;
                }
                if (extension.forceMale == true)
                {
                    __instance.pawn.gender = Gender.Male;
                }

                if (extension.forcedBodyType != null)
                {
                    __instance.pawn.story.bodyType = extension.forcedBodyType;
                    __instance.pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
                }
            }
        }
    }
}