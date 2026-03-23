using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace VEF.Maps;

[HarmonyPatch(typeof(SectionLayer_LightingOverlay), "GenerateLightingOverlay")]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_SectionLayer_LightingOverlay_GenerateLightingOverlay_Patch
{
    private static bool Prepare(MethodBase method)
    {
        // Always allow after first pass
        if (method != null)
            return true;

        foreach (var def in DefDatabase<RoofDef>.AllDefs)
        {
            var extension = def.GetModExtension<RoofExtension>();
            if (extension is { AlwaysDrawsShadow: false })
                return true;
        }

        return false;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var matcher = new CodeMatcher(instr);

        matcher.MatchEndForward(
            CodeMatch.IsLdloc(),
            CodeMatch.IsLdloc(),
            CodeMatch.Calls(typeof(RoofGrid).DeclaredMethod(nameof(RoofGrid.RoofAt), [typeof(int)])),
            CodeMatch.IsStloc()
        );

        matcher.Insert(
            // Load the map argument
            CodeInstruction.LoadArgument(0),
            // Load the cell index local (use the same this method uses earlier)
            matcher.InstructionAt(-2).Clone(),
            // Call our map
            CodeInstruction.Call(() => RoofAtWrapper)
        );

        return matcher.Instructions();
    }

    private static RoofDef RoofAtWrapper(RoofDef def, Map map, int cellIndex)
    {
        var extension = def?.GetModExtension<RoofExtension>();
        if (extension != null && !extension.ShouldDrawShadow(map, cellIndex, def))
            return null;
        return def;
    }
}