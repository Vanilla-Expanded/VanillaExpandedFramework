using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VEF;

public static class GenRadialPatches
{
    private const int Range = 200;

    public static void IncreaseRadialPatternRadiiSize()
    {
        // Return early if the radius is already the same or greater (different mod already did this?)
        if (GenRadial.RadialPattern.Length >= Range * Range * 4)
            return;

        var list = new List<(IntVec3 pos, int length)>();

        for (int i = -Range; i <= Range; i++)
        {
            for (int j = -Range; j <= Range; j++)
            {
                var vec = new IntVec3(i, 0, j);
                list.Add((vec, vec.LengthHorizontalSquared));
            }
        }

        // Anything with LengthHorizontalSquared beyond that value would
        // be over 200 range, and thus wouldn't create a circle anymore.
        var maxLength = new IntVec3(Range, 0, 0).LengthHorizontalSquared;
        list.RemoveAll(x => x.length > maxLength);

        list.Sort((a, b) =>
        {
            var num = a.length;
            var num2 = b.length;
            if (num < num2)
            {
                return -1;
            }

            return (num != num2) ? 1 : 0;
        });

        var radialPattern = new IntVec3[list.Count];
        var radii = new float[list.Count];
        // Max value is Range * Range + 1, or list[list.Count].length + 1.
        var lengthSquaredToIndex = new int[Range * Range + 1];

        for (var k = 0; k < list.Count; k++)
        {
            radialPattern[k] = list[k].pos;
            radii[k] = list[k].pos.LengthHorizontal;
        }

        // Initialize LengthSquaredToIndexArray with initial value
        for (var k = 0; k < lengthSquaredToIndex.Length; k++)
            lengthSquaredToIndex[k] = -1;

        // Go through the entire RadialPattern, grab its LengthHorizontalSquared, and assign
        // a value from an index at that length in LengthSquaredToIndexArray to the current index.
        for (var i = 0; i < radialPattern.Length; i++)
        {
            var length = radialPattern[i].LengthHorizontalSquared;
            if (lengthSquaredToIndex[length] == -1)
                lengthSquaredToIndex[length] = i;
        }

        // Go through LengthSquaredToIndexArray and fill up the gaps
        var num = 0;
        for (var i = 0; i < lengthSquaredToIndex.Length; i++)
        {
            if (lengthSquaredToIndex[i] != -1)
                num = lengthSquaredToIndex[i];
            else
                lengthSquaredToIndex[i] = num;
        }

        typeof(GenRadial).Field(nameof(GenRadial.RadialPattern)).SetValue(null, radialPattern);
        typeof(GenRadial).Field(nameof(GenRadial.RadialPatternRadii)).SetValue(null, radii);
        typeof(GenRadial).Field("LengthSquaredToIndexArray").SetValue(null, lengthSquaredToIndex);

        // VEF_Mod.harmonyInstance is still null
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            VEF_Mod.harmonyInstance.Patch(typeof(GenRadial).DeclaredMethod(nameof(GenRadial.NumCellsInRadius)),
                transpiler: new HarmonyMethod(IncreaseNumCellsInRadiusCount));
        });
    }

    private static IEnumerable<CodeInstruction> IncreaseNumCellsInRadiusCount(IEnumerable<CodeInstruction> instr)
    {
        var maxPatternRadii = ((float[])typeof(GenRadial).Field(nameof(GenRadial.RadialPatternRadii)).GetValue(null)).Length;
        var maxIndexToArray = ((int[])typeof(GenRadial).Field("LengthSquaredToIndexArray").GetValue(null)).Length - 1;

        var matcher = new CodeMatcher(instr);

        // Find the value used as RadialPatternRadii length.
        // We could just hardcode it to 20000, but if another
        // mod patches this stuff as well - our patch would fail.
        // And we run our patch anytime the radius is under 200m even if modified.
        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldc_I4),
            new CodeMatch(OpCodes.Ret)
        );
        var originalMaxPatternRadii = Convert.ToInt64(matcher.Instruction.operand);

        matcher.Reset();

        // Find the value used as LengthSquaredToIndexArray (-1) length.
        // We could just hardcode it to 6400, but if another
        // mod patches this stuff as well - our patch would fail.
        // And we run our patch anytime the radius is under 200m even if modified.
        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldc_I4),
            new CodeMatch(OpCodes.Stloc_2)
        );
        var originalMaxIndexToArray = Convert.ToInt64(matcher.Instruction.operand);

        // Only replace the value if original the current value is smaller than what we use.
        if (originalMaxPatternRadii < maxPatternRadii)
        {
            matcher.Reset();
            // Just in case, infinite loop prevention
            for (var i = 0; i < 25; i++)
            {
                matcher.MatchStartForward(CodeMatch.LoadsConstant(originalMaxPatternRadii));
                if (matcher.IsInvalid)
                    break;

                matcher.Operand = maxPatternRadii;
            }
        }

        // Only replace the value if original the current value is smaller than what we use.
        if (originalMaxIndexToArray < maxIndexToArray)
        {
            matcher.Reset();
            // Just in case, infinite loop prevention
            for (var i = 0; i < 25; i++)
            {
                matcher.MatchStartForward(CodeMatch.LoadsConstant(originalMaxIndexToArray));
                if (matcher.IsInvalid)
                    break;

                matcher.Operand = maxIndexToArray;
            }
        }

        return matcher.Instructions();
    }
}