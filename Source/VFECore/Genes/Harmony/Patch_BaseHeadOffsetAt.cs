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
            FieldInfo field = GetPawnField();
            Pawn pawn = (Pawn)field.GetValue(__instance);
            if (pawn != null)
            {

                if (PawnDataCache.GetPawnDataCache(pawn, canRefresh:false) is CachedPawnData data)
                {
                    var bodySize = data.bodyRenderSize;
                    var headSize = data.headRenderSize;
                    var headPos = Mathf.Lerp(bodySize, headSize, 0.8f);

                    // Move up the head for dwarves etc. so they don't end up a walking head.
                    if (headPos < 1) { headPos = Mathf.Pow(headPos, 0.96f); }
                    __result.z *= headPos;
                    __result.x *= headPos;
                }
            }
        }
        public static FieldInfo pawnFieldInfo = null;
        private static FieldInfo GetPawnField()
        {
            if (pawnFieldInfo == null)
            {
                pawnFieldInfo = typeof(PawnRenderer).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return pawnFieldInfo;
        }
    }
}
