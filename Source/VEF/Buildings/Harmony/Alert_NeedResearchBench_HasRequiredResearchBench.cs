using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(Alert_NeedResearchBench), "HasRequiredResearchBench", MethodType.Getter)]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_Alert_NeedResearchBench_HasRequiredResearchBench_Patch
{
    private static bool Prepare() => VanillaExpandedFramework_ResearchProjectDef_CanBeResearchedAt_Patch.IsPatchActive;

    private static void Postfix(ref bool __result)
    {
        // If bench found, return
        if (__result)
            return;

        var benches = Find.ResearchManager.GetProject().requiredResearchBuilding?.GetModExtension<ResearchBuildingExtension>()?.equivalentBenches;
        // If the research building has no equivalent, return
        if (benches == null || benches.Count == 0)
            return;

        // Search for a presence of an equivalent research bench on any map
        var maps = Find.Maps;
        for (var i = 0; i < maps.Count; i++)
        {
            for (var j = 0; j < benches.Count; j++)
            {
                if (maps[i].listerBuildings.ColonistsHaveBuilding(benches[j]))
                {
                    __result = true;
                    return;
                }
            }
        }
    }

    // private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    // {
    //     var matcher = new CodeMatcher(instr);
    //
    //     matcher.MatchEndForward(
    //         CodeMatch.IsLdloc(),
    //         CodeMatch.LoadsField(typeof(ResearchProjectDef).DeclaredField(nameof(ResearchProjectDef.requiredResearchBuilding))),
    //         CodeMatch.Branches(),
    //         CodeMatch.IsLdloc(),
    //         CodeMatch.IsLdloc()
    //     );
    //
    //     if (matcher.IsValid)
    //     {
    //         matcher.InsertAfter(
    //             // Clone the last 2 instructions
    //             matcher.InstructionAt(-1).Clone(),
    //             matcher.Instruction.Clone()
    //         );
    //     }
    //     else Log.Error("Patching Alert_NeedResearchBench:HasRequiredResearchBench failed - failed to find code match. Alerts about missing research benches may be incorrectly displayed.");
    //
    //     return matcher.Instructions();
    // }
    //
    // private static bool ColonistsHaveEquivalentBuilding(ResearchProjectDef researchProject)
    // {
    // }
}