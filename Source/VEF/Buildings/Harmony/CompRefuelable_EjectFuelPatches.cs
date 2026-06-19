using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace VEF.Buildings;

[HarmonyPatch]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_CompRefuelable_EjectFuelPatches
{
    public static bool patchActive = false;

    private static bool Prepare() => patchActive;

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return typeof(CompRefuelable).DeclaredMethod(nameof(CompRefuelable.EjectFuel));
        yield return typeof(CompRefuelable).DeclaredMethod(nameof(CompRefuelable.PostDestroy));
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
    {
        var matcher = new CodeMatcher(instr);

        matcher.MatchEndForward(CodeMatch.LoadsField(typeof(CompRefuelable).DeclaredField("fuel")));
        matcher.InsertAfter(
            // Load "this"
            CodeInstruction.LoadArgument(0),
            // Call our method
            CodeInstruction.Call(() => ApplyFuelMultiplier)
        );

        return matcher.Instructions();
    }

    private static float ApplyFuelMultiplier(float fuel, CompRefuelable __instance)
    {
        var extension = __instance.parent.def.GetModExtension<RefuelableExtension>();
        if (extension is { ejectingFuelRespectsFuelMultiplier: true })
            return fuel / __instance.Props.FuelMultiplierCurrentDifficulty;
        return fuel;
    }
}