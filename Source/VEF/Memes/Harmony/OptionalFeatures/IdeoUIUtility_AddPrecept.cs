using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace VEF.Memes
{

    // This Harmony patch will only be patched if IdeoFloatMenuPlus is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_IdeoUIUtility_AddPrecept_Patch
    {

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = instructions.ToList();
            var info1 = AccessTools.Method(typeof(WindowStack), nameof(WindowStack.Add));
            var idx1 = list.FindIndex(ins => ins.Calls(info1));
            var label1 = generator.DefineLabel();
            var label2 = generator.DefineLabel();
            list[idx1].labels.Add(label1);
            list.InsertRange(idx1, new[]
            {
                new CodeInstruction(OpCodes.Br, label1),
                new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(Dialog_FloatMenuOptions), new[] {typeof(List<FloatMenuOption>)})).WithLabels(label2)
            });
            list.InsertRange(idx1 - 1, new[]
            {
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(List<FloatMenuOption>), "Count")),
                new CodeInstruction(OpCodes.Ldc_I4, 30),
                new CodeInstruction(OpCodes.Bge, label2)
            });
            return list;
        }
    }
}
