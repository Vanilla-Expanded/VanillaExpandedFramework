using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(RitualOutcomeComp_GravshipFacilities), nameof(RitualOutcomeComp_GravshipFacilities.GetQualityFactor))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_RitualOutcomeComp_GravshipFacilities_GetQualityFactor_Patch
{
    private static HashSet<ThingDef> tmpUsedFacilitiesForCount = [];

    private static bool Prepare() => VanillaExpandedFramework_CompAffectedByFacilities_CanPotentiallyLinkTo_Patch.isActive;

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instr, generator);

        var clearDictInstr = new[]
        {
            CodeInstruction.LoadField(typeof(VanillaExpandedFramework_RitualOutcomeComp_GravshipFacilities_GetQualityFactor_Patch), nameof(tmpUsedFacilitiesForCount)),
            CodeInstruction.Call(() => tmpUsedFacilitiesForCount.Clear)
        };

        // Catch a call to clear the built-in temp dictionary and clear our own
        matcher.MatchEndForward(CodeMatch.Calls(typeof(Dictionary<ThingDef, int>).DeclaredMethod(nameof(Dictionary<ThingDef, int>.Clear))));
        if (matcher.IsInvalid)
            Log.Error($"[VEF] Failed patching {nameof(RitualOutcomeComp_GravshipFacilities)} - couldn't find `tmpFacilityCount:Clear` call.");
        matcher.InsertAfter(clearDictInstr);

        // Match thingDef.GetCompProperties<CompProperties_GravshipFacility>().maxSimultaneous
        // Replace thingDef with the equivalent, and skip if duplicate.
        matcher.MatchStartForward(
            CodeMatch.LoadsLocal(),
            CodeMatch.LoadsLocal(),
            CodeMatch.Calls(typeof(ThingDef).DeclaredMethod(nameof(ThingDef.GetCompProperties)).MakeGenericMethod(typeof(CompProperties_GravshipFacility))),
            CodeMatch.LoadsField(typeof(CompProperties_Facility).DeclaredField(nameof(CompProperties_Facility.maxSimultaneous)))
        );
        if (matcher.IsInvalid)
            Log.Error($"[VEF] Failed patching {nameof(RitualOutcomeComp_GravshipFacilities)} - couldn't find location where max simultaneous connections are calculated.");
        matcher.DefineLabel(out var label);
        matcher.Insert(
            CodeInstruction.LoadLocal(matcher.InstructionAt(1).LocalIndex(), true),
            CodeInstruction.Call(() => HandleEquivalentFacility),
            new CodeInstruction(OpCodes.Brfalse_S, label)
        );

        // Look for the MoveNext method
        matcher.MatchStartForward(
            CodeMatch.LoadsLocal(true),
            CodeMatch.Calls(typeof(Dictionary<ThingDef, float>.Enumerator).DeclaredMethod(nameof(Dictionary<ThingDef, float>.Enumerator.MoveNext)))
        );
        if (matcher.IsInvalid)
            Log.Error($"[VEF] Failed patching {nameof(RitualOutcomeComp_GravshipFacilities)} - failed to find Dictionary+Enumerator:MoveNext.");
        matcher.AddLabels([label]);

        // Catch a call very close to the end of the method
        matcher.MatchEndForward(
            new CodeMatch(OpCodes.Newobj, typeof(QualityFactor).Constructor([]))
        );
        if (matcher.IsInvalid)
            Log.Error($"[VEF] Failed patching {nameof(RitualOutcomeComp_GravshipFacilities)} - couldn't find `new QualityFactor` call near final return statement.");
        matcher.Insert(clearDictInstr);

        return matcher.Instructions();
    }

    public static bool HandleEquivalentFacility(ref ThingDef def)
    {
        def = def.GetModExtension<FacilityExtension>()?.equivalentToFacility ?? def;
        return tmpUsedFacilitiesForCount.Add(def);
    }
}