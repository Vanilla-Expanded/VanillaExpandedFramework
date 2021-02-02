using System.Collections.Generic;
using HarmonyLib;
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
        public static void DoPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(JobDriver_Wait), "CheckForAutoAttack"),
                postfix: new HarmonyMethod(typeof(MiscPatches), "Postfix_JobDriver_Wait_CheckForAutoAttack"));
            harm.Patch(AccessTools.Method(typeof(Pawn), "DrawAt"),
                postfix: new HarmonyMethod(typeof(MiscPatches), "Postfix_Pawn_DrawAt"));
            harm.Patch(AccessTools.Method(typeof(Pawn), "SpawnSetup"),
                postfix: new HarmonyMethod(typeof(MiscPatches), "Postfix_Pawn_SpawnSetup"));
            harm.Patch(AccessTools.Method(typeof(Pawn), "DeSpawn"),
                postfix: new HarmonyMethod(typeof(MiscPatches), "Postfix_Pawn_DeSpawn"));
            harm.Patch(AccessTools.Constructor(typeof(BattleLogEntry_RangedFire), new[]
            {
                typeof(Thing),
                typeof(Thing),
                typeof(ThingDef), typeof(ThingDef), typeof(bool)
            }), new HarmonyMethod(typeof(MiscPatches), "FixFakeCaster"));
            harm.Patch(AccessTools.Constructor(typeof(BattleLogEntry_RangedImpact), new[]
            {
                typeof(Thing), typeof(Thing),
                typeof(Thing), typeof(ThingDef), typeof(ThingDef), typeof(ThingDef)
            }), new HarmonyMethod(typeof(MiscPatches), "FixFakeCaster"));
            harm.Patch(AccessTools.Method(typeof(PlayLogEntryUtility), "RulesForOptionalWeapon"),
                postfix: new HarmonyMethod(typeof(MiscPatches), "PlayLogEntryUtility_RulesForOptionalWeapon_Postfix"));
            harm.Patch(AccessTools.Method(typeof(Pawn_StanceTracker), "SetStance"),
                new HarmonyMethod(typeof(MiscPatches), "Pawn_StanceTracker_SetStance"));
        }

        public static void Postfix_JobDriver_Wait_CheckForAutoAttack(JobDriver_Wait __instance)
        {
            if (__instance.pawn.Downed ||
                __instance.pawn.stances.FullBodyBusy ||
                !__instance.pawn.RaceProps.Animal ||
                !__instance.job.canUseRangedWeapon ||
                __instance.job.def != JobDefOf.Wait_Combat)
                return;
            var currentEffectiveVerb = __instance.pawn.CurrentEffectiveVerb;
            if (currentEffectiveVerb == null || currentEffectiveVerb.verbProps.IsMeleeAttack)
                return;
            var flags = TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            if (currentEffectiveVerb.IsIncendiary())
                flags |= TargetScanFlags.NeedNonBurning;
            var thing = (Thing) AttackTargetFinder.BestShootTargetFromCurrentPosition(__instance.pawn, flags);
            if (thing == null)
                return;
            __instance.pawn.TryStartAttack((LocalTargetInfo) thing);
            __instance.collideWithPawns = true;
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
            return !(newStance is Stance_Busy busy && busy.verb.caster is IFakeCaster);
        }
    }

    public interface IFakeCaster
    {
        Thing RealCaster();
    }
}