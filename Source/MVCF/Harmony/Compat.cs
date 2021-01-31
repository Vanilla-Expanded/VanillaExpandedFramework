using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

            if (ModLister.HasActiveModWithName("A RimWorld of Magic")) LimitedMode(harm);
        }

        public static IEnumerable<CodeInstruction> RunAndGunSetStance(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var idx1 = list.FindIndex(ins => ins.IsLdarg(0));
            var idx2 = list.FindIndex(ins => ins.opcode == OpCodes.Ldfld && (FieldInfo) ins.operand ==
                AccessTools.Field(typeof(Pawn_StanceTracker), "curStance"));
            list.RemoveRange(idx1, idx2 - idx1 - 2);
            // var idx3 = list.FindIndex(ins => ins.IsLdarg(0));
            // var idx4 = list.FindIndex(idx3 + 1, ins => ins.IsLdarg(0));
            // var idx5 = list.FindIndex(ins => ins.opcode == OpCodes.Brfalse_S);
            // var list2 = list.Skip(idx4).Take(idx5 - idx4 + 1).Select(ins => ins.Clone()).ToList();
            // list2.Find(ins => ins.opcode == OpCodes.Isinst).operand = typeof(Stance_Mobile);
            // list.InsertRange(idx5 + 1, list2);
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