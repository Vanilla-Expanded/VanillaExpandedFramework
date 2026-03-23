using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace VEF.Maps;

[HarmonyPatch(typeof(RoofGrid), nameof(RoofGrid.GetCellExtraColor))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_RoofGrid_GetCellExtraColor_Patch
{
    private static bool expandedRoofingActive = false;
    private static Color baseColor = new(0.3f, 1f, 0.4f);

    private static bool Prepare(MethodBase method)
    {
        // Always run on second pass
        if (method != null)
            return true;

        expandedRoofingActive = ModLister.AnyModActiveNoSuffix(["Mlie.ExpandedRoofing"]);

        foreach (var def in DefDatabase<RoofDef>.AllDefs)
        {
            var extension = def.GetModExtension<RoofExtension>();
            if (extension is { EverUsesCustomOverlayColor: true })
                return true;
        }

        return false;
    }

    private static void Postfix(int index, RoofDef[] ___roofGrid, Map ___map, ref Color __result)
    {
        var roof = ___roofGrid[index];
        if (roof?.GetModExtension<RoofExtension>()?.RoofOverlayColor(___map, index, roof) is { } color)
            __result = color;
        // Expanded roofing replaces vanilla color handling. It gives thin mountain roof unique color, on top of making on non-vanilla roofs magenta.
        else if (!expandedRoofingActive)
            __result *= baseColor;
    }
}