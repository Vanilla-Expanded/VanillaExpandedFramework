using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

// ReSharper disable once CheckNamespace
namespace VFECore
{
    public static class Patch_PawnRenderer
    {
        public static FastInvokeHandler CE_IsVisibleLayer;

        public static bool IsShell(ApparelLayerDef def)
        {
            return def == RimWorld.ApparelLayerDefOf.Shell || IsOuterShell(def);
        }

        public static bool IsOuterShell(ApparelLayerDef def)
        {
            return def == ApparelLayerDefOf.VFEC_OuterShell || CE_IsVisibleLayer != null && (bool) CE_IsVisibleLayer.Invoke(null, def);
        }

        [HarmonyPatch(typeof(PawnRenderer), "DrawBodyApparel")]
        public static class DrawBodyApparel
        {
            [HarmonyTranspiler]
            [HarmonyPriority(Priority.First)]
            public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var list = instructions.ToList();
                var info1 = AccessTools.Field(typeof(RimWorld.ApparelLayerDefOf), nameof(RimWorld.ApparelLayerDefOf.Shell));
                var idx1 = list.FindIndex(ins => ins.LoadsField(info1));
                if (idx1 < 0)
                {
                    Log.Warning("[VFECore] Another mod is overwriting PawnRenderer.DrawBodyApparel before we can, this will cause OuterShell items to render wrong");
                    return list;
                }

                list[idx1] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer), nameof(IsShell)));
                list[idx1 + 1].opcode = OpCodes.Brfalse;
                var idx2 = list.FindIndex(ins => ins.opcode == OpCodes.Stloc_S && ins.operand is LocalBuilder lb && lb.LocalIndex == 5);
                var idx3 = list.FindIndex(idx2, ins => ins.opcode == OpCodes.Ldloca_S && ins.operand is LocalBuilder lb && lb.LocalIndex == 5);
                var idx4 = list.FindIndex(idx3, ins => ins.opcode == OpCodes.Stind_R4);
                var list2 = list.GetRange(idx3, idx4 - idx3 + 1).Select(ins => ins.Clone()).ToList();
                var ins1 = list2.Find(ins => ins.opcode == OpCodes.Ldc_R4);
                ins1.operand = (float) ins1.operand / 2f;
                var label1 = generator.DefineLabel();
                list[idx2 + 1].labels.Add(label1);
                list.InsertRange(idx2 + 1, new[]
                {
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ApparelGraphicRecord), nameof(ApparelGraphicRecord.sourceApparel))),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Thing), nameof(Thing.def))),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ThingDef), nameof(ThingDef.apparel))),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ApparelProperties), nameof(ApparelProperties.LastLayer))),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer), nameof(IsOuterShell))),
                    new CodeInstruction(OpCodes.Brfalse_S, label1)
                }.Concat(list2));
                return list;
            }
        }

        [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.MatsBodyBaseAt))]
        public static class MatsBodyBaseAt
        {
            [HarmonyTranspiler]
            [HarmonyPriority(Priority.First)]
            public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
            {
                var list = instructions.ToList();
                var info1 = AccessTools.Field(typeof(RimWorld.ApparelLayerDefOf), nameof(RimWorld.ApparelLayerDefOf.Shell));
                var idx1 = list.FindIndex(ins => ins.LoadsField(info1));
                list[idx1] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer), nameof(IsShell)));
                list[idx1 + 1].opcode = OpCodes.Brtrue;
                return list;
            }
        }
    }
}