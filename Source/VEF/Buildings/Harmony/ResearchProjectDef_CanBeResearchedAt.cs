using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(ResearchProjectDef), nameof(ResearchProjectDef.CanBeResearchedAt))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_ResearchProjectDef_CanBeResearchedAt_Patch
{
    private static bool Prepare(MethodBase baseMethod)
    {
        // Always run after first pass
        if (baseMethod != null)
            return true;

        foreach (var def in DefDatabase<ThingDef>.AllDefs)
        {
            var extension = def.GetModExtension<ResearchBuildingExtension>();
            if (extension != null && !extension.equivalentBenches.NullOrEmpty())
                return true;
        }

        return false;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var matcher = new CodeMatcher(instr);

        matcher.MatchEndForward(
            // Loads the Building_ResearchBench argument
            CodeMatch.IsLdarg(1),
            // Loads the "def" field
            CodeMatch.LoadsField(typeof(Thing).DeclaredField(nameof(Thing.def))),
            // Loads "this" argument
            CodeMatch.IsLdarg(0),
            // Loads the "requiredResearchBuilding" field
            CodeMatch.LoadsField(typeof(ResearchProjectDef).DeclaredField(nameof(ResearchProjectDef.requiredResearchBuilding))),
            // Jumps over the "return false" if we don't have the correct building
            CodeMatch.Branches()
        );

        // Rather than comparing the 2 defs (which we move to our wrapper method),
        // we instead check if our method returned true or false.
        // This instruction branches over "return false", so we only jump if our wrapper returned true.
        matcher.Opcode = OpCodes.Brtrue_S;
        // Insert our wrapper method
        matcher.Insert(CodeInstruction.Call(() => AreBenchesEquivalent));

        return matcher.Instructions();
    }

    private static bool AreBenchesEquivalent(ThingDef actualBench, ThingDef requiredBench)
    {
        if (actualBench == requiredBench)
            return true;

        var extension = requiredBench.GetModExtension<ResearchBuildingExtension>();
        return extension != null && !extension.equivalentBenches.NullOrEmpty() && extension.equivalentBenches.Contains(actualBench);
    }
}