using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(Pawn_DrawTracker), nameof(Pawn_DrawTracker.DrawPos), MethodType.Getter)]
    public static class Pawn_DrawTracker_Patch
    {
        public struct PGPRRCache // Special little cache to save us from running GetPosture().
        {
            public Pawn pawn;
            public CachedPawnData cache;
            public bool cachingDisabled;
            public bool doOffset;
            public bool spawned;
        }
        [ThreadStatic]
        static PGPRRCache threadStaticCache;

        public static bool skipOffset = false;
        [HarmonyPostfix]
        public static void Postfix(ref Vector3 __result, Pawn ___pawn)
        {
            if (___pawn == null || skipOffset) return;

            if (threadStaticCache.pawn != ___pawn)
            {
                threadStaticCache.cache = PawnDataCache.GetPawnDataCache(___pawn, canRefresh: false);
                threadStaticCache.pawn = ___pawn;
                threadStaticCache.doOffset = ___pawn.GetPosture() == PawnPosture.Standing;
            }

            if (threadStaticCache.doOffset)
            {
                __result.z += threadStaticCache.cache.renderPosOffset;
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
