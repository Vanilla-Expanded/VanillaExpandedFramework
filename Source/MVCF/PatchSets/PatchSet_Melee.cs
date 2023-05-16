using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.PatchSets;

public class PatchSet_Melee : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Transpiler(AccessTools.Method(typeof(Pawn_MeleeVerbs), nameof(Pawn_MeleeVerbs.GetUpdatedAvailableVerbsList)),
            AccessTools.Method(GetType(), nameof(AddApparelMeleeVerbs)));
    }

    public static IEnumerable<CodeInstruction> AddApparelMeleeVerbs(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var info1 = AccessTools.Field(typeof(Pawn), nameof(Pawn.apparel));
        var idx1 = codes.FindIndex(ins => ins.LoadsField(info1));
        Label? label1 = null;
        var idx2 = codes.FindIndex(idx1, ins => ins.Branches(out label1));
        if (label1 == null) return codes;
        var idx3 = codes.FindIndex(idx2, ins => ins.labels.Contains(label1.Value));
        codes.RemoveRange(idx2 + 1, idx3 - idx2 - 1);
        codes.InsertRange(idx2 + 1, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            CodeInstruction.LoadField(typeof(Pawn_MeleeVerbs), "pawn"),
            CodeInstruction.LoadField(typeof(Pawn_MeleeVerbs), "verbsToAdd"),
            CodeInstruction.Call(typeof(MeleeVerbUtility), nameof(MeleeVerbUtility.AddAdditionalMeleeVerbs))
        });
        return codes;
    }
}
