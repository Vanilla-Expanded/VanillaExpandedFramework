using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace VEF.Maps;

[HarmonyPatch(typeof(RoofGrid), nameof(RoofGrid.GetCellExtraColor))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_RoofGrid_GetCellExtraColor_Patch
{
    private static bool Prepare(MethodBase method)
    {
        // Always run on second pass
        if (method != null)
            return true;

        foreach (var def in DefDatabase<RoofDef>.AllDefs)
        {
            var extension = def.GetModExtension<RoofExtension>();
            if (extension is { EverUsesCustomOverlayTint: true })
                return true;
        }

        return false;
    }

    private static bool Prefix(int index, RoofDef[] ___roofGrid, Map ___map, ref Color __result)
    {
        var roof = ___roofGrid[index];
        var extension = roof?.GetModExtension<RoofExtension>();
        if (extension == null)
            return true;

        __result = extension.RoofOverlayTint(___map, index, roof);
        return __result == Color.white;
    }
}