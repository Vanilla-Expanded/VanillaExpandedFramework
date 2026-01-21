using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Pawns;

[HarmonyPatch(typeof(ShotReport), nameof(ShotReport.GetTextReadout))]
public static class ShotReport_GetTextReadout_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var totalEstimatedHitChanceGetter = typeof(ShotReport).DeclaredPropertyGetter(nameof(ShotReport.TotalEstimatedHitChance));
        var factorFromPostureGetter = typeof(ShotReport).DeclaredPropertyGetter("FactorFromPosture");
        var targetField = typeof(ShotReport).DeclaredField("target");

        var matcher = new CodeMatcher(instr);

        matcher.MatchStartForward(
            // Ldloc 0/loads string builder
            CodeMatch.IsLdloc(),
            // Loads "this"
            CodeMatch.IsLdarg(0),
            // Calls TotalEstimatedHitChance getter
            CodeMatch.Calls(totalEstimatedHitChanceGetter),
            // Calls GenText.ToStringPercent (skip argument and accept any method so we don't need to retrieve it)
            new CodeMatch(OpCodes.Call),
            // Calls StringBuilder.AppendLine (skip argument and accept any method so we don't need to retrieve it)
            new CodeMatch(OpCodes.Callvirt),
            // Pops the string builder
            new CodeMatch(OpCodes.Pop)
        );

        // Move forward (before TotalEstimatedHitChance getter)
        matcher.Advance(3);

        matcher.Insert(
            // Load "this"
            CodeInstruction.LoadArgument(0),
            // Load target field by reference
            new CodeInstruction(OpCodes.Ldflda, targetField),
            // Call our method that will multiply total accuracy by inverse chance to dodge (10% chance to doge = 90% accuracy)
            CodeInstruction.Call(() => AddDodgeToTotalEstimate)
        );

        // Go back to start
        matcher.Start();

        matcher.MatchStartForward(
            // Loads "this"
            CodeMatch.IsLdarg(0),
            // Calls FactorFromPosture getter
            CodeMatch.Calls(factorFromPostureGetter),
            // Loads a constant of 0.9999
            CodeMatch.LoadsConstant(0.9999f),
            // Branches away
            CodeMatch.Branches()
        );

        matcher.Insert(
            // Load string builder and move labels from current instruction to it
            CodeInstruction.LoadLocal(0).MoveLabelsFrom(matcher.Instruction),
            // Load "this"
            CodeInstruction.LoadArgument(0),
            // Load target field by reference
            new CodeInstruction(OpCodes.Ldflda, targetField),
            // Call our method that will add dodge chance text
            CodeInstruction.Call(() => AddDodgeChanceText)
        );

        return matcher.Instructions();
    }

    private static float AddDodgeToTotalEstimate(float currentEstimate, ref TargetInfo target)
    {
        if (target.Thing is not { } thing)
            return currentEstimate;

        var chance = thing.GetStatValue(InternalDefOf.VEF_RangedDodgeChance);
        if (chance <= 0f)
            return currentEstimate;
        return Mathf.Clamp01(currentEstimate * (1f - chance));
    }

    private static void AddDodgeChanceText(StringBuilder builder, ref TargetInfo target)
    {
        if (target.Thing is not { } thing)
            return;

        var chance = thing.GetStatValue(InternalDefOf.VEF_RangedDodgeChance);
        if (chance > 0f)
            builder.AppendLine($"   {"VEF.RangedDodge.ShotReportReadout".Translate()}: {Mathf.Clamp01(1f - chance).ToStringPercent()}");
    }
}