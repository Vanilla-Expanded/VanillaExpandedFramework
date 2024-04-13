using System.Collections.Generic;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(PawnRenderNode_Head), "GraphicFor")]
    public static class PawnRenderNode_Head_GraphicFor_Patch
    {
        public static void Postfix(PawnRenderNode_Head __instance, Pawn pawn, ref Graphic __result)
        {
            if (ModsConfig.BiotechActive && pawn?.RaceProps?.Humanlike == true && pawn?.genes != null)
            {
                List<Gene> genes = pawn.genes.GenesListForReading;
                foreach (Gene gene in genes)
                {
                    if (gene.Active)
                    {
                        var extension = gene.def.GetModExtension<GeneExtension>();
                        if (extension != null)
                        {
                            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
                            {
                                if (extension.headDessicatedGraphicPath.NullOrEmpty() is false)
                                {
                                    Shader skinShader = ShaderUtility.GetSkinShader(pawn);
                                    __result = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(extension.headDessicatedGraphicPath, skinShader, Vector2.one, Color.white);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
