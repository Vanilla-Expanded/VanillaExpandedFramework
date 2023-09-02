using HarmonyLib;
using UnityEngine;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(GeneGraphicData), "GetColorFor")]
    public static class VanillaGenesExpanded_GeneGraphicData_GetColorFor_Patch
    {
        public static bool Prefix(GeneGraphicData __instance, Pawn pawn, ref Color __result)
        {
            foreach (var gene in DefDatabase<GeneDef>.AllDefsListForReading)
            {
                if (gene.graphicData == __instance)
                {
                    var extension = gene.GetModExtension<GeneExtension>();
                    if (extension?.applySkinColorWithGenes != null)
                    {
                        foreach (var otherGene in extension.applySkinColorWithGenes)
                        {
                            if (pawn.genes.GetGene(otherGene)?.Active ?? false)
                            {
                                __result = pawn.story.SkinColor;
                                return false;
                            }
                        }
                    }
                    break;
                }
            }
            return true;
        }
    }
}