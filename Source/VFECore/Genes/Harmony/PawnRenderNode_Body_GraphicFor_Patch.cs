using System.Collections.Generic;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(PawnRenderNode_Body), "GraphicFor")]
    public static class PawnRenderNode_Body_GraphicFor_Patch
    {
        public static void Postfix(PawnRenderNode_Body __instance, Pawn pawn, ref Graphic __result)
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
                                if (extension.bodyDessicatedGraphicPath.NullOrEmpty() is false)
                                {
                                    __result = GraphicDatabase.Get<Graphic_Multi>(extension.bodyDessicatedGraphicPath, ShaderDatabase.Cutout);
                                }
                            }
                            else
                            {
                                if (extension.furHidesBody)
                                {
                                    __result = GraphicDatabase.Get<Graphic_Multi>("UI/EmptyImage", ShaderUtility.GetSkinShader(pawn), Vector2.one, pawn.story.SkinColor); ;
                                }
                                if (extension.bodyNakedGraphicPath.NullOrEmpty() is false)
                                {
                                    __result = GraphicDatabase.Get<Graphic_Multi>(extension.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(pawn), Vector2.one, pawn.story.SkinColor);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
