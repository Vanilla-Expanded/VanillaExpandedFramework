using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

// ReSharper disable once CheckNamespace
namespace VFECore
{
    public static class Patch_PawnRenderer
    {
        public static bool IsShell(ApparelLayerDef def)
        {
            return def == RimWorld.ApparelLayerDefOf.Shell || def == ApparelLayerDefOf.VFEC_OuterShell;
        }

        [HarmonyPatch(typeof(PawnRenderer), "DrawBodyApparel")]
        public static class DrawBodyApparel
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
            {
                var list = instructions.ToList();
                var info1 = AccessTools.Field(typeof(RimWorld.ApparelLayerDefOf), nameof(RimWorld.ApparelLayerDefOf.Shell));
                var idx1 = list.FindIndex(ins => ins.LoadsField(info1));
                list[idx1] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer), nameof(IsShell)));
                list[idx1 + 1].opcode = OpCodes.Brfalse;
                return list;
            }
        }

        [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.MatsBodyBaseAt))]
        public static class MatsBodyBaseAt
        {
            [HarmonyTranspiler]
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