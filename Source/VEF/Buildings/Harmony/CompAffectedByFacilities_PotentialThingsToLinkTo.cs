using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(CompAffectedByFacilities), nameof(CompAffectedByFacilities.PotentialThingsToLinkTo), MethodType.Enumerator)]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class CompAffectedByFacilities_PotentialThingsToLinkTo_Patch
{
    private static bool Prepare() => VanillaExpandedFramework_CompAffectedByFacilities_CanPotentiallyLinkTo_Patch.IsActive;

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var matcher = new CodeMatcher(instr);

        int totalPatched;
        for (totalPatched = 0; totalPatched < 100; totalPatched++)
        {
            matcher.MatchEndForward(
                new CodeMatch(op => op.IsLdloc() && op.LocalIndex() == 7),
                CodeMatch.LoadsField(typeof(Thing).DeclaredField(nameof(Thing.def)))
            );

            if (matcher.IsInvalid)
                break;

            matcher.InsertAfter(CodeInstruction.Call(() => GetEquivalentFacility));
        }

        const int expectedPatched = 3;
        if (totalPatched != expectedPatched)
            Log.Error($"Patched incorrect amount of instructions for {nameof(CompAffectedByFacilities)}.{nameof(CompAffectedByFacilities.PotentialThingsToLinkTo)}. Expected: {expectedPatched}, patched: {totalPatched}.");

        return matcher.Instructions();
    }

    private static ThingDef GetEquivalentFacility(ThingDef def) => def.GetModExtension<FacilityExtension>()?.equivalentToFacility ?? def;
}