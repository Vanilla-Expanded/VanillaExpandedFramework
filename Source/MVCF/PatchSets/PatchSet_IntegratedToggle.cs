using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Utilities;
using Verse;

namespace MVCF.PatchSets;

public class PatchSet_IntegratedToggle : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Transpiler(AccessTools.Method(typeof(Command), "GizmoOnGUIInt"), AccessTools.Method(GetType(), nameof(GizmoOnGUI_Transpile)));
    }

    public static IEnumerable<CodeInstruction> GizmoOnGUI_Transpile(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var list = instructions.ToList();
        var field = AccessTools.Field(typeof(GizmoGridDrawer), "customActivator");
        var idx = list.FindIndex(ins => ins.LoadsField(field));
        var method = AccessTools.Method(typeof(Widgets), "ButtonInvisible");
        var label = list[list.FindIndex(ins => ins.Calls(method)) + 1].operand;
        var list2 = new List<CodeInstruction>
        {
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldarg_1),
            new(OpCodes.Ldarg_2),
            new(OpCodes.Call, AccessTools.Method(typeof(DrawUtility), nameof(DrawUtility.DrawToggle))),
            new(OpCodes.Brtrue_S, label)
        };
        list2[0].labels = list[idx].labels.ListFullCopy();
        list[idx].labels.Clear();
        list.InsertRange(idx, list2);
        return list;
    }
}
