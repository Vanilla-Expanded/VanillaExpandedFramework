using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(Pawn_DrawTracker), nameof(Pawn_DrawTracker.DrawPos), MethodType.Getter)]
    public static class Pawn_DrawTracker_Patch
    {
        public static bool skipOffset = false;
        [HarmonyPostfix]
        public static void Postfix(ref Vector3 __result, Pawn ___pawn)
        {
            if (!skipOffset
                && PawnDataCache.GetPawnDataCache(___pawn, canRefresh: false) is CachedPawnData data
                && ___pawn.GetPosture() == PawnPosture.Standing
                )
            {
                __result.z += data.renderPosOffset;
            }
        }
    }

    [HarmonyPatch(typeof(SelectionDrawer), nameof(SelectionDrawer.DrawSelectionBracketFor))]
    public static class SelectionDrawer_DrawSelection_Patch
    {
        public static void Prefix(object obj)
        {
            Pawn_DrawTracker_Patch.skipOffset = true;
        }

        public static void Postfix(object obj)
        {
            Pawn_DrawTracker_Patch.skipOffset = false;
        }
    }

    [HarmonyPatch(typeof(PawnUIOverlay), nameof(PawnUIOverlay.DrawPawnGUIOverlay))]
    public static class PawnUIOverlay_DrawSelection_Patch
    {
        public static void Prefix()
        {
            Pawn_DrawTracker_Patch.skipOffset = true;
        }

        public static void Postfix()
        {
            Pawn_DrawTracker_Patch.skipOffset = false;
        }
    }
}
