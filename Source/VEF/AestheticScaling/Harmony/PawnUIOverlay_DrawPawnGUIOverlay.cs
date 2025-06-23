using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace VEF.AestheticScaling
{
    [HarmonyPatch(typeof(PawnUIOverlay), nameof(PawnUIOverlay.DrawPawnGUIOverlay))]
    public static class VanillaExpandedFramework_PawnUIOverlay_DrawPawnGUIOverlay_Patch
    {
        public static void Prefix()
        {
            VanillaExpandedFramework_Pawn_DrawTracker_DrawPos_Patch.skipOffset = true;
        }

        public static void Postfix()
        {
            VanillaExpandedFramework_Pawn_DrawTracker_DrawPos_Patch.skipOffset = false;
        }
    }
}
