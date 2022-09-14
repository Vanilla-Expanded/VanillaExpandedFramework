using HarmonyLib;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(DeepResourceGrid), "GetCellExtraColor")]
    public static class Patch_DeepResourceGrid_GetCellExtraColor
    {
        [HarmonyPostfix]
        public static void PostFix(int index, DeepResourceGrid __instance, Map ___map, ref Color __result)
        {
            IntVec3 c = ___map.cellIndices.IndexToCell(index);
            ThingDef thingDef = __instance.ThingDefAt(c);
            if (thingDef.GetModExtension<ThingDefExtension>() is ThingDefExtension thingDefExtension)
            {
                int num = __instance.CountAt(c);
                float percent = (float)num / thingDef.deepCountPerCell * thingDefExtension.transparencyMultiplier;
                __result = thingDefExtension.deepColor.ToTransparent(percent);
            }
        }
    }
}
