using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(ListerBuildings), nameof(ListerBuildings.Add))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_ListerBuildings_Add_Patch
{
    private static bool Prepare() => VanillaExpandedFramework_ShortCircuitUtility_TryShortCircuitInRain_Patch.patchActive;

    private static void Postfix(Building b, HashSet<Building> ___allBuildingsColonistElecFire)
    {
        if (b.def.building is { isNaturalRock: true })
            return;

        var customShortCircuit = b.def.GetCompProperties<CompProperties_CustomShortCircuitInRain>();
        if (customShortCircuit != null)
            ___allBuildingsColonistElecFire.Add(b);
    }
}