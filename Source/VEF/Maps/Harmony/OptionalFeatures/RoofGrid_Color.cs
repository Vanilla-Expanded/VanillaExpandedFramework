using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace VEF.Maps;

[HarmonyPatch(typeof(RoofGrid), nameof(RoofGrid.Color), MethodType.Getter)]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class RoofGrid_Color_Patch
{
    private static bool Prepare(MethodBase method)
    {
        if (method != null)
            return true;
        // Expanded roofing does the same as us, don't apply if it's active
        if (ModLister.AnyModActiveNoSuffix(["Mlie.ExpandedRoofing"]))
            return false;

        foreach (var def in DefDatabase<RoofDef>.AllDefs)
        {
            var extension = def.GetModExtension<RoofExtension>();
            if (extension is { EverUsesCustomOverlayColor: true })
                return true;
        }

        return false;
    }

    private static Color baseColor = new(0.3f, 1f, 0.4f);

    private static bool Prefix(ref Color __result)
    {
        __result = Color.white;
        return false;
    }
}