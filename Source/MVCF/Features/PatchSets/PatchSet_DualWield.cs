using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace MVCF.Features.PatchSets
{
    public class PatchSet_DualWield : PatchSet
    {
        public static MethodInfo GetStancesOffHandInfo;
        public static AccessTools.FieldRef<Pawn_RotationTracker, Pawn> RotationTrackerPawn;
        public static AccessTools.FieldRef<PawnRenderer, Pawn> RendererPawn;

        public override IEnumerable<Patch> GetPatches()
        {
            RotationTrackerPawn ??= AccessTools.FieldRefAccess<Pawn_RotationTracker, Pawn>("pawn");
            RendererPawn ??= AccessTools.FieldRefAccess<PawnRenderer, Pawn>("pawn");
            GetStancesOffHandInfo ??= AccessTools.Method(AccessTools.TypeByName("DualWield.Ext_Pawn"), "GetStancesOffHand");

            yield return Patch.Transpiler(AccessTools.TypeByName("DualWield.Harmony.Pawn_RotationTracker_UpdateRotation")?.GetMethod("Postfix", AccessTools.all),
                AccessTools.Method(GetType(), nameof(UpdateRotationTranspile)));
            yield return Patch.Transpiler(AccessTools.TypeByName("DualWield.Harmony.PawnRenderer_RenderPawnAt")?.GetMethod("Postfix", AccessTools.all),
                AccessTools.Method(GetType(), nameof(RenderPawnAtTranspile)));
        }

        public static IEnumerable<CodeInstruction> UpdateRotationTranspile(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var builder = generator.DeclareLocal(typeof(Pawn_StanceTracker));
            var list = instructions.ToList();
            var idx = list.FindIndex(ins =>
                ins.opcode == OpCodes.Call && (MethodInfo) ins.operand == GetStancesOffHandInfo);
            var label = list.FindLast(ins => ins.opcode == OpCodes.Br_S).operand;
            var list2 = new List<CodeInstruction>
            {
                new(OpCodes.Stloc, builder),
                new(OpCodes.Ldloc, builder),
                new(OpCodes.Brfalse_S, label),
                new(OpCodes.Ldloc, builder)
            };
            list.InsertRange(idx + 1, list2);
            return list;
        }

        public static IEnumerable<CodeInstruction> RenderPawnAtTranspile(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var builder = generator.DeclareLocal(typeof(Pawn_StanceTracker));
            var list = instructions.ToList();
            var idx = list.FindLastIndex(ins =>
                ins.opcode == OpCodes.Callvirt);
            var label = list.FindLast(ins => ins.opcode == OpCodes.Brfalse_S).operand;
            var list2 = new List<CodeInstruction>
            {
                new(OpCodes.Stloc, builder),
                new(OpCodes.Ldloc, builder),
                new(OpCodes.Brfalse_S, label),
                new(OpCodes.Ldloc, builder)
            };
            list.InsertRange(idx, list2);
            return list;
        }
    }
}