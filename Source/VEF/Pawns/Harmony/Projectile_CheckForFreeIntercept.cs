using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Pawns;

[HarmonyPatch(typeof(Projectile), "CheckForFreeIntercept")]
public class VanillaExpandedFramework_Projectile_CheckForFreeIntercept_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var matcher = new CodeMatcher(instr);

        // Search for "num2 = 0.4f * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);"
        matcher.MatchEndForward(
            // Loads the value of 0.4 (base dodge)
            CodeMatch.LoadsConstant(0.4f),
            // Loads the pawn
            CodeMatch.LoadsLocal(),
            // Calls the BodySize getter
            new CodeMatch(OpCodes.Callvirt),
            // Loads 0.1 as minimum dodge chance factor from body size
            CodeMatch.LoadsConstant(0.1f),
            // Loads 2 as maximum dodge chance factor from body size
            CodeMatch.LoadsConstant(2f),
            // Calls Mathf.Clamp on the last 3 values
            new CodeMatch(OpCodes.Call),
            // Multiplies 0.4 with the body size factor
            new CodeMatch(OpCodes.Mul),
            // Stores the result in a local variable
            CodeMatch.StoresLocal()
        );

        // Move back once, before storing the value
        matcher.Advance(-1);

        // Wrap the method above with our method
        matcher.Insert(
            // Loads the pawn
            CodeInstruction.LoadLocal(7),
            // Calls our method
            CodeInstruction.Call(() => VanillaExpandedFramework_Projectile_ImpactSomething_Patch.GetHitChanceFactor),
            // Multiply the current value
            new CodeInstruction(OpCodes.Mul)
        );

        return matcher.Instructions();
    }
}