using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt))]
    public static class Patch_RenderPawnAt_2
    {
        [HarmonyPrefix]
        public static void RenderPawnAt_Prefix(ref Vector3 drawLoc, Pawn ___pawn)
        {
            if (___pawn.GetPosture() == PawnPosture.Standing &&
                ScaleCache.GetScaleCache(___pawn) is SizeData data)
            {
                drawLoc.z += data.renderPosOffset;
            }
        }
    }
}
