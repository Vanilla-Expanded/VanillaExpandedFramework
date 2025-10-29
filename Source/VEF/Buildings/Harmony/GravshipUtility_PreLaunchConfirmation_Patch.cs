using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.GetOrbitalWarnings), MethodType.Enumerator)]
public static class GravshipUtility_PreLaunchConfirmation_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGenerator)
    {
        // Grab the HasComp<CompOxygenPusher>, which we'll search for
        var hasCompMethod = typeof(ThingCompUtility).DeclaredMethod(nameof(ThingCompUtility.HasComp)).MakeGenericMethod(typeof(CompOxygenPusher));
        // Grab our methods which we'll insert into this code
        var oxygenPusherCheck = typeof(GravshipUtility_PreLaunchConfirmation_Patch).DeclaredMethod(nameof(IsOxygenPusher));
        var heaterCheck = typeof(GravshipUtility_PreLaunchConfirmation_Patch).DeclaredMethod(nameof(IsHeater));

        // Create the code matcher
        var matcher = new CodeMatcher(instr, ilGenerator);

        // Find the "if (thing.HasComp<CompOxygenPusher>) {}" sequence,
        // set pos to the beginning of it (loading the "thing" local)
        matcher.MatchStartForward(
            CodeMatch.IsLdloc(),
            CodeMatch.Calls(hasCompMethod),
            CodeMatch.Branches()
        );

        // Get the index of the local (Thing) that was loaded, we'll re-use
        var index = matcher.Instruction.LocalIndex();

        // Advance inside if branch
        matcher.Advance(3)
            // Create a label that we'll make the original condition jump to
            .CreateLabel(out var oxygenPusherLabel)
            // Move back 1 position before the existing jump
            .Advance(-1)
            // Insert instructions
            .Insert(
                // Insert a jump ahead for the previous condition
                new CodeInstruction(OpCodes.Brtrue, oxygenPusherLabel),
                // Load the thing from the index we stored
                CodeInstruction.LoadLocal(index),
                // Call our method that checks for allowed oxygen pushers
                new CodeInstruction(OpCodes.Call, oxygenPusherCheck)
                // After that, we'll re-use the existing jump instruction
            )
            // Go back to the beginning
            .Start()
            // Match the beginning of the "if (thing is Building_Heater [...]) {}" sequence,
            // set the pos at the end of the sequence (jump instruction)
            .MatchEndForward(
                CodeMatch.IsLdloc(),
                new CodeMatch(OpCodes.Isinst, typeof(Building_Heater)),
                CodeMatch.Branches()
            )
            // Insert instructions
            .Insert(
                // Load the thing from the index we stored
                CodeInstruction.LoadLocal(index),
                // Call our method that checks for allowed heaters
                new CodeInstruction(OpCodes.Call, heaterCheck),
                // Copy the current jump instruction we're at, so when true - jump to inside of if branch
                new CodeInstruction(matcher.Instruction)
            );

        return matcher.Instructions();
    }

    private static bool IsOxygenPusher(Thing thing) => thing.def.GetModExtension<GravshipLaunchExtension>() is { isOxygenPusher: true };

    private static bool IsHeater(Thing thing) => thing.def.GetModExtension<GravshipLaunchExtension>() is { isHeater: true };
}