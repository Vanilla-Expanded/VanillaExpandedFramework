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
        public static Delegate GetStancesOffHand;
        public static Delegate IsOffHand;

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
                GetStancesOffHand = AccessTools.Method(Type.GetType(
                        "DualWield.Ext_Pawn, DualWield"), "GetStancesOffHand")
                    .CreateDelegate(typeof(Func<Pawn, Pawn_StanceTracker>));
                IsOffHand = AccessTools.Method(Type.GetType(
                        "DualWield.Ext_ThingWithComps, DualWield"), "IsOffHand")
                    .CreateDelegate(typeof(Func<ThingWithComps, bool>));
                harm.Patch(
                    Type.GetType("DualWield.Harmony.Pawn_RotationTracker_UpdateRotation, DualWield")
                        ?.GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static),
                    new HarmonyMethod(typeof(Compat), "UpdateRotation"));
                harm.Patch(
                    Type.GetType("DualWield.Harmony.PawnRenderer_RenderPawnAt, DualWield")
                        ?.GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static),
                    new HarmonyMethod(typeof(Compat), "RenderPawnAt"));
            }
        }

        public static bool UpdateRotation(Pawn_RotationTracker __0)
        {
            var stances = GetStancesOffHand.DynamicInvoke(Traverse.Create(__0).Field("pawn").GetValue<Pawn>());
            return stances != null;
        }

        public static bool RenderPawnAt(PawnRenderer __0)
        {
            var stances = GetStancesOffHand.DynamicInvoke(Traverse.Create(__0).Field("pawn").GetValue<Pawn>());
            return stances != null;
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
            if (!__result) __result = instance.Manager().ManagedVerbs.Any(mv => mv.Enabled && !mv.Verb.IsMeleeAttack);
        }

        public static bool RunAndGunVerbCast(ref bool __result, Verb __0)
        {
            if (!(__0.caster is IFakeCaster)) return true;
            __result = true;
            return false;
        }
    }
}