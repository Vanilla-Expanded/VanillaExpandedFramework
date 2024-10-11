using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(PawnRenderer), "BaseHeadOffsetAt")]
    public static class PawnRenderer_BaseHeadOffsetAt
    {
        public static void Postfix(PawnRenderer __instance, ref Vector3 __result)
        {
            Pawn pawn = GetPawnFromRef(__instance);
            if (pawn != null)
            {

                if (PawnDataCache.GetCacheUltraSpeed(pawn, canRefresh:false) is CachedPawnData data)
                {
                    __result = new Vector3(__result.x * data.headPositionMultiplier, __result.y, __result.z * data.headPositionMultiplier);
                }
            }
        }
        public static AccessTools.FieldRef<PawnRenderer, Pawn> pawnFieldRef = null;
        private static Pawn GetPawnFromRef(PawnRenderer __instance)
        {
            pawnFieldRef ??= AccessTools.FieldRefAccess<PawnRenderer, Pawn>("pawn");
            return pawnFieldRef(__instance);
        }
    }
}
