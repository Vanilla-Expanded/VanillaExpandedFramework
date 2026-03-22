using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Maps;

[HarmonyPatch(typeof(RoofCollapserImmediate), "DropRoofInCellPhaseOne")]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_RoofCollapserImmediate_DropRoofInCellPhaseOne_Patch
{
    private static bool Prepare(MethodBase method)
    {
        // Always run after the first pass
        if (method != null)
            return true;

        foreach (var def in DefDatabase<RoofDef>.AllDefs)
        {
            var extension = def.GetModExtension<RoofExtension>();
            if (extension is { AlwaysDealsDamageOnCollapsed: false })
                return true;
        }

        return false;
    }

    private static bool Prefix(IntVec3 c, Map map)
    {
        var roof = map.roofGrid.RoofAt(c);
        var extension = roof?.GetModExtension<RoofExtension>();
        // Let the original run if no extension or deal damage on collapsed is true.
        if (extension == null || extension.DealDamageOnCollapsed(map, c, roof))
            return true;

        // Throw a dust puff, like in the original method.
        FleckMaker.ThrowDustPuff(c.ToVector3Shifted() + Gen.RandomHorizontalVector(0.6f), map, 2f);
        return false;
    }
}