using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Things;

[HarmonyPatch(typeof(Alert_FireInHomeArea), nameof(Alert_FireInHomeArea.GetReport))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_Alert_FireInHomeArea_GetReport_Patch
{
    private static readonly List<CustomFireDef> CustomFiresWithDefaultAlert = [];

    private static bool Prepare(MethodBase baseMethod)
    {
        // Always true after first pass
        if (baseMethod != null)
            return true;

        var defs = DefDatabase<CustomFireDef>.AllDefsListForReading;
        for (var i = 0; i < defs.Count; i++)
        {
            var def = defs[i];
            if (def.useVanillaFireAlert)
                CustomFiresWithDefaultAlert.Add(def);
        }

        CustomFiresWithDefaultAlert.Capacity = CustomFiresWithDefaultAlert.Count;
        return CustomFiresWithDefaultAlert.Count > 0;
    }

    private static void Postfix(ref AlertReport __result)
    {
        if (__result.active)
            return;

        var maps = Find.Maps;
        for (var i = 0; i < maps.Count; i++)
        {
            var map = maps[i];
            for (var j = 0; j < CustomFiresWithDefaultAlert.Count; j++)
            {
                var list = map.listerThings.ThingsOfDef(CustomFiresWithDefaultAlert[j]);
                for (var k = 0; k < list.Count; k++)
                {
                    var fire = list[k];
                    if (map.areaManager.Home[fire.Position] && !fire.Position.Fogged(fire.Map))
                    {
                        __result = fire;
                        return;
                    }
                }
            }
        }
    }
}