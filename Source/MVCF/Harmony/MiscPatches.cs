using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

// ReSharper disable InconsistentNaming

namespace MVCF.Harmony
{
    [HarmonyPatch]
    public class MiscPatches
    {
        private static readonly MethodInfo UsableVerbMI = AccessTools.Method(typeof(BreachingUtility), "UsableVerb");

        public static void DoBasePatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(BreachingUtility), nameof(BreachingUtility.FindVerbToUseForBreaching)),
                postfix: new HarmonyMethod(typeof(MiscPatches), nameof(FindVerbToUseForBreaching)));
            harm.Patch(AccessTools.Method(typeof(SlaveRebellionUtility), "CanApplyWeaponFactor"), postfix: new HarmonyMethod(typeof(MiscPatches), nameof(CanApplyWeaponFactor)));
        }

        public static void CanApplyWeaponFactor(ref bool __result, Pawn pawn)
        {
            if (!__result && (pawn.Manager()?.AllVerbs.Except(pawn.verbTracker.AllVerbs).Any() ?? false)) __result = true;
        }

        public static void FindVerbToUseForBreaching(ref Verb __result, Pawn pawn)
        {
            if (__result == null && pawn.Manager() is VerbManager man)
                __result = man.AllVerbs.FirstOrDefault(v => (bool) UsableVerbMI.Invoke(null, new object[] {v}) && v.verbProps.ai_IsBuildingDestroyer);
        }

        public static void DoAnimalPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(JobDriver_Wait), "CheckForAutoAttack"),
                transpiler: new HarmonyMethod(typeof(MiscPatches), "Transpiler_JobDriver_Wait_CheckForAutoAttack"));
        }

        public static void DoDrawPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn), "DrawAt"),
                postfix: new HarmonyMethod(typeof(MiscPatches), "Postfix_Pawn_DrawAt"));
        }

        public static IEnumerable<CodeInstruction> Transpiler_JobDriver_Wait_CheckForAutoAttack(
            IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var method = AccessTools.Method(typeof(Thing), "get_Faction");
            var idx = list.FindLastIndex(ins => ins.opcode == OpCodes.Callvirt && (MethodInfo) ins.operand == method);
            list.RemoveRange(idx - 2, 4);
            return list;
        }

        public static void Postfix_Pawn_DrawAt(Pawn __instance, Vector3 drawLoc, bool flip = false)
        {
            __instance.Manager(false)?.DrawAt(drawLoc);
        }
    }

    public interface IFakeCaster
    {
        Thing RealCaster();
    }
}