using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Comps;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Grammar;

// ReSharper disable InconsistentNaming

namespace MVCF.Harmony
{
    [HarmonyPatch]
    public class MiscPatches
    {
        public static void DoLogPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Constructor(typeof(BattleLogEntry_RangedImpact), new[]
            {
                typeof(Thing), typeof(Thing),
                typeof(Thing), typeof(ThingDef), typeof(ThingDef), typeof(ThingDef)
            }), new HarmonyMethod(typeof(MiscPatches), "FixFakeCaster"));
            harm.Patch(AccessTools.Method(typeof(PlayLogEntryUtility), "RulesForOptionalWeapon"),
                postfix: new HarmonyMethod(typeof(MiscPatches), "PlayLogEntryUtility_RulesForOptionalWeapon_Postfix"));
        }

        public static void DoAnimalPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(JobDriver_Wait), "CheckForAutoAttack"),
                transpiler: new HarmonyMethod(typeof(MiscPatches), "Transpiler_JobDriver_Wait_CheckForAutoAttack"));
        }

        public static void DoIndependentPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn), "SpawnSetup"),
                postfix: new HarmonyMethod(typeof(MiscPatches), "Postfix_Pawn_SpawnSetup"));
            harm.Patch(AccessTools.Method(typeof(Pawn), "DeSpawn"),
                postfix: new HarmonyMethod(typeof(MiscPatches), "Postfix_Pawn_DeSpawn"));
            harm.Patch(AccessTools.Method(typeof(Pawn_StanceTracker), "SetStance"),
                new HarmonyMethod(typeof(MiscPatches), "Pawn_StanceTracker_SetStance"));
        }

        public static void DoDrawPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn), "DrawAt"),
                postfix: new HarmonyMethod(typeof(MiscPatches), "Postfix_Pawn_DrawAt"));
        }

        public static void DoExtraEquipmentPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(ThingDef), "get_IsRangedWeapon"),
                new HarmonyMethod(typeof(MiscPatches), "Prefix_IsRangedWeapon"));
            harm.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddDraftedOrders"),
                transpiler: new HarmonyMethod(typeof(MiscPatches), "CheckForMelee"));
        }

        public static IEnumerable<CodeInstruction> CheckForMelee(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var idx = list.FindIndex(ins => ins.opcode == OpCodes.Brtrue);
            var label = list[idx].operand;
            list.InsertRange(idx + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), "equipment")),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_EquipmentTracker), "get_Primary")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VerbManager), "PreferMelee")),
                new CodeInstruction(OpCodes.Brtrue, label)
            });
            return list;
        }

        public static bool Prefix_IsRangedWeapon(ref bool __result, ThingDef __instance)
        {
            if (__instance.IsWeapon &&
                __instance.GetCompProperties<CompProperties_VerbProps>() is CompProperties_VerbProps props &&
                props.ConsiderMelee)
            {
                __result = false;
                return false;
            }

            return true;
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

        public static void Postfix_Pawn_SpawnSetup(Pawn __instance)
        {
            var man = __instance.Manager();
            if (man == null) return;
            if (man.NeedsTicking)
                WorldComponent_MVCF.GetComp().TickManagers.Add(new System.WeakReference<VerbManager>(man));
        }

        public static void Postfix_Pawn_DeSpawn(Pawn __instance)
        {
            var man = __instance.Manager(false);
            if (man == null) return;
            if (man.NeedsTicking)
                WorldComponent_MVCF.GetComp().TickManagers.RemoveAll(wr =>
                {
                    if (!wr.TryGetTarget(out var vm)) return true;
                    return vm == man;
                });
        }

        public static void FixFakeCaster(ref Thing initiator)
        {
            if (initiator is IFakeCaster fc) initiator = fc.RealCaster();
        }

        public static IEnumerable<Rule> PlayLogEntryUtility_RulesForOptionalWeapon_Postfix(IEnumerable<Rule> __result,
            string prefix, ThingDef weaponDef, ThingDef projectileDef)
        {
            foreach (var rule in __result) yield return rule;
            if (weaponDef != null || projectileDef == null) yield break;

            foreach (var rule in GrammarUtility.RulesForDef(prefix + "_projectile", projectileDef))
                yield return rule;
        }

        public static bool Pawn_StanceTracker_SetStance(Stance newStance)
        {
            return !(newStance is Stance_Busy busy && busy.verb?.caster is IFakeCaster);
        }
    }

    public interface IFakeCaster
    {
        Thing RealCaster();
    }
}