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
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.DrawAt))]
    public static class Patch_RenderPawnAt_2
    {
        [HarmonyPrefix]
        public static void DrawAt_Prefix(ref Vector3 drawLoc, Pawn __instance)
        {
            if (__instance.GetPosture() == PawnPosture.Standing &&
                ScaleCache.GetScaleCache(__instance) is SizeData data)
            {
                drawLoc.z += data.renderPosOffset;
            }
        }
    }
}
