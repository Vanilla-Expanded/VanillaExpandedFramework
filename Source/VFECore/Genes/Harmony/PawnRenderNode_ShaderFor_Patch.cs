using System.Collections.Generic;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(PawnRenderNode), "ShaderFor")]
    public static class PawnRenderNode_ShaderFor_Patch
    {
        public static void Postfix(PawnRenderNode __instance, Pawn pawn, ref Shader __result)
        {
            if (__instance is PawnRenderNode_Head)
            {
                if (ModsConfig.BiotechActive && pawn?.RaceProps?.Humanlike == true && pawn?.genes != null)
                {
                    List<Gene> genes = pawn.genes.GenesListForReading;
                    foreach (Gene gene in genes)
                    {
                        if (gene.Active)
                        {
                            var extension = gene.def.GetModExtension<GeneExtension>();
                            if (extension?.forceHeadShader != null)
                            {
                                __result = extension.forceHeadShader.Shader;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
