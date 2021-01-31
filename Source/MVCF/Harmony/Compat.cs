using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DualWield;
using HarmonyLib;
using MVCF.Utilities;
using Verse;
using Verse.AI;

namespace MVCF.Harmony
{
    internal class Compat
    {
        public static void ApplyCompat(HarmonyLib.Harmony harm)
        {
            if (ModLister.HasActiveModWithName("A RimWorld of Magic")) LimitedMode(harm);

            if (ModLister.HasActiveModWithName("RunAndGun"))
            {
                Log.Message("[MVCF] Applying RunAndGun compatibility patch");
                harm.Patch(Type.GetType("RunAndGun.Harmony.Verb_TryCastNextBurstShot, RunAndGun")
                        ?.GetMethod("SetStanceRunAndGun"),
                    transpiler: new HarmonyMethod(typeof(Compat), "RunAndGunSetStance"));
                harm.Patch(Type.GetType("RunAndGun.Extensions, RunAndGun")
                        ?.GetMethod("HasRangedWeapon"),
                    postfix: new HarmonyMethod(typeof(Compat), "RunAndGunHasRangedWeapon"));
            }

            if (ModLister.HasActiveModWithName("Dual Wield"))
            {
                HarmonyLib.Harmony.DEBUG = true;
                Log.Message("[MVCF] Applying Dual Wield compatibility patch");
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
            return Traverse.Create(__0).Field("pawn").GetValue<Pawn>()?.GetStancesOffHand() != null;
        }

        public static bool RenderPawnAt(PawnRenderer __0)
        {
            return Traverse.Create(__0).Field("pawn").GetValue<Pawn>()?.GetStancesOffHand() != null;
        }


        public static IEnumerable<CodeInstruction> RunAndGunSetStance(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var idx1 = list.FindIndex(ins => ins.IsLdarg(0));
            var idx2 = list.FindIndex(ins => ins.opcode == OpCodes.Ldfld && (FieldInfo) ins.operand ==
                AccessTools.Field(typeof(Pawn_StanceTracker), "curStance"));
            list.RemoveRange(idx1, idx2 - idx1 - 2);
            return list;
        }

        // ReSharper disable once InconsistentNaming
        public static void RunAndGunHasRangedWeapon(Pawn instance, ref bool __result)
        {
            if (!__result) __result = instance.Manager().ManagedVerbs.Any(mv => mv.Enabled && mv.Verb.IsMeleeAttack);
        }

        public static void LimitedMode(HarmonyLib.Harmony harm)
        {
            Log.Warning("[MVCF] Mod conflict detected, deactivating most MVCF features...");
            harm.UnpatchAll(harm.Id);
            Base.LimitedMode = true;
            harm.Patch(AccessTools.Method(typeof(Pawn), "TryGetAttackVerb"),
                new HarmonyMethod(typeof(Pawn_TryGetAttackVerb), "Prefix"));
            harm.Patch(AccessTools.Method(typeof(JobDriver_Wait), "CheckForAutoAttack"),
                new HarmonyMethod(typeof(MiscPatches), "Postfix_JobDriver_Wait_CheckForAutoAttack"));
        }
    }
}