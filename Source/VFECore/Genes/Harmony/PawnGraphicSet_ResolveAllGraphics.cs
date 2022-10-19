using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    public static class PawnGraphicSet_ResolveAllGraphics_Patch
    {
        [HarmonyPostfix]
        public static void PostFix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            if (ModLister.BiotechInstalled && pawn.RaceProps.Humanlike)
            {
                if (pawn.genes.GenesListForReading.Any(g => g.def.GetModExtension<GeneExtension>()?.useSkinColorForFur ?? false))
                {
                    __instance.furCoveredGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.furDef.GetFurBodyGraphicPath(pawn), ShaderUtility.GetSkinShader(pawn.story.SkinColorOverriden), Vector2.one, pawn.story.SkinColor);
                }

                if (pawn.genes.GenesListForReading.Any(g => g.def.GetModExtension<GeneExtension>()?.noHeadColouring ?? false))
                {
                    
                    __instance.headGraphic = pawn.story.headType.GetGraphic(Color.white, dessicated: false);
                }
            }
        }
    }
}