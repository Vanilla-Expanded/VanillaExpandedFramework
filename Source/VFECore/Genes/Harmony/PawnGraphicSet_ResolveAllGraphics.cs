using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    public static class VanillaGenesExpanded_PawnGraphicSet_ResolveAllGraphics_Patch
    {
        [HarmonyPostfix]
        public static void PostFix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            if (ModLister.BiotechInstalled && pawn.RaceProps.Humanlike && pawn.genes != null)
            {
                if (pawn.genes.GenesListForReading.Where(x => x.Active).Any(g => g.def.GetModExtension<GeneExtension>()?.useMaskForFur ?? false))
                {
                    __instance.furCoveredGraphic = pawn.genes.GenesListForReading.Where(x => x.Active).Any(g => g.def.GetModExtension<GeneExtension>()?.useSkinColorForFur ?? false) ? 
                                                       GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderDatabase.CutoutComplex, Vector2.one, pawn.story.SkinColor) : 
                                                       GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderDatabase.CutoutSkinOverlay, Vector2.one, pawn.story.HairColor);
                    __instance.headGraphic = __instance.headGraphic.GetCopy(__instance.headGraphic.drawSize, ShaderDatabase.CutoutComplex);
                }
                else
                {
                    if (pawn.genes.GenesListForReading.Where(x => x.Active).Any(g => g.def.GetModExtension<GeneExtension>()?.useSkinColorForFur ?? false))
                        __instance.furCoveredGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, pawn.story.SkinColor);
                }

                if (pawn.genes.GenesListForReading.Where(x => x.Active).Any(g => g.def.GetModExtension<GeneExtension>()?.dontColourFur ?? false))
                {
                    __instance.furCoveredGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderDatabase.Cutout, Vector2.one, Color.white);
                }
                if (pawn.genes.GenesListForReading.Where(x => x.Active).Any(g => g.def.GetModExtension<GeneExtension>()?.furHidesBody ?? false))
                {
                    __instance.nakedGraphic= GraphicDatabase.Get<Graphic_Multi>("UI/EmptyImage", ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, pawn.story.SkinColor); ;
                }

                List<Gene> genes = __instance.pawn.genes.GenesListForReading;
                foreach (Gene gene in genes)
                {
                    if (gene.Active)
                    {
                        GeneExtension extension = gene.def.GetModExtension<GeneExtension>();
                        if (extension != null)
                        {
                            Color color = pawn.story.SkinColorOverriden
                                    ? (PawnGraphicSet.RottingColorDefault * pawn.story.SkinColor)
                                    : PawnGraphicSet.RottingColorDefault;
                            if (extension.bodyNakedGraphicPath.NullOrEmpty() is false)
                            {
                                __instance.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(extension.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, pawn.story.SkinColor);
                                __instance.rottingGraphic = GraphicDatabase.Get<Graphic_Multi>(extension.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, color);
                                if (pawn.style != null && ModsConfig.IdeologyActive && (!ModLister.BiotechInstalled || pawn.genes == null || !pawn.genes.GenesListForReading.Any((Gene x) => x.def.graphicData != null && !x.def.graphicData.tattoosVisible && x.Active)))
                                {
                                    Color skinColor = pawn.story.SkinColor;
                                    skinColor.a *= 0.8f;
                                    if (pawn.style.BodyTattoo != null && pawn.style.BodyTattoo != TattooDefOf.NoTattoo_Body)
                                    {
                                        __instance.bodyTattooGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.style.BodyTattoo.texPath, ShaderDatabase.CutoutSkinOverlay, Vector2.one, skinColor, Color.white, null, extension.bodyNakedGraphicPath);
                                    }
                                }
                            }

                            if (extension.bodyDessicatedGraphicPath.NullOrEmpty() is false)
                            {
                                __instance.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(extension.bodyDessicatedGraphicPath, ShaderDatabase.Cutout);
                            }

                            if (extension.headDessicatedGraphicPath.NullOrEmpty() is false)
                            {
                                __instance.desiccatedHeadGraphic = GetGraphic(extension.headDessicatedGraphicPath, color,
                                    dessicated: true, pawn.story.SkinColorOverriden);
                            }
                            if (extension.skullGraphicPath.NullOrEmpty() is false)
                            {
                                __instance.skullGraphic = GetGraphic(extension.skullGraphicPath, Color.white, dessicated: true);
                            }
                        }
                    }
                }

                static Graphic_Multi GetGraphic(string graphicPath, Color color, bool dessicated = false, bool skinColorOverriden = false)
                {
                    Shader shader = (!dessicated) ? ShaderUtility.GetSkinShader(skinColorOverriden) : ShaderDatabase.Cutout;
                    Graphic_Multi graphic_Multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPath, shader, Vector2.one, color);
                    return graphic_Multi;
                }
            }
        }
    }
}