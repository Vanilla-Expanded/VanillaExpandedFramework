using HarmonyLib;
using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(PawnRenderNode_Fur), "GraphicFor")]
    public static class PawnRenderNode_Fur_GraphicFor_Patch
    {
        public static void Postfix(PawnRenderNode_Fur __instance, Pawn pawn, ref Graphic __result)
        {
            if (__instance.gene != null)
            {
                GeneExtension extension = __instance.gene.def.GetModExtension<GeneExtension>();
                if (extension != null)
                {
                    if (extension.useMaskForFur)
                    {
                        __result = pawn.genes.GenesListForReading.Where(x => x.Active).Any(g => g.def.GetModExtension<GeneExtension>()?.useSkinColorForFur ?? false) ?
                                                                               GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderDatabase.CutoutComplex, Vector2.one, pawn.story.SkinColor) :
                                                                               GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderDatabase.CutoutSkinOverlay, Vector2.one, pawn.story.HairColor);
                    }
                    else if (extension.useSkinColorForFur)
                    {
                        __result = GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderUtility.GetSkinShader(pawn), Vector2.one, pawn.story.SkinColor);

                    }
                    else if (extension.useSkinAndHairColorsForFur)
                    {
                        __result = GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderDatabase.CutoutComplex, Vector2.one, pawn.story.SkinColor, pawn.story.HairColor);
                    }
                    else if (extension.dontColourFur)
                    {
                        __result = GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                }
            }
        }
    }
}