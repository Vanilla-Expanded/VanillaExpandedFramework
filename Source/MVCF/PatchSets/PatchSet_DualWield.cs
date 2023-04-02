using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace MVCF.PatchSets;

public class PatchSet_DualWield : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Transpiler(AccessTools.Method(AccessTools.TypeByName("DualWield.Harmony.PawnRenderer_RenderPawnAt"), "Postfix"),
            AccessTools.Method(GetType(), nameof(AddNullCheck)));
    }

    public static IEnumerable<CodeInstruction> AddNullCheck(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var info = AccessTools.Method(AccessTools.TypeByName("DualWield.Ext_Pawn"), "GetStancesOffHand");
        foreach (var instruction in instructions)
        {
            yield return instruction;
            if (instruction.Calls(info))
            {
                var label = generator.DefineLabel();
                yield return new CodeInstruction(OpCodes.Dup);
                yield return new CodeInstruction(OpCodes.Brtrue, label);
                yield return new CodeInstruction(OpCodes.Pop);
                yield return new CodeInstruction(OpCodes.Ret);
                yield return new CodeInstruction(OpCodes.Nop).WithLabels(label);
            }
        }
    }
}
