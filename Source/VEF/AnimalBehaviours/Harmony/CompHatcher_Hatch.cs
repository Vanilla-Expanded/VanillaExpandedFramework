using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours;

[HarmonyPatch(typeof(CompHatcher), nameof(CompHatcher.Hatch))]
public static class VanillaExpandedFramework_CompHatcher_Hatch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> SetHatchlingKind(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codes = instructions.ToList();
        var generatePawn = AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn),
            new[] { typeof(PawnGenerationRequest) });
        int callIndex = codes.FindIndex(instruction => instruction.Calls(generatePawn));
        if (callIndex < 0 || callIndex != codes.FindLastIndex(instruction => instruction.Calls(generatePawn)))
        {
            Log.Warning("[VEF] Could not find the pawn generation call in CompHatcher.Hatch. Crossbred eggs will use the egg def's hatchling kind.");
            return codes;
        }

        // Hatch builds one request before the loop. Change a copy for each egg and leave
        // the original generation call, spawn logic and parent relations in place.
        var eggIndex = generator.DeclareLocal(typeof(int));
        var loadHatcher = CodeInstruction.LoadArgument(0);
        loadHatcher.labels.AddRange(codes[callIndex].labels);
        codes[callIndex].labels.Clear();
        codes.InsertRange(callIndex, new[]
        {
            loadHatcher,
            new CodeInstruction(OpCodes.Ldloca, eggIndex),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VanillaExpandedFramework_CompHatcher_Hatch), nameof(WithHatchlingKind)))
        });
        codes.InsertRange(0, new[]
        {
            new CodeInstruction(OpCodes.Ldc_I4_0),
            new CodeInstruction(OpCodes.Stloc, eggIndex)
        });
        return codes;
    }

    private static PawnGenerationRequest WithHatchlingKind(PawnGenerationRequest request, CompHatcher hatcher, ref int eggIndex)
    {
        var clutch = hatcher.parent.TryGetComp<CompCrossbredEggs>();
        if (clutch != null)
            request.KindDef = clutch.KindAt(eggIndex++, request.KindDef);
        return request;
    }
}
