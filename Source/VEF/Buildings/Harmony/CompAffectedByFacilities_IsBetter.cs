using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(CompAffectedByFacilities), "IsBetter")]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_CompAffectedByFacilities_IsBetter_Patch
{
    private static bool Prepare() => VanillaExpandedFramework_CompAffectedByFacilities_CanPotentiallyLinkTo_Patch.IsActive;

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var matcher = new CodeMatcher(instr);

        matcher.MatchEndForward(
            // Loads the facility ThingDef
            CodeMatch.IsLdarg(1),
            // Loads the actual facility Thing
            CodeMatch.IsLdarg(4),
            // Load the building's def
            CodeMatch.LoadsField(typeof(Thing).DeclaredField(nameof(Thing.def))),
            // Branches the error/return
            CodeMatch.Branches()
        );

        // Rather than comparing the 2 defs (which we move to our wrapper method),
        // we instead check if our method returned true or false.
        // This jumps over code that logs an error and returns false.
        matcher.Opcode = OpCodes.Brtrue_S;
        // Insert our wrapper method
        matcher.Insert(CodeInstruction.Call(() => FacilityExtension.AreFacilitiesEquivalent));

        return matcher.Instructions();
    }
}