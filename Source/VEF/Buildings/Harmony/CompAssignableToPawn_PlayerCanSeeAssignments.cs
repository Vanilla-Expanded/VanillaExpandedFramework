using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(CompAssignableToPawn), nameof(CompAssignableToPawn.PlayerCanSeeAssignments), MethodType.Getter)]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public class VanillaExpandedFramework_CompAssignableToPawn_PlayerCanSeeAssignments__Patch
{
    public static readonly HashSet<Def> DefsWithNoAssignmentOverlay = [];
    public static readonly HashSet<Def> DefsWithNoUnownedAssignmentOverlay = [];

    public static bool Prepare() => DefsWithNoAssignmentOverlay.Any() || DefsWithNoUnownedAssignmentOverlay.Any();

    public static bool Prefix(CompAssignableToPawn __instance)
    {
        if (DefsWithNoAssignmentOverlay.Contains(__instance.parent.def))
            return false;
        if (__instance.AssignedPawnsForReading.Count <= 0 && DefsWithNoUnownedAssignmentOverlay.Contains(__instance.parent.def))
            return false;
        return true;
    }
}