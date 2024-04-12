using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(TattooDef), "GraphicFor")]
    public static class TattooDef_GraphicFor_Patch
    {
        public static void Postfix(TattooDef __instance, ref Graphic __result, Pawn pawn, Color color)
        {
            if (__result != null && pawn.genes != null && __instance.tattooType == TattooType.Body)
            {
                List<Gene> genes = pawn.genes.GenesListForReading;
                foreach (Gene gene in genes)
                {
                    if (gene.Active)
                    {
                        GeneExtension extension = gene.def.GetModExtension<GeneExtension>();
                        if (extension != null && extension.bodyNakedGraphicPath.NullOrEmpty() is false)
                        {
                            __result = GraphicDatabase.Get<Graphic_Multi>(__instance.texPath, ShaderDatabase.CutoutSkinOverlay, Vector2.one, color, Color.white, null, extension.bodyNakedGraphicPath);
                        }
                    }
                }
            }
        }
    }
}