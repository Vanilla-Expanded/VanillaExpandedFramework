using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Utilities;
using Verse;

namespace MVCF.Harmony
{
    public class Compat
    {
        public static MethodInfo GetStancesOffHandInfo;
        public static Delegate IsOffHand;
        public static AccessTools.FieldRef<Pawn_RotationTracker, Pawn> RotationTrackerPawn;
        public static AccessTools.FieldRef<PawnRenderer, Pawn> RendererPawn;
        public static MethodInfo GetToggleComp;

        public static void ApplyCompat(HarmonyLib.Harmony harm)
        {
            if (ModLister.HasActiveModWithName("RunAndGun") && Base.Features.EnabledAtAll)
            {
                Log.Message("[MVCF] Applying RunAndGun compatibility patch");
                harm.Patch(AccessTools.Method(Type.GetType("RunAndGun.Harmony.Verb_TryCastNextBurstShot, RunAndGun"),
                        "SetStanceRunAndGun"),
                    transpiler: new HarmonyMethod(typeof(Compat), "RunAndGunSetStance"));
                harm.Patch(AccessTools.Method(Type.GetType("RunAndGun.Harmony.Verb_TryStartCastOn, RunAndGun"),
                        "Prefix"),
                    new HarmonyMethod(typeof(Compat), "RunAndGunVerbCast"));
                harm.Patch(Type.GetType("RunAndGun.Extensions, RunAndGun")
                        ?.GetMethod("HasRangedWeapon"),
                    postfix: new HarmonyMethod(typeof(Compat), "RunAndGunHasRangedWeapon"));
            }

            if (ModLister.HasActiveModWithName("Dual Wield") && Base.Features.HumanoidVerbs)
            {
                Log.Message("[MVCF] Applying Dual Wield compatibility patch");
                RotationTrackerPawn = AccessTools
                    .FieldRefAccess<Pawn_RotationTracker, Pawn>("pawn");
                RendererPawn = AccessTools.FieldRefAccess<PawnRenderer, Pawn>("pawn");
                GetStancesOffHandInfo = AccessTools.Method(Type.GetType(
                    "DualWield.Ext_Pawn, DualWield"), "GetStancesOffHand");
                IsOffHand = AccessTools.Method(Type.GetType(
                        "DualWield.Ext_ThingWithComps, DualWield"), "IsOffHand")
                    .CreateDelegate(typeof(Func<ThingWithComps, bool>));

                harm.Patch(
                    Type.GetType("DualWield.Harmony.Pawn_RotationTracker_UpdateRotation, DualWield")
                        ?.GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static),
                    transpiler: new HarmonyMethod(typeof(Compat), "UpdateRotationTranspile"));
                harm.Patch(
                    Type.GetType("DualWield.Harmony.PawnRenderer_RenderPawnAt, DualWield")
                        ?.GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static),
                    transpiler: new HarmonyMethod(typeof(Compat), "RenderPawnAtTranspile"));
            }

            var type = AccessTools.TypeByName("CompToggleFireMode.CompToggleFireMode");
            if (type != null)
                GetToggleComp = AccessTools.Method(typeof(ThingCompUtility), "TryGetComp")
                    .MakeGenericMethod(type);
        }

        public static bool ShouldIgnore(Thing thing)
        {
            return thing is ThingWithComps twc &&
                   twc.AllComps.Any(comp => comp.GetType().Name.Contains("ToggleFireMode"));
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
                new CodeInstruction(OpCodes.Stloc, builder),
                new CodeInstruction(OpCodes.Ldloc, builder),
                new CodeInstruction(OpCodes.Brfalse_S, label),
                new CodeInstruction(OpCodes.Ldloc, builder)
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
                new CodeInstruction(OpCodes.Stloc, builder),
                new CodeInstruction(OpCodes.Ldloc, builder),
                new CodeInstruction(OpCodes.Brfalse_S, label),
                new CodeInstruction(OpCodes.Ldloc, builder)
            };
            list.InsertRange(idx, list2);
            return list;
        }

        public static IEnumerable<CodeInstruction> RunAndGunSetStance(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var idx1 = list.FindIndex(ins => ins.IsLdarg(0));
            var idx2 = list.FindIndex(ins => ins.opcode == OpCodes.Ldfld && (FieldInfo) ins.operand ==
                AccessTools.Field(typeof(Pawn_StanceTracker), "curStance"));
            var label = list.Find(ins => ins.opcode == OpCodes.Br).operand;
            list.RemoveRange(idx1, idx2 - idx1 - 2);
            var idx3 = list.FindIndex(ins => ins.IsLdarg(0));
            var list2 = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_StanceTracker), "pawn")),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Stance_Busy), "verb")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Compat), "CanRunAndGun")),
                new CodeInstruction(OpCodes.Brfalse_S, label)
            };
            list.InsertRange(idx3 - 1, list2);
            return list;
        }

        public static bool CanRunAndGun(Pawn pawn, Verb verb)
        {
            if (verb.EquipmentSource == null) return true;
            if (IsOffHand == null) return true;
            return !(bool) IsOffHand.DynamicInvoke(verb.EquipmentSource);
        }

        // ReSharper disable once InconsistentNaming
        public static void RunAndGunHasRangedWeapon(Pawn instance, ref bool __result)
        {
            if (!__result) __result = instance.Manager().CurrentlyUseableRangedVerbs.Any();
        }

        public static bool RunAndGunVerbCast(ref bool __result, Verb __0)
        {
            if (!(__0.caster is IFakeCaster)) return true;
            __result = true;
            return false;
        }
    }
}