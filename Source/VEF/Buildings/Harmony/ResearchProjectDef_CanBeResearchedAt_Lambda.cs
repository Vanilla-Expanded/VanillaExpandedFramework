using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_ResearchProjectDef_CanBeResearchedAt_Lambda_Patch
{
    private static bool Prepare(MethodBase baseMethod)
    {
        // Always run after first pass
        if (baseMethod != null)
            return true;

        foreach (var def in DefDatabase<ThingDef>.AllDefs)
        {
            var extension = def.GetModExtension<ResearchBuildingExtension>();
            if (extension != null && !extension.equivalentFacilities.NullOrEmpty())
                return true;
        }

        return false;
    }

    private static MethodBase TargetMethod()
    {
        return typeof(ResearchProjectDef)
            .FirstInner(x => x.DeclaredField("affectedByFacilities") != null)
            .FirstMethod(x =>
                x.Name.Contains("<CanBeResearchedAt>") &&
                x.ReturnType == typeof(bool) &&
                x.GetParameters().Length == 1 &&
                x.GetParameters()[0].ParameterType == typeof(Thing));
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var matcher = new CodeMatcher(instr);

        matcher.MatchEndForward(
            // Loads the facility Thing argument
            CodeMatch.IsLdarg(1),
            // Loads the "Thing.def" field
            CodeMatch.LoadsField(typeof(Thing).DeclaredField(nameof(Thing.def))),
            // Loads "this"
            CodeMatch.IsLdarg(0),
            // Loads "<>4__this" (ResearchProjectDef) field (skip actual field check, just that a field is loaded)
            new CodeMatch(OpCodes.Ldfld),
            // Loads "ResearchProjectDef.requiredResearchFacilities" field
            CodeMatch.LoadsField(typeof(ResearchProjectDef).DeclaredField(nameof(ResearchProjectDef.requiredResearchFacilities))),
            // Loads "this"
            CodeMatch.IsLdarg(0),
            // Loads "i" (current index) field (skip actual field check, just that a field is loaded)
            new CodeMatch(OpCodes.Ldfld),
            // Calls the list indexer getter (skip actual method check, just that a method is called)
            CodeMatch.Calls((MethodInfo)null),
            // Jumps to the return instruction
            CodeMatch.Branches()
        );

        // Rather than comparing the 2 defs (which we move to our wrapper method),
        // we instead check if our method returned true or false.
        // This instruction branches to "return false", so we only jump if our wrapper returned false.
        matcher.Opcode = OpCodes.Brfalse_S;
        // Insert our wrapper method
        matcher.Insert(CodeInstruction.Call(() => AreFacilitiesEquivalent));

        return matcher.Instructions();
    }

    private static bool AreFacilitiesEquivalent(ThingDef actualFacility, ThingDef requiredFacility)
    {
        if (actualFacility == requiredFacility)
            return true;

        var extension = requiredFacility.GetModExtension<ResearchBuildingExtension>();
        return extension != null && !extension.equivalentFacilities.NullOrEmpty() && extension.equivalentFacilities.Contains(actualFacility);
    }
}