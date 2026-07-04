using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.PostDraw))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_CompRefuelable_PostDraw_Patch
{
    public static bool patchActive = false;

    private static bool Prepare() => patchActive;

    private static void Postfix(CompRefuelable __instance)
    {
        __instance.parent.def?.GetModExtension<RefuelableExtension>()?.customFuelGauge?.DrawGauge(__instance.parent, __instance.FuelPercentOfMax);
    }
}