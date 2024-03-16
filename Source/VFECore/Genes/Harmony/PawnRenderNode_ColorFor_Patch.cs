using HarmonyLib;
using UnityEngine;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(PawnRenderNode), "ColorFor")]
    public static class PawnRenderNode_ColorFor_Patch
    {
        public static void Postfix(PawnRenderNode __instance, Pawn pawn, ref Color __result)
        {
            if (__instance.gene != null)
            {
                var extension = __instance.gene.def.GetModExtension<GeneExtension>();
                if (extension?.applySkinColorWithGenes != null)
                {
                    foreach (var otherGene in extension.applySkinColorWithGenes)
                    {
                        if (pawn.genes.GetGene(otherGene)?.Active ?? false)
                        {
                            __result = pawn.story.SkinColor;
                            return;
                        }
                    }
                }
            }
        }
    }
}